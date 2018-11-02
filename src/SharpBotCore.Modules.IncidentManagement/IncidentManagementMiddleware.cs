using System;
using System.Collections.Generic;

using SharpBotCore.Messaging.Domain;
using SharpBotCore.Middleware.Domain;
using SharpBotCore.Middleware.Domain.Handlers;

namespace SharpBotCore.Modules.IncidentManagement
{
	internal class IncidentManagementMiddleware : MiddlewareBase
	{
		private readonly IManageIncidents incidentManager;

		private readonly ModuleConfiguration configuration;

		private readonly string declareIncidentCommand = $"new {Parameters.Incident}";

		private readonly string mitigatedIncidentCommand = $"{Parameters.Incident} mitigated";

		private static readonly string IncidentPostmortemCommand = $"{Parameters.Incident} postmortem";

		private readonly string closeIncidentCommand = $"close {Parameters.Incident}";

		public IncidentManagementMiddleware(IManageIncidents incidentManager, ModuleConfiguration configuration)
		{
			this.incidentManager = incidentManager;
			this.configuration = configuration;

			this.HandlerMappings = new[]
									{
										new HandlerMapping
										{
											Handlers = StartsWithHandler.For(this.declareIncidentCommand),
											EvaluatorFunc = this.DeclareNewIncident,
											Description = $"Declares a new incident. I.e @{{bot}} {this.declareIncidentCommand} Server is on fire",
											VisibleInHelp = true
											
										},
										new HandlerMapping
										{
											Handlers = ExactMatchHandler.For(this.mitigatedIncidentCommand),
											EvaluatorFunc = this.MitigateIncidentHandler,
											Description = "Marks the open incident associated with current channel as mitigated.",
											VisibleInHelp = true
										},
										new HandlerMapping
										{
											Handlers = StartsWithHandler.For($"{IncidentPostmortemCommand}"),
											EvaluatorFunc = this.AddPostmortemToIncidentHandler,
											Description = $"Adds the postmortem link to the incident associated with current channel. Ie. {GetAddPostmortemExample()}",
											VisibleInHelp = true
										},
										new HandlerMapping
										{
											Handlers = ExactMatchHandler.For($"{this.closeIncidentCommand}"),
											EvaluatorFunc = this.CloseIncidentHandler,
											Description = "Close the incident associated with current channel.",
											VisibleInHelp = true
										},
									};
		}

		private IEnumerable<ResponseMessage> DeclareNewIncident(IncomingMessage incomingMessage, IHandler matchedHandle)
		{
			yield return incomingMessage.IndicateTypingOnChannel();

			if (!IncidentCommandWellFormatted(incomingMessage.TargetedText))
			{
				yield return incomingMessage.ReplyToChannel("Sorry, you must provide a title in order to declare an incident.");
				yield break;
			}

			var incidentText = GetIncidentText(this.declareIncidentCommand, incomingMessage.TargetedText);

			var incidentRequest = this.incidentManager.DeclareNewIncident(incidentText, incomingMessage.Username)
				.GetAwaiter().GetResult();
			if (incidentRequest.OperationStatus == IncidentOperationStatus.NoWarroomAvailable)
			{
				yield return
					incomingMessage
						.ReplyToChannel("Sorry, no warrooms are available so this incident cannot be declare at the moment.");
			}
			else
			{
				yield return incomingMessage.ReplyToChannel(
					$"Incident #{incidentRequest.Incident.FriendlyId} has been declared and bound to #{incidentRequest.Incident.ChannelName} channel.");
			}
		}

		private IEnumerable<ResponseMessage> MitigateIncidentHandler(IncomingMessage incomingMessage, IHandler matchedHandle)
		{
			yield return incomingMessage.IndicateTypingOnChannel();

			var incidentRequest = this.incidentManager.ResolveIncident(incomingMessage.Username, incomingMessage.Channel)
				.GetAwaiter().GetResult();

			switch (incidentRequest.OperationStatus)
			{
				case IncidentOperationStatus.Success:
					yield return incomingMessage.ReplyToChannel(
						$"Incident #{incidentRequest.Incident.FriendlyId} successfully marked as mitigated. Please add the postmortem if not already done. I.e {GetAddPostmortemExample()}\n"
						+ $"To create a new postmortem using the template, please go here { this.configuration.PostmortemTemplateLink }.");
					break;
				case IncidentOperationStatus.IncidentAlreadyResolved:
					yield return incomingMessage.ReplyToChannel(
						"The open incident linked to this channel has already been marked as mitigated.");
					break;
				default:
					yield return incomingMessage.ReplyToChannel("No open incident was found associated to this channel.");
					break;
			}
		}

		private IEnumerable<ResponseMessage> AddPostmortemToIncidentHandler(IncomingMessage incomingMessage, IHandler matchedHandle)
		{
			yield return incomingMessage.IndicateTypingOnChannel();

			if (!IncidentCommandWellFormatted(incomingMessage.TargetedText))
			{
				yield return incomingMessage.ReplyToChannel($"Please provide the postmortem link. I.e `{GetAddPostmortemExample()}`");
			}

			var postmortemLink = GetIncidentText(IncidentPostmortemCommand, incomingMessage.TargetedText);

			var incidentRequest = this.incidentManager
				.AddPostmortemToIncident(postmortemLink, incomingMessage.Username, incomingMessage.Channel).GetAwaiter()
				.GetResult();

			switch (incidentRequest.OperationStatus)
			{
				case IncidentOperationStatus.Success:
					yield return incomingMessage.ReplyToChannel(
						$"Incident #{incidentRequest.Incident.FriendlyId} has been successfully updated with the postmortem link. "
						+ $"Once the postmortem and incident are both complete, please run `@{{bot}} {this.closeIncidentCommand}` to close the incident.");
					break;
				default:
					yield return incomingMessage.ReplyToChannel("No open incident was found associated to this channel.");
					break;
			}
		}

		private IEnumerable<ResponseMessage> CloseIncidentHandler(IncomingMessage incomingMessage, IHandler matchedHandle)
		{
			yield return incomingMessage.IndicateTypingOnChannel();

			var incidentRequest = this.incidentManager.CloseIncident(incomingMessage.Username, incomingMessage.Channel).GetAwaiter()
				.GetResult();

			switch (incidentRequest.OperationStatus)
			{
				case IncidentOperationStatus.Success:
					yield return incomingMessage.ReplyToChannel(
						$"Incident #{incidentRequest.Incident.FriendlyId} has been successfully closed. Please the ensure the postmortem is complete ({incidentRequest.Incident.PostmortermLink}).\n" 
						+ "This channel will now be marked as available ready for the next incident.");
					break;
				case IncidentOperationStatus.IncidentNotResolved:
					yield return incomingMessage.ReplyToChannel("The incident cannot be closed as it has not been marked as mitigated and postmortem is missing.");
					break;
				case IncidentOperationStatus.IncidentMissingPostmortem:
					yield return incomingMessage.ReplyToChannel("The incident cannot be closed as no postmortem has been added.");
					break;
				default:
					yield return incomingMessage.ReplyToChannel("No open incident was found associated to this channel.");
					break;
			}
		}

		private static bool IncidentCommandWellFormatted(string message)
		{
			return message.Split(" ", StringSplitOptions.RemoveEmptyEntries).Length >= 3;
		}

		private static string GetIncidentText(string commandPrefix, string message)
		{
			return message.Replace(commandPrefix, string.Empty).Trim();
		}

		private static string GetAddPostmortemExample() => $"@{{bot}} {IncidentPostmortemCommand} https://mywebsite/postmortem/101";
	}
}
