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
			var attachmentFields = AttachmentGenerator.GetSlackCoreAttachmentFields(incident);

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
			var attachmentFields = AttachmentGenerator.GetSlackCoreAttachmentFields(incident);
			attachmentFields.AddRange(AttachmentGenerator.GetSlackResolvedAttachmentFields(incident));

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
			var attachmentFields = AttachmentGenerator.GetSlackCoreAttachmentFields(incident);
			attachmentFields.AddRange(AttachmentGenerator.GetSlackPostmortemAttachmentFields(incident));

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
			var attachmentFields = AttachmentGenerator.GetSlackCoreAttachmentFields(incident);
			attachmentFields.AddRange(AttachmentGenerator.GetSlackResolvedAttachmentFields(incident));
			attachmentFields.AddRange(AttachmentGenerator.GetSlackPostmortemAttachmentFields(incident));
			attachmentFields.AddRange(AttachmentGenerator.GetSlackClosedAttachmentFields(incident));

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
	}
}