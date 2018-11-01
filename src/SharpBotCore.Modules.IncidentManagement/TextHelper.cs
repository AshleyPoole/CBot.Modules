using SharpBotCore.Modules.IncidentManagement.Models;

namespace SharpBotCore.Modules.IncidentManagement
{
	internal static class TextHelper
	{
		public static string GetNewIncidentTextForWarRoomChannel(Incident incident)
		{
			// TODO: SHOULD ADD @{bot} to the helper text?
			return $"Incident #{ incident.FriendlyId } regarding '{ incident.Title }' has been declared by @{ incident.DeclaredBy } and bound to this channel.\n"
					+ "Please run `incident resolve` once the incident has been mitigated, and remember to add people to the channel that might be able to help. Good luck!";
		}
	}
}
