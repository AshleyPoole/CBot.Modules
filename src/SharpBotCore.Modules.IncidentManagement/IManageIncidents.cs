using System.Threading.Tasks;

using SharpBotCore.Modules.IncidentManagement.Models;

namespace SharpBotCore.Modules.IncidentManagement
{
	internal interface IManageIncidents
	{
		Task<IncidentResponse> DeclareNewIncident(string incidentTitle, string reportedByUser);

		Task<IncidentResponse> ResolveIncident(string resolvedBy, string incidentChannelId);

		Task<IncidentResponse> AddPostmortemToIncident(
			string postmortemLink,
			string addedByUser,
			string incidentChannelId);

		Task<IncidentResponse> CloseIncident(string resolvedBy, string incidentChannelId);
	}
}