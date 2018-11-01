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

		public IncidentManagementMiddleware(IManageIncidents incidentManager, ModuleConfiguration configuration)
		{
			this.incidentManager = incidentManager;
			this.configuration = configuration;

			this.HandlerMappings = new HandlerMapping[]
									{
										new HandlerMapping
										{
											Handlers = StartsWithHandler.For($"new { Parameters.Incident }"),
											EvaluatorFunc = this.DeclareNewIncident,
											Description = "Declares a new incident.", // TODO: ADD EXAMPLE TO HELP TEXT
											VisibleInHelp = true
										},
										new HandlerMapping
										{
											Handlers = ExactMatchHandler.For($"mitigated { Parameters.Incident }"),
											EvaluatorFunc = this.MitigateIncidentHandler,
											Description = $"Resolve the incident associated with current channel. {this.resolveIncidentHelpText}",
											VisibleInHelp = true
										},
										new HandlerMapping
										{
											Handlers = StartsWithHandler.For($"{ Parameters.Incident } postmortem"),
											EvaluatorFunc = this.AddPostmortemToIncidentHandler,
											Description = $"Adds the postmortem link to the incident associated with current channel. {this.postmortemIncidentHelpText}",
											VisibleInHelp = true
										},
										new HandlerMapping
										{
											Handlers = ExactMatchHandler.For($"close { Parameters.Incident }"),
											EvaluatorFunc = this.CloseIncidentHandler,
											Description = $"Close the incident associated with current channel. {this.closeIncidentHelpText}",
											VisibleInHelp = true
										},
									};
		}

		private IEnumerable<ResponseMessage> DeclareNewIncident(IncomingMessage incomingMessage, IHandler matchedHandle)
		{
			yield return incomingMessage.IndicateTypingOnChannel();

			if (!IncidentCommandWellFormatted(incomingMessage.TargetedText))
			{
				yield return incomingMessage.ReplyToChannel($"Sorry, you must provide a title in order to declare an incident.");
				yield break;
			}

			// TODO: SHOULD new incident be in a var or const?
			var incidentText = GetIncidentText($"new {Parameters.Incident}", incomingMessage.TargetedText);

			// TODO: AWAIT THIS?
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
						$"Incident #{incidentRequest.Incident.FriendlyId} successfully marked as mitigated. Please run {this.postmortemIncidentHelpText} followed "
						+ $"by the postmortem link to add the postmortem to this incident, if not already done. "
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
				// TODO: TWEAK THE TEXT SLIGHTLY
				yield return incomingMessage.ReplyToChannel($"Please provide the postmortem link. Help: {this.postmortemIncidentHelpText}");
			}

			// TODO: SHOULD new incident be in a var or const?
			var postmortemLink = GetIncidentText($"{Parameters.Incident} postmortem", incomingMessage.TargetedText);

			var incidentRequest = this.incidentManager
				.AddPostmortemToIncident(postmortemLink, incomingMessage.Username, incomingMessage.Channel).GetAwaiter()
				.GetResult();

			switch (incidentRequest.OperationStatus)
			{
				case IncidentOperationStatus.Success:
					yield return incomingMessage.ReplyToChannel(
						$"Incident #{incidentRequest.Incident.FriendlyId} has been successfully updated with the postmortem link. "
						+ $"Once the postmortem and incident are both complete, please run {this.closeIncidentHelpText} to close the incident.");
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
						$"Incident #{incidentRequest.Incident.FriendlyId} has been successfully closed. Please the ensure the postmortem is complete. This channel will now be marked as available.");
					break;
				case IncidentOperationStatus.IncidentNotResolved:
					yield return incomingMessage.ReplyToChannel("The incident cannot be closed as the incident has not been marked as mitigated and no postmortem has been recorded.");
					break;
				case IncidentOperationStatus.IncidentMissingPostmortem:
					yield return incomingMessage.ReplyToChannel("The incident cannot be closed as no postmortem has been recorded.");
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
	}
}
