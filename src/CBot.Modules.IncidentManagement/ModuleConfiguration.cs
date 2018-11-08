using System.Collections.Generic;
using System.Linq;

namespace CBot.Modules.IncidentManagement
{
	internal class ModuleConfiguration
	{
		public string IncidentNotificationChannel { get; set; }

		public string PostmortemTemplateLink {get; set; }

		public string Warrooms { get; set; }

		public string AzureConnectionString { get; set; }

		public List<string> WarroomList() => this.Warrooms.Split(',').ToList();
	}
}
