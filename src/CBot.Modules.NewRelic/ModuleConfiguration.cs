namespace CBot.Modules.NewRelic
{
	public class ModuleConfiguration
	{
		public string ApiKey { get; set; }

		public string ApplicationsAccountBaseUrl { get; set; }

		public string GetApplicationUrl(string applicationId)
		{
			return this.ApplicationsAccountBaseUrl + applicationId;
		}
	}
}
