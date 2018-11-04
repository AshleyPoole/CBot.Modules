namespace SharpBotCore.Modules.Cloudflare
{
	public class CloudflareResponse
	{
		public CloudflareResponse(OperationStatus operationStatus, string zoneName, string zoneId)
		{
			this.OperationStatus = operationStatus;
			this.ZoneName = zoneName;
			this.ZoneId = zoneId;
		}
		public OperationStatus OperationStatus { get; }

		public string ZoneName { get; }

		public string ZoneId { get; }

	}
}