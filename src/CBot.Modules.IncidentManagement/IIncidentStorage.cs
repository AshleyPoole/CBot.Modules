using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using CBot.Modules.IncidentManagement.Models;

namespace CBot.Modules.IncidentManagement
{
	internal interface IIncidentStorage
	{
		Task<int> GetNextRowKey(string incidentDateTime);

		Task<Incident> PersistNewIncident(Incident incident);

		Task<Incident> UpdateIncident(Incident incident);

		Task<Incident> GetIncidentByChannelId(string channelId);

		Task<Incident> GetIncidentByChannelName(string channelName);

		Task<Incident> GetIncidentById(Guid friendlyId);

		Task<List<Incident>> GetActiveIncidents();

		Task<List<Incident>> GetRecentIncidents(int days);
	}
}