using System;
using System.ComponentModel;

using Microsoft.WindowsAzure.Storage.Table;

namespace CBot.Modules.IncidentManagement.Models
{
	public class Incident : TableEntity
	{
		internal Incident(string incidentTitle, Channel channel, string declaredBy)
		{
			this.Id = Guid.NewGuid();
			this.PartitionKey = DateTime.UtcNow.ToString("yyyy-MM-dd");

			this.ChannelId = channel.Id;
			this.ChannelName = channel.Name;
			this.Title = incidentTitle;
			this.DeclaredBy = declaredBy;
			this.DeclaredDateTimeUtc = DateTime.UtcNow;

			this.Resolved = false;
			this.Closed = false;
		}

		public Incident()
		{
		}

		public Guid Id { get; set; }

		public string Title { get; set; }

		public string ChannelId { get; set; }

		[DisplayName("Channel")]
		public string ChannelName { get; set; }

		[DisplayName("Declared By")]
		public string DeclaredBy { get; set; }

		[DisplayName("Declared Timestamp")]
		public DateTime DeclaredDateTimeUtc { get; set; }

		public bool Resolved { get; set; }

		[DisplayName("Resolved By")]
		public string ResolvedBy { get; set; }

		[DisplayName("Resolved Timestamp")]
		public DateTime? ResolvedDateTimeUtc { get; set; }

		[DisplayName("Postmortem Link")]
		public string PostmortermLink { get; set; }

		[DisplayName("Postmortem Added By")]
		public string PostmortermAddedBy { get; set; }

		[DisplayName("Postmortem Added Timestamp")]
		public DateTime? PostmortemAddedDateTimeUtc { get; set; }

		public bool Closed { get; set; }

		[DisplayName("Closed By")]
		public string ClosedBy { get; set; }

		[DisplayName("Closed Timestamp")]
		public DateTime? ClosedDateTimeUtc { get; set; }

		[DisplayName("Forced Closed")]
		public bool ForceClosed { get; set; }

		public bool Deleted { get; set; }

		[DisplayName("Id")]
		public string FriendlyId => $"{this.PartitionKey}-{this.RowKey}";

		public bool PostmortemAdded => !string.IsNullOrWhiteSpace(this.PostmortermLink);

		public string CleanPostmortemLink =>
			string.IsNullOrEmpty(this.PostmortermLink)
				? string.Empty
				: this.PostmortermLink.Replace("<", string.Empty).Replace(">", string.Empty);

		[DisplayName("Status")]
		public string FriendlyStatus
		{
			get
			{
				if (this.ForceClosed)
				{
					return "FORCE CLOSED";
				}

				if (this.Deleted)
				{
					return "DELETED";
				}

				if (!this.Resolved && !this.Closed)
				{
					return "IN-PROGRESS";
				}

				if (this.Resolved && !this.Closed)
				{
					return "MITIGATED";
				}

				if (this.Resolved && this.Closed)
				{
					return "CLOSED";
				}

				return "UNKNOWN";
			}
		}

		public void MarkAsResolved(string resolvedBy)
		{
			this.ResolvedDateTimeUtc = DateTime.UtcNow;
			this.ResolvedBy = resolvedBy;
			this.Resolved = true;
		}

		public void AddPostmortem(string addedBy, string postmortemLink)
		{
			this.PostmortemAddedDateTimeUtc = DateTime.UtcNow;
			this.PostmortermAddedBy = addedBy;
			this.PostmortermLink = postmortemLink;
		}

		public void MarkAsClosed(string closedBy)
		{
			this.ClosedDateTimeUtc = DateTime.UtcNow;
			this.ClosedBy = closedBy;
			this.Closed = true;
		}

		public void MarkAsForcedClosed(string closedBy)
		{
			this.MarkAsClosed(closedBy);
			this.ForceClosed = true;
		}

		public void MarkAsDeleted(string deletedBy)
		{
			this.MarkAsClosed(deletedBy);
			this.Deleted = true;
		}

		public void SetRowKey(int rowKey)
		{
			this.RowKey = rowKey.ToString();
		}
	}
}
