using System.Collections.Generic;

using CBot.Messaging.Domain;
using CBot.Modules.IncidentManagement.Models;

using SlackConnector.Models;

namespace CBot.Modules.IncidentManagement
{
	internal static class AttachmentGenerator
	{
		public static List<AttachmentField> GetCoreAttachmentFields(Incident incident)
		{
			return new List<AttachmentField>
					{
						new AttachmentField { IsShort = false, Title = "Description", Value = incident.Title },
						new AttachmentField { IsShort = true, Title = "Status", Value = incident.FriendlyStatus },
						new AttachmentField
						{
							IsShort = true, Title = "Warroom", Value = $"#{incident.ChannelName}"
						},
						new AttachmentField
						{
							IsShort = true,
							Title = "Declared Timestamp",
							Value = $"{string.Format("{0:s}", incident.DeclaredDateTimeUtc)} UTC"
						},
						new AttachmentField
						{
							IsShort = true, Title = "Declared By", Value = $"@{incident.DeclaredBy}"
						}
					};
		}

		public static List<SlackAttachmentField> GetSlackCoreAttachmentFields(Incident incident)
		{
			var slackAttachmentFields = new List<SlackAttachmentField>();

			foreach (var attachmentFields in GetCoreAttachmentFields(incident))
			{
				slackAttachmentFields.Add(ConvertAttachmentFieldToSlackAttachmentField(attachmentFields));
			}

			return slackAttachmentFields;
		}

		public static List<AttachmentField> GetResolvedAttachmentFields(Incident incident)
		{
			return new List<AttachmentField>
					{
						new AttachmentField
						{
							IsShort = true,
							Title = "Resolved Timestamp",
							Value = $"{string.Format("{0:s}", incident.ResolvedDateTimeUtc)} UTC"
						},
						new AttachmentField { IsShort = true, Title = "Resolved By", Value = $"@{incident.ResolvedBy}" }
					};
		}

		public static List<SlackAttachmentField> GetSlackResolvedAttachmentFields(Incident incident)
		{
			var slackAttachmentFields = new List<SlackAttachmentField>();

			foreach (var attachmentFields in GetResolvedAttachmentFields(incident))
			{
				slackAttachmentFields.Add(ConvertAttachmentFieldToSlackAttachmentField(attachmentFields));
			}

			return slackAttachmentFields;
		}

		public static List<AttachmentField> GetPostmortemAttachmentFields(Incident incident)
		{
			return new List<AttachmentField>
					{
						new AttachmentField { IsShort = true, Title = "Postmortem Link", Value = incident.PostmortermLink },
						new AttachmentField
						{
							IsShort = true,
							Title = "Postmortem Added By",
							Value = $"@{incident.PostmortermAddedBy}"
						}
					};
		}

		public static List<SlackAttachmentField> GetSlackPostmortemAttachmentFields(Incident incident)
		{
			var slackAttachmentFields = new List<SlackAttachmentField>();

			foreach (var attachmentFields in GetPostmortemAttachmentFields(incident))
			{
				slackAttachmentFields.Add(ConvertAttachmentFieldToSlackAttachmentField(attachmentFields));
			}

			return slackAttachmentFields;
		}

		public static List<AttachmentField> GetClosedAttachmentFields(Incident incident)
		{
			return new List<AttachmentField>
					{
						new AttachmentField
						{
							IsShort = true,
							Title = "Closed Timestamp",
							Value = $"{string.Format("{0:s}", incident.ClosedDateTimeUtc)} UTC"
						},
						new AttachmentField { IsShort = true, Title = "Closed By", Value = $"@{incident.ClosedBy}" }
					};
		}

		public static List<SlackAttachmentField> GetSlackClosedAttachmentFields(Incident incident)
		{
			var slackAttachmentFields = new List<SlackAttachmentField>();

			foreach (var attachmentFields in GetClosedAttachmentFields(incident))
			{
				slackAttachmentFields.Add(ConvertAttachmentFieldToSlackAttachmentField(attachmentFields));
			}

			return slackAttachmentFields;
		}

		private static SlackAttachmentField ConvertAttachmentFieldToSlackAttachmentField(
			AttachmentField attachmentField)
		{
			return new SlackAttachmentField
					{
						IsShort = attachmentField.IsShort, Title = attachmentField.Title, Value = attachmentField.Value
					};
		}
	}
}
