namespace CBot.Modules.NewRelic
{
	public class ModuleConfiguration
	{
		public string AccountName { get; set; }

		public bool IsDefault { get; set; }

		public string ApiUrl { get; set; }

		public string ApiKey { get; set; }

		public string ApplicationsAccountBaseUrl { get; set; }

		public string GetApplicationUrl(string applicationId)
		{
			return this.ApplicationsAccountBaseUrl + applicationId;
		}
	}
}
