using System;
using System.Collections.Generic;
using System.Linq;

using CBot.Messaging.Domain;
using CBot.Middleware.Domain;
using CBot.Middleware.Domain.Handlers;
using CBot.Modules.NewRelic.API.Models;

using MoreLinq;

namespace CBot.Modules.NewRelic
{
	public class NewRelicMiddleware : IMiddleware
	{
		public IEnumerable<HandlerMapping> HandlerMappings { get; }

		private readonly IManageNewRelic newRelicManager;

		private static readonly string ApplicationDetailCommand = $"{Parameters.NewRelic} application detail";

		public NewRelicMiddleware(IManageNewRelic newRelicManager)
		{
			this.newRelicManager = newRelicManager;

			this.HandlerMappings = new[]
									{
										new HandlerMapping
										{
											Handlers = ExactMatchHandler.For($"{Parameters.NewRelic} applications"),
											EvaluatorFunc = this.AllApplicationsHandler,
											Description = "Gets all applications from NewRelic.",
											VisibleInHelp = true
										},
										new HandlerMapping
										{
											Handlers =
												ExactMatchHandler.For($"{Parameters.NewRelic} applications detail"),
											EvaluatorFunc = this.ApplicationsDetailHandler,
											Description =
												"Gets all applications from NewRelic with detailed health information.",
											VisibleInHelp = true
										},
										new HandlerMapping
										{
											Handlers =
												ExactMatchHandler.For($"unhealthy {Parameters.NewRelic} applications"),
											EvaluatorFunc = this.UnhealthyApplicationsHandler,
											Description =
												"Gets all unhealthy applications from NewRelic including detailed health information.",
											VisibleInHelp = true
										},
										new HandlerMapping
										{
											Handlers =
												StartsWithHandler.For(ApplicationDetailCommand),
											EvaluatorFunc = this.ApplicationDetailFilteredHandler,
											Description =
												$"Gets application detail for one or more applications from NewRelic based on pattern. I.e {GetApplicationDetailExample}",
											VisibleInHelp = true
										},
									};
		}

		private IEnumerable<ResponseMessage> ApplicationDetailFilteredHandler(
			IncomingMessage incomingMessage,
			IHandler matchedHandle)
		{
			incomingMessage.IndicateTypingOnChannel();

			if (ApplicationDetailTargetedCommandMisformed(incomingMessage.TargetedText))
			{
				yield return incomingMessage.ReplyToChannel($"Please provide a search term. I.e {GetApplicationDetailExample}");
				yield break;
			}

			var searchTerm = incomingMessage.TargetedText.Split(" ", StringSplitOptions.RemoveEmptyEntries)[3];
			var newRelicApplicationsRelicResponse = this.newRelicManager
				.GetApplicationsLikeName(searchTerm).GetAwaiter().GetResult();

			if (newRelicApplicationsRelicResponse.OperationStatus == OperationStatus.Success
				&& !newRelicApplicationsRelicResponse.Applications.Any())
			{
				yield return incomingMessage.ReplyToChannel($"Sorry, no applications were found in NewRelic for search term `{searchTerm}`.");
				yield break;
			}

			if (newRelicApplicationsRelicResponse.OperationStatus == OperationStatus.Success)
			{
				var attachments = GenerateDetailedAttachments(newRelicApplicationsRelicResponse.Applications).ToList();

				foreach (var responseMessage in ChuckAttachmentsAndReplyToChannel(
					incomingMessage,
					attachments,
					$"There are {attachments.Count} matching NewRelic application(s):")) yield return responseMessage;
				yield break;
			}

			yield return incomingMessage.ReplyToChannel(
				"Sorry, something went wrong with getting applications from NewRelic.");
		}

		private IEnumerable<ResponseMessage> UnhealthyApplicationsHandler(
			IncomingMessage incomingMessage,
			IHandler matchedHandle)
		{
			incomingMessage.IndicateTypingOnChannel();

			var newRelicApplicationsRelicResponse =
				this.newRelicManager.GetUnhealthyApplications().GetAwaiter().GetResult();

			if (newRelicApplicationsRelicResponse.OperationStatus == OperationStatus.Success
				&& !newRelicApplicationsRelicResponse.Applications.Any())
			{
				yield return incomingMessage.ReplyToChannel("Great news, all applications in NewRelic are health!");
				yield break;
			}

			if (newRelicApplicationsRelicResponse.OperationStatus == OperationStatus.Success)
			{
				var attachments = GenerateDetailedAttachments(newRelicApplicationsRelicResponse.Applications).ToList();

				foreach (var responseMessage in ChuckAttachmentsAndReplyToChannel(
					incomingMessage,
					attachments,
					$"There are {attachments.Count} unhealthy NewRelic application(s):")) yield return responseMessage;
				yield break;
			}

			yield return incomingMessage.ReplyToChannel(
				"Sorry, something went wrong with getting unhealthy applications from NewRelic.");
		}

		private IEnumerable<ResponseMessage> ApplicationsDetailHandler(
			IncomingMessage incomingMessage,
			IHandler matchedHandle)
		{
			incomingMessage.IndicateTypingOnChannel();

			var newRelicApplicationsRelicResponse = this.newRelicManager.GetAllApplications().GetAwaiter().GetResult();

			if (newRelicApplicationsRelicResponse.OperationStatus == OperationStatus.Success
				&& !newRelicApplicationsRelicResponse.Applications.Any())
			{
				yield return incomingMessage.ReplyToChannel("Sorry, no applications were found in NewRelic.");
				yield break;
			}

			if (newRelicApplicationsRelicResponse.OperationStatus == OperationStatus.Success)
			{
				var attachments = GenerateDetailedAttachments(newRelicApplicationsRelicResponse.Applications).ToList();

				foreach (var responseMessage in ChuckAttachmentsAndReplyToChannel(
					incomingMessage,
					attachments,
					$"There are {attachments.Count} NewRelic Application(s):")) yield return responseMessage;
				yield break;
			}

			yield return incomingMessage.ReplyToChannel(
				"Sorry, something went wrong with getting applications from NewRelic.");
		}

		private IEnumerable<ResponseMessage> AllApplicationsHandler(
			IncomingMessage incomingMessage,
			IHandler matchedHandle)
		{
			incomingMessage.IndicateTypingOnChannel();

			var newRelicApplicationsRelicResponse = this.newRelicManager.GetAllApplications().GetAwaiter().GetResult();

			if (newRelicApplicationsRelicResponse.OperationStatus == OperationStatus.Success
				&& !newRelicApplicationsRelicResponse.Applications.Any())
			{
				yield return incomingMessage.ReplyToChannel("Sorry, no applications were found in NewRelic.");
				yield break;
			}

			if (newRelicApplicationsRelicResponse.OperationStatus == OperationStatus.Success)
			{
				var attachments = GenerateSummaryAttachments(newRelicApplicationsRelicResponse.Applications).ToList();

				foreach (var responseMessage in ChuckAttachmentsAndReplyToChannel(
					incomingMessage,
					attachments,
					$"There are {attachments.Count} NewRelic Application(s):")) yield return responseMessage;
				yield break;
			}

			yield return incomingMessage.ReplyToChannel(
				"Sorry, something went wrong with getting applications from NewRelic.");
		}

		private static IEnumerable<Attachment> GenerateSummaryAttachments(IEnumerable<Application> applications)
		{
			var applicationAttachments = new List<Attachment>();

			foreach (var application in applications)
			{
				applicationAttachments.Add(
					new Attachment
					{
						AttachmentFields = GetCoreAttachmentFields(application),
						Color = GetAttachmentColourForHealth(application.HealthStatus)
					});
			}

			return applicationAttachments;
		}

		private static IEnumerable<Attachment> GenerateDetailedAttachments(IEnumerable<Application> applications)
		{
			var applicationAttachments = new List<Attachment>();

			foreach (var application in applications)
			{
				var attachmentFields = GetCoreAttachmentFields(application);
				attachmentFields.AddRange(GetDetailedAttachmentFields(application));

				applicationAttachments.Add(
					new Attachment
					{
						AttachmentFields = attachmentFields,
						Color = GetAttachmentColourForHealth(application.HealthStatus)
					});
			}

			return applicationAttachments;
		}

		private static List<AttachmentField> GetCoreAttachmentFields(Application application)
		{
			return new List<AttachmentField>
					{
						new AttachmentField
						{
							IsShort = true, Title = nameof(application.Id), Value = application.Id.ToString()
						},
						new AttachmentField
						{
							IsShort = true, Title = nameof(application.Name), Value = application.Name
						},
						new AttachmentField
						{
							IsShort = true, Title = nameof(application.Reporting), Value = application.Reporting.ToString()
						},
						new AttachmentField
						{
							IsShort = true, Title = nameof(application.HealthStatus), Value = application.HealthStatus
						}
					};
		}

		private static List<AttachmentField> GetDetailedAttachmentFields(Application application)
		{
			return new List<AttachmentField>
					{
						new AttachmentField
						{
							IsShort = true, Title = nameof(application.Language), Value = application.Language
						},
						new AttachmentField
						{
							IsShort = true,
							Title = nameof(application.LastReportedAt),
							Value = $"{application.LastReportedAt.UtcDateTime} UTC"
						},
						new AttachmentField
						{
							IsShort = true,
							Title = nameof(application.ApplicationSummary.ErrorRate),
							Value = $"{application.ApplicationSummary?.ErrorRate * 100}%"
						},
						new AttachmentField
						{
							IsShort = true,
							Title = nameof(application.ApplicationSummary.ResponseTime),
							Value = $"{application.ApplicationSummary?.ResponseTime}ms"
						},
						new AttachmentField
						{
							IsShort = true,
							Title = nameof(application.ApplicationSummary.ApdexScore),
							Value = $"{application.ApplicationSummary?.ApdexScore} / {application.ApplicationSummary?.ApdexTarget} target"
						},
						new AttachmentField
						{
							IsShort = true,
							Title =
								$"{nameof(application.EndUserSummary.ApdexScore)} {nameof(application.EndUserSummary)}",
							Value =
								$"{application.EndUserSummary?.ApdexScore} / {application.EndUserSummary?.ApdexTarget} target"
						},
						new AttachmentField
						{
							IsShort = true,
							Title = nameof(application.ApplicationSummary.Throughput),
							Value = $"{application.ApplicationSummary?.Throughput}rpm"
						}
					};
		}

		private static string GetAttachmentColourForHealth(string health)
		{
			switch (health)
			{
				case Parameters.NewRelicGoodStatus:
					return Parameters.NewRelicGoodStatusColor;
				case Parameters.NewRelicWarningStatus:
					return Parameters.NewRelicWarningStatusColor;
				case Parameters.NewRelicBadStatus:
					return Parameters.NewRelicBadStatusColor;
				default:
					return Parameters.NewRelicUnknownStatusColor;
			}
		}

		private static bool ApplicationDetailTargetedCommandMisformed(string message)
		{
			return message.Split(" ", StringSplitOptions.RemoveEmptyEntries).Length != 4;
		}

		private static IEnumerable<ResponseMessage> ChuckAttachmentsAndReplyToChannel(
			IncomingMessage incomingMessage,
			IEnumerable<Attachment> applicationAttachments,
			string title)
		{
			var firstBatch = true;
			var attachments = applicationAttachments.ToList();

			if (!attachments.Any())
			{
				yield return incomingMessage.ReplyToChannel(title);
			}

			foreach (var chuckedAttachments in attachments.Batch(5))
			{
				var messageText = string.Empty;

				if (firstBatch)
				{
					firstBatch = false;
					messageText = title;
				}

				yield return incomingMessage.ReplyToChannel(messageText, chuckedAttachments.ToList());
			}
		}

		private static string GetApplicationDetailExample => $"`@{{bot}} {ApplicationDetailCommand} %PROD%`";
	}
}
