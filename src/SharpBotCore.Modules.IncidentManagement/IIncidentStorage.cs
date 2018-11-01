using System.Collections.Generic;
using System.Threading.Tasks;

using SharpBotCore.Modules.IncidentManagement.Models;

namespace SharpBotCore.Modules.IncidentManagement
{
	internal interface IIncidentStorage
	{
		Task<int> GetNextRowKey(string incidentDateTime);

		Task<Incident> PersistNewIncident(Incident incident);

		Task<Incident> UpdateIncident(Incident incident);

		Task<Incident> GetIncidentByChannelId(string channelId);

		Task<Incident> GetIncidentByChannelName(string channelName);

		Task<List<Incident>> GetOpenIncidents();

		Task<List<Incident>> GetRecentIncidents();
	}
}