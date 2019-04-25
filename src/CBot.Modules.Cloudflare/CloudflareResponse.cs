namespace CBot.Modules.Cloudflare
{
	public class CloudflareResponse
	{
		public CloudflareResponse(
			OperationStatus operationStatus,
			string zoneName,
			string zoneId = null,
			string response = null)
		{
			this.OperationStatus = operationStatus;
			this.ZoneName = zoneName;
			this.ZoneId = zoneId;
			this.ResponseText = response;
		}
		public OperationStatus OperationStatus { get; }

		public string ZoneName { get; }

		public string ZoneId { get; }

		public string ResponseText { get; }
	}
}