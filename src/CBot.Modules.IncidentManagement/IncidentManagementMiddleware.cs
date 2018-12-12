using System;
using System.Collections.Generic;
using System.Linq;

using CBot.Messaging.Domain;
using CBot.Middleware.Domain;
using CBot.Middleware.Domain.Handlers;
using CBot.Modules.IncidentManagement.Models;

namespace CBot.Modules.IncidentManagement
{
	internal class IncidentManagementMiddleware : MiddlewareBase
	{
		private readonly IManageIncidents incidentManager;

		private readonly ModuleConfiguration configuration;

		private readonly string declareIncidentCommand = $"new {Parameters.Incident}";

		private readonly string mitigatedIncidentCommand = $"{Parameters.Incident} mitigated";

		private static readonly string IncidentPostmortemCommand = $"{Parameters.Incident} postmortem";

		private readonly string closeIncidentCommand = $"close {Parameters.Incident}";

		private readonly string forceCloseIncidentCommand = $"force close {Parameters.Incident}";

		private readonly string deleteIncidentCommand = $"delete {Parameters.Incident}";

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
											Description = $"Declares a new incident. I.e {this.DeclareIncidentExample()}",
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
											Handlers = StartsWithHandler.For(IncidentPostmortemCommand),
											EvaluatorFunc = this.AddPostmortemToIncidentHandler,
											Description = $"Adds the postmortem link to the incident associated with current channel. Ie. {AddPostmortemExample}",
											VisibleInHelp = true
										},
										new HandlerMapping
										{
											Handlers = ExactMatchHandler.For($"{this.closeIncidentCommand}"),
											EvaluatorFunc = this.CloseIncidentHandler,
											Description = "Close the incident associated with current channel.",
											VisibleInHelp = true
										},
										new HandlerMapping
										{
											Handlers = ExactMatchHandler.For($"{this.forceCloseIncidentCommand}"),
											EvaluatorFunc = this.ForceCloseIncidentHandler,
											Description = "Force closes the incident associated with current channel.",
											VisibleInHelp = false
										},
										new HandlerMapping
										{
											Handlers = ExactMatchHandler.For($"{this.deleteIncidentCommand}"),
											EvaluatorFunc = this.DeleteIncidentHandler,
											Description = "Deletes the incident associated with current channel.",
											VisibleInHelp = false
										},
										new HandlerMapping
										{
											Handlers = ExactMatchHandler.For($"active {Parameters.Incident}s"),
											EvaluatorFunc = this.ActiveIncidentsHandler,
											Description = "Lists active incidents.",
											VisibleInHelp = true
										},
										new HandlerMapping
										{
											Handlers = ExactMatchHandler.For($"recent {Parameters.Incident}s"),
											EvaluatorFunc = this.RecentIncidentsHandler,
											Description = "Lists recent incidents.",
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
			if (incidentRequest.OperationStatus == OperationStatus.NoWarroomAvailable)
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
				case OperationStatus.Success:
					yield return incomingMessage.ReplyToChannel(
						$"Incident #{incidentRequest.Incident.FriendlyId} successfully marked as mitigated. Please add the postmortem if not already done. I.e {AddPostmortemExample}\n"
						+ $"To create a new postmortem using the template, please go here { this.configuration.PostmortemTemplateLink }.");
					break;
				case OperationStatus.IncidentAlreadyResolved:
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
				yield return incomingMessage.ReplyToChannel($"Please provide the postmortem link. I.e `{AddPostmortemExample}`");
				yield break;
			}

			var postmortemLink = GetIncidentText(IncidentPostmortemCommand, incomingMessage.TargetedText);

			var incidentRequest = this.incidentManager
				.AddPostmortemToIncident(postmortemLink, incomingMessage.Username, incomingMessage.Channel).GetAwaiter()
				.GetResult();

			switch (incidentRequest.OperationStatus)
			{
				case OperationStatus.Success:
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
				case OperationStatus.Success:
					yield return incomingMessage.ReplyToChannel(
						$"Incident #{incidentRequest.Incident.FriendlyId} has been successfully closed. Please the ensure the postmortem is complete ({incidentRequest.Incident.PostmortermLink}).\n" 
						+ "This channel will now be marked as available ready for the next incident.");
					break;
				case OperationStatus.IncidentNotResolved:
					yield return incomingMessage.ReplyToChannel("The incident cannot be closed as it has not been marked as mitigated and postmortem is missing.");
					break;
				case OperationStatus.IncidentMissingPostmortem:
					yield return incomingMessage.ReplyToChannel("The incident cannot be closed as no postmortem has been added.");
					break;
				default:
					yield return incomingMessage.ReplyToChannel("No open incident was found associated to this channel.");
					break;
			}
		}

		private IEnumerable<ResponseMessage> ForceCloseIncidentHandler(IncomingMessage incomingMessage, IHandler matchedHandle)
		{
			yield return incomingMessage.IndicateTypingOnChannel();

			var incidentRequest = this.incidentManager.ForceCloseIncident(incomingMessage.Username, incomingMessage.Channel).GetAwaiter()
				.GetResult();

			switch (incidentRequest.OperationStatus)
			{
				case OperationStatus.Success:
					yield return incomingMessage.ReplyToChannel(
						$"Incident #{incidentRequest.Incident.FriendlyId} has been successfully been forced closed. Please the ensure the postmortem is complete if applicable ({incidentRequest.Incident.PostmortermLink}).\n" 
						+ "This channel will now be marked as available ready for the next incident.");
					break;
				default:
					yield return incomingMessage.ReplyToChannel("No open incident was found associated to this channel.");
					break;
			}
		}

		private IEnumerable<ResponseMessage> DeleteIncidentHandler(IncomingMessage incomingMessage, IHandler matchedHandle)
		{
			yield return incomingMessage.IndicateTypingOnChannel();

			var incidentRequest = this.incidentManager.DeleteIncident(incomingMessage.Username, incomingMessage.Channel).GetAwaiter()
				.GetResult();

			switch (incidentRequest.OperationStatus)
			{
				case OperationStatus.Success:
					yield return incomingMessage.ReplyToChannel(
						$"Incident #{incidentRequest.Incident.FriendlyId} has been successfully been deleted. " 
						+ "This channel will now be marked as available ready for the next incident.");
					break;
				default:
					yield return incomingMessage.ReplyToChannel("No open incident was found associated to this channel.");
					break;
			}
		}

		private IEnumerable<ResponseMessage> ActiveIncidentsHandler(IncomingMessage incomingMessage, IHandler matchedHandle)
		{
			yield return incomingMessage.IndicateTypingOnChannel();

			var activeIncidents = this.incidentManager.GetActiveIncidents().GetAwaiter().GetResult()
				.OrderBy(x => x.DeclaredDateTimeUtc).ToList();

			if (activeIncidents.Any())
			{
				yield return incomingMessage.ReplyToChannel(
					$"There are {activeIncidents.Count} incident(s) currently open:",
					GetIncidentAttachments(activeIncidents));
			}
			else
			{
				yield return incomingMessage.ReplyToChannel(
					$"Great news! There's no active incidents. If you need to declare a new incident, run { this.DeclareIncidentExample() }.");
			}
		}

		private IEnumerable<ResponseMessage> RecentIncidentsHandler(IncomingMessage incomingMessage, IHandler matchedHandle)
		{
			yield return incomingMessage.IndicateTypingOnChannel();

			var recentIncidents = this.incidentManager.GetRecentIncidents(pastDays: 10).GetAwaiter().GetResult()
				.OrderBy(x => x.DeclaredDateTimeUtc).ToList();

			if (recentIncidents.Any())
			{
				yield return incomingMessage.ReplyToChannel(
					$"There have been {recentIncidents.Count} recent incident(s):",
					GetIncidentAttachments(recentIncidents));
			}
			else
			{
				yield return incomingMessage.ReplyToChannel(
					$"Great news! There's no recent incidents. If you need to declare a new incident, run { this.DeclareIncidentExample() }.");
			}
		}

		private static List<Attachment> GetIncidentAttachments(List<Incident> incidents)
		{
			var attachments = new List<Attachment>();

			foreach (var incident in incidents)
			{
				var attachmentFields = new List<AttachmentField>();
				attachmentFields.AddRange(AttachmentGenerator.GetCoreAttachmentFields(incident));

				var incidentNotificationColor = Parameters.UnresolvedIncidentColor;

				if (incident.Resolved)
				{
					incidentNotificationColor = Parameters.ResolvedIncidentColor;
					attachmentFields.AddRange(AttachmentGenerator.GetResolvedAttachmentFields(incident));
				}

				if (incident.PostmortemAdded)
				{
					incidentNotificationColor = Parameters.PostmortemIncidentColor;
					attachmentFields.AddRange(AttachmentGenerator.GetPostmortemAttachmentFields(incident));
				}

				if (incident.Closed)
				{
					incidentNotificationColor = Parameters.ClosedIncidentColor;
					attachmentFields.AddRange(AttachmentGenerator.GetClosedAttachmentFields(incident));
				}

				attachments.Add(
					new Attachment
					{
						AttachmentFields = attachmentFields,
						Color = incidentNotificationColor,
						Title = $"INCIDENT #{incident.FriendlyId}"
					});
			}

			return attachments;
		}

		private static bool IncidentCommandWellFormatted(string message) => message.Split(" ", StringSplitOptions.RemoveEmptyEntries).Length >= 3;

		private static string GetIncidentText(string commandPrefix, string message) => message.Replace(commandPrefix, string.Empty).Trim();

		private string DeclareIncidentExample() => $"`@{{bot}} {this.declareIncidentCommand} Server is on fire`";

		private static string AddPostmortemExample => $"`@{{bot}} {IncidentPostmortemCommand} https://mywebsite/postmortem/101`";
	}
}
