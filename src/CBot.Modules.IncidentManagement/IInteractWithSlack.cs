using System.Threading.Tasks;

using CBot.Modules.IncidentManagement.Models;

namespace CBot.Modules.IncidentManagement
{
	internal interface IInteractWithSlack
	{
		Task<string> GetChannelIdByName(string channelName);

		Task SendIncidentDeclaredMainChannelMessage(Incident incident);

		Task SendIncidentResolvedMainChannelMessage(Incident incident);

		Task SendIncidentPostmortemAddedMainChannelMessage(Incident incident);

		Task SendIncidentClosedMainChannelMessage(Incident incident);

		Task SendIncidentForcedClosedMainChannelMessage(Incident incident);

		Task SendIncidentDeletedMainChannelMessage(Incident incident);

		Task SendIncidentBoundMessageToChannel(Incident incident);

		Task UpdateChannelTopic(string channelId, string topic);

		Task UpdateChannelPurpose(string channelId, string purpose);

		
	}
}
