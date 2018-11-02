using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using SharpBotCore.Modules.IncidentManagement.Models;

namespace SharpBotCore.Modules.IncidentManagement
{
	public interface IRetrieveIncidents
	{
		Task<Incident> GetIncidentById(Guid friendlyId);

		Task<List<Incident>> GetActiveIncidents();

		Task<List<Incident>> GetRecentIncidents(int pastDays = 14);
	}
}