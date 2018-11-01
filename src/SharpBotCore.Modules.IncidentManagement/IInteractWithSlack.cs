using System.Threading.Tasks;

using SharpBotCore.Modules.IncidentManagement.Models;

namespace SharpBotCore.Modules.IncidentManagement
{
	internal interface IInteractWithSlack
	{
		Task<string> GetChannelIdByName(string channelName);

		Task SendIncidentDeclaredMainChannelMessage(Incident incident);

		Task SendIncidentResolvedMainChannelMessage(Incident incident);

		Task SendIncidentPostmortemAddedMainChannelMessage(Incident incident);

		Task SendIncidentClosedMainChannelMessage(Incident incident);

		Task SendMessageToChannel(string channelName, string messageText);

		Task UpdateChannelTopic(string channelId, string topic);

		Task UpdateChannelPurpose(string channelId, string purpose);

		
	}
}
