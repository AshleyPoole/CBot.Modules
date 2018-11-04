using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using SharpBotCore.Modules.IncidentManagement.Models;

namespace SharpBotCore.Modules.IncidentManagement
{
	internal class IncidentManager : IManageIncidents, IRetrieveIncidents
	{
		private readonly IIncidentStorage incidentStorage;

		private readonly IInteractWithSlack slackInteraction;

		private readonly ILogger<IncidentManager> logger;

		private readonly ModuleConfiguration configuration;

		public IncidentManager(IIncidentStorage incidentStorage, IInteractWithSlack slackInteraction, ILogger<IncidentManager> logger, ModuleConfiguration configuration)
		{
			this.incidentStorage = incidentStorage;
			this.slackInteraction = slackInteraction;
			this.logger = logger;
			this.configuration = configuration;
		}

		public async Task<IncidentResponse> DeclareNewIncident(string incidentTitle, string reportedByUser)
		{
			var assignedWarRoomChannel = await this.GetAvailableWarRoomChannel();
			if (assignedWarRoomChannel == null)
			{
				this.logger.LogInformation($"No available channels are available for incident being declared by {reportedByUser}.");
				return new IncidentResponse(null, OperationStatus.NoWarroomAvailable);
			}

			var incident = new Incident(incidentTitle, assignedWarRoomChannel, reportedByUser);
			incident.SetRowKey(await this.incidentStorage.GetNextRowKey(incident.PartitionKey));

			incident = await this.incidentStorage.PersistNewIncident(incident);

			// TODO: PUT IN ASYNC WHEN ALL
			await this.SetChannelPurposeBasedOnIncidentStatus(incident);
			await this.SetChannelTopicBasedOnIncidentStatus(incident);

			await this.slackInteraction.SendIncidentBoundMessageToChannel(incident);
			await this.slackInteraction.SendIncidentDeclaredMainChannelMessage(incident);

			this.logger.LogInformation($"Declared new incident for {reportedByUser} with incidentId:{incident.Id}");

			return new IncidentResponse(incident, OperationStatus.Success);
		}

		public async Task<IncidentResponse> ResolveIncident(string resolvedBy, string incidentChannelId)
		{
			var incident = await this.incidentStorage.GetIncidentByChannelId(incidentChannelId);
			if (incident == null)
			{
				return new IncidentResponse(null, OperationStatus.NoIncidentForChannel);
			}

			if (incident.Resolved && !incident.Closed)
			{
				return new IncidentResponse(null, OperationStatus.IncidentAlreadyResolved);
			}

			if (incident.Resolved && incident.Closed)
			{
				return new IncidentResponse(null, OperationStatus.IncidentAlreadyClosed);
			}

			incident.MarkAsResolved(resolvedBy);

			await this.incidentStorage.UpdateIncident(incident);

			await this.SetChannelPurposeBasedOnIncidentStatus(incident);
			await this.slackInteraction.SendIncidentResolvedMainChannelMessage(incident);

			return new IncidentResponse(incident, OperationStatus.Success);
		}

		public async Task<IncidentResponse> AddPostmortemToIncident(string postmortemLink, string addedByUser, string incidentChannelId)
		{
			var incident = await this.incidentStorage.GetIncidentByChannelId(incidentChannelId);
			if (incident == null || incident.Closed)
			{
				return new IncidentResponse(null, OperationStatus.NoIncidentForChannel);
			}

			incident.AddPostmortem(addedByUser, postmortemLink);

			await this.incidentStorage.UpdateIncident(incident);

			await this.slackInteraction.SendIncidentPostmortemAddedMainChannelMessage(incident);

			return new IncidentResponse(incident, OperationStatus.Success);
		}

		public async Task<IncidentResponse> CloseIncident(string resolvedBy, string incidentChannelId)
		{
			var incident = await this.incidentStorage.GetIncidentByChannelId(incidentChannelId);

			if (incident == null)
			{
				return new IncidentResponse(null, OperationStatus.NoIncidentForChannel);
			}

			if (!incident.Resolved)
			{
				return new IncidentResponse(null, OperationStatus.IncidentNotResolved);
			}

			if (!incident.PostmortemAdded)
			{
				return new IncidentResponse(null, OperationStatus.IncidentMissingPostmortem);
			}

			incident.MarkAsClosed(resolvedBy);

			await this.incidentStorage.UpdateIncident(incident);

			await this.SetChannelPurposeBasedOnIncidentStatus(incident);
			await this.SetChannelTopicBasedOnIncidentStatus(incident);

			await this.slackInteraction.SendIncidentClosedMainChannelMessage(incident);

			return new IncidentResponse(incident, OperationStatus.Success);
		}

		public async Task<Incident> GetIncidentById(Guid id)
		{
			return await this.incidentStorage.GetIncidentById(id);
		}

		public async Task<List<Incident>> GetActiveIncidents()
		{
			return await this.incidentStorage.GetActiveIncidents();
		}

		public async Task<List<Incident>> GetRecentIncidents(int pastDays = 14)
		{
			return await this.incidentStorage.GetRecentIncidents(pastDays);
		}

		private async Task<Channel> GetAvailableWarRoomChannel()
		{
			foreach (var warRoomName in this.configuration.WarroomList())
			{
				var lastInstanceForChannel = await this.incidentStorage.GetIncidentByChannelName(warRoomName);
				if (lastInstanceForChannel == null || lastInstanceForChannel.Closed)
				{
					return new Channel(
						await this.slackInteraction.GetChannelIdByName(warRoomName),
						warRoomName);
				}
			}

			return null;
		}

		private async Task SetChannelPurposeBasedOnIncidentStatus(Incident incident)
		{
			var purpose = incident.Resolved && incident.Closed
							? $"Incident Warroom -- No active incident bound"
							: $"INCIDENT #{incident.FriendlyId} -- {incident.FriendlyStatus} -- {incident.Title}";

			await this.slackInteraction.UpdateChannelPurpose(incident.ChannelId, purpose);
		}

		private async Task SetChannelTopicBasedOnIncidentStatus(Incident incident)
		{
			var topic = incident.Resolved && incident.Closed
							? Parameters.Whitespace
							: $"INCIDENT #{incident.FriendlyId}";

			await this.slackInteraction.UpdateChannelTopic(incident.ChannelId, topic);
		}
	}
}
