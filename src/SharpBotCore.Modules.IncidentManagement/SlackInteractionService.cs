using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using SharpBotCore.Messaging.Slack;
using SharpBotCore.Modules.IncidentManagement.Models;

using SlackConnector;
using SlackConnector.Models;

namespace SharpBotCore.Modules.IncidentManagement
{
	internal class SlackInteractionService : IInteractWithSlack
	{
		private readonly ISlackConnection slackConnection;

		private readonly ModuleConfiguration configuration;

		public SlackInteractionService(ISlackConnectionFactory slackConnectionFactory, ModuleConfiguration configuration)
		{
			this.slackConnection = slackConnectionFactory.GetConnection().GetAwaiter().GetResult();
			this.configuration = configuration;
		}

		public async Task<string> GetChannelIdByName(string channelName)
		{
			var channels = await this.slackConnection.GetChannels();
			return channels.FirstOrDefault(x => x.Name == $"#{ channelName }")?.Id;
		}

		public async Task SendIncidentDeclaredMainChannelMessage(Incident incident)
		{
			var attachmentFields = GetCoreAttachmentFields(incident);

			var chatHub = new SlackChatHub { Id = this.configuration.IncidentNotificationChannel };
			var attachment = new SlackAttachment { Fields = attachmentFields, ColorHex = Parameters.UnresolvedIncidentColor };

			var message = new BotMessage
			{
				ChatHub = chatHub,
				Text = $"*INCIDENT DECLARED #{incident.FriendlyId}*",
				Attachments = new List<SlackAttachment> { attachment }
			};

			await this.slackConnection.Say(message);
		}

		public async Task SendIncidentResolvedMainChannelMessage(Incident incident)
		{
			var attachmentFields = GetCoreAttachmentFields(incident);
			attachmentFields.AddRange(GetResolvedAttachmentFields(incident));

			var chatHub = new SlackChatHub { Id = this.configuration.IncidentNotificationChannel };
			var attachment = new SlackAttachment { Fields = attachmentFields, ColorHex = Parameters.ResolvedIncidentColor };

			var message = new BotMessage
						{
							ChatHub = chatHub,
							Text = $"*INCIDENT MITIGATED #{incident.FriendlyId}*",
							Attachments = new List<SlackAttachment> { attachment }
						};

			await this.slackConnection.Say(message);
		}

		public async Task SendIncidentPostmortemAddedMainChannelMessage(Incident incident)
		{
			var attachmentFields = GetCoreAttachmentFields(incident);
			attachmentFields.AddRange(GetPostmortemAttachmentFields(incident));

			var chatHub = new SlackChatHub { Id = this.configuration.IncidentNotificationChannel };
			var attachment = new SlackAttachment { Fields = attachmentFields, ColorHex = Parameters.PostmortemIncidentColor };

			var message = new BotMessage
						{
							ChatHub = chatHub,
							Text = $"*INCIDENT POSTMORTEM ADDED #{incident.FriendlyId}*",
							Attachments = new List<SlackAttachment> { attachment }
						};

			await this.slackConnection.Say(message);
		}

		public async Task SendIncidentClosedMainChannelMessage(Incident incident)
		{
			var attachmentFields = GetCoreAttachmentFields(incident);
			attachmentFields.AddRange(GetResolvedAttachmentFields(incident));
			attachmentFields.AddRange(GetPostmortemAttachmentFields(incident));
			attachmentFields.AddRange(GetClosedAttachmentFields(incident));

			var chatHub = new SlackChatHub { Id = this.configuration.IncidentNotificationChannel };
			var attachment = new SlackAttachment { Fields = attachmentFields, ColorHex = Parameters.ClosedIncidentColor };

			var message = new BotMessage
						{
							ChatHub = chatHub,
							Text = $"*INCIDENT CLOSED #{incident.FriendlyId}*",
							Attachments = new List<SlackAttachment> { attachment }
						};

			await this.slackConnection.Say(message);
		}

		public async Task UpdateChannelTopic(string channelId, string topic)
		{
			await this.slackConnection.SetChannelTopic(channelId, topic);
		}

		public async Task UpdateChannelPurpose(string channelId, string purpose)
		{
			await this.slackConnection.SetChannelPurpose(channelId, purpose);
		}

		public async Task SendIncidentBoundMessageToChannel(Incident incident)
		{
			var messageText = $"Incident #{incident.FriendlyId} regarding '{incident.Title}' has been declared by @{incident.DeclaredBy} and bound to this channel.\n"
				+ "Please run `mitigated incident` once the incident has been mitigated and remember to add people to the channel that might be able to help. Good luck!";

			var chatHub = new SlackChatHub { Id = incident.ChannelName };
			var message = new BotMessage { ChatHub = chatHub, Text = messageText };

			await this.slackConnection.Say(message);
		}

		private static List<SlackAttachmentField> GetCoreAttachmentFields(Incident incident)
		{
			return new List<SlackAttachmentField>
					{
						new SlackAttachmentField { IsShort = false, Title = "Description", Value = incident.Title },
						new SlackAttachmentField { IsShort = true, Title = "Status", Value = incident.FriendlyStatus },
						new SlackAttachmentField
						{
							IsShort = true, Title = "Warroom", Value = $"#{incident.ChannelName}"
						},
						new SlackAttachmentField
						{
							IsShort = true,
							Title = "Declared Timestamp",
							Value = $"{string.Format("{0:s}", incident.DeclaredDateTimeUtc)} UTC"
						},
						new SlackAttachmentField
						{
							IsShort = true, Title = "Declared By", Value = $"@{incident.DeclaredBy}"
						}
					};
		}

		private static List<SlackAttachmentField> GetResolvedAttachmentFields(Incident incident)
		{
			return new List<SlackAttachmentField>
					{
						new SlackAttachmentField
						{
							IsShort = true,
							Title = "Resolved Timestamp",
							Value = $"{string.Format("{0:s}", incident.ResolvedDateTimeUtc)} UTC"
						},
						new SlackAttachmentField { IsShort = true, Title = "Resolved By", Value = $@"{incident.ResolvedBy}" }
					};
		}

		private static List<SlackAttachmentField> GetPostmortemAttachmentFields(Incident incident)
		{
			return new List<SlackAttachmentField>
					{
						new SlackAttachmentField { IsShort = true, Title = "Postmortem Link", Value = incident.PostmortermLink },
						new SlackAttachmentField
						{
							IsShort = true,
							Title = "Postmortem Added By",
							Value = $"@{incident.PostmortermAddedBy}"
						}
					};
		}

		private static List<SlackAttachmentField> GetClosedAttachmentFields(Incident incident)
		{
			return new List<SlackAttachmentField>
					{
						new SlackAttachmentField
						{
							IsShort = true,
							Title = "Closed Timestamp",
							Value = $"{string.Format("{0:s}", incident.ClosedDateTimeUtc)} UTC"
						},
						new SlackAttachmentField { IsShort = true, Title = "Closed By", Value = $"@{incident.ClosedBy}" }
					};
		}
	}
}