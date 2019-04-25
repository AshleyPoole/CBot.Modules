using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using CBot.Modules.Cloudflare.API;

namespace CBot.Modules.Cloudflare
{
	public class CloudflareManager : IManageCloudflare
	{
		private readonly ICloudflareApi cloudflareApi;

		private readonly ILogger<CloudflareManager> logger;

		public CloudflareManager(ICloudflareApi cloudflareApi, ILogger<CloudflareManager> logger)
		{
			this.cloudflareApi = cloudflareApi;
			this.logger = logger;
		}

		public async Task<CloudflareResponse> PurgeZone(string zoneName, string requestedBy)
		{
			var zone = await this.cloudflareApi.GetZoneByName(zoneName);

			if (zone == null)
			{
				this.logger.LogWarning($"Unable to find Cloudflare zone for zone {zoneName} purge requested by {requestedBy}");
				return new CloudflareResponse(OperationStatus.ZoneNotFound, zoneName);
			}

			var purgeSuccess = await this.cloudflareApi.PurgeZone(zone);

			if (!purgeSuccess)
			{
				this.logger.LogError($"Failed to purge Cloudflare cache zone {zone.Name} for {requestedBy}");
				return new CloudflareResponse(OperationStatus.Failed, zone.Name, zone.Id);
			}

			this.logger.LogInformation($"Purged Cloudflare cache zone {zone.Name} for {requestedBy}");

			return new CloudflareResponse(OperationStatus.Success, zone.Name, zone.Id);
		}

		public async Task<CloudflareResponse> PurgeZoneCacheTag(string zoneName, string cacheTag, string requestedBy)
		{
			var zone = await this.cloudflareApi.GetZoneByName(zoneName);

			if (zone == null)
			{
				this.logger.LogWarning($"Unable to find Cloudflare zone for zone {zoneName} purge cache tag requested by {requestedBy}");
				return new CloudflareResponse(OperationStatus.ZoneNotFound, zoneName);
			}

			var purgeSuccess = await this.cloudflareApi.PurgeZoneCacheTag(zone, cacheTag);

			if (!purgeSuccess)
			{
				this.logger.LogError($"Failed to purge Cloudflare cache zone {zone.Name} of tag {cacheTag} for {requestedBy}");
				return new CloudflareResponse(OperationStatus.Failed, zone.Name, zone.Id);
			}

			this.logger.LogInformation($"Purged Cloudflare cache zone {zone.Name} of tag {cacheTag} for {requestedBy}");

			return new CloudflareResponse(OperationStatus.Success, zone.Name, zone.Id);
		}

		public async Task<CloudflareResponse> GetZoneRayIdLogs(string zoneName, string rayId)
		{
			const string RayIdLogFields = "RayID,ClientIP,ClientRequestHost,ClientRequestMethod,ClientRequestProtocol,ClientRequestUserAgent,ClientRequestURI,ClientSSLProtocol,ClientSrcPort,CacheCacheStatus,EdgeStartTimestamp,EdgeEndTimestamp,EdgeRequestHost,EdgeResponseStatus,EdgeResponse,EdgeOriginIP,EdgeRateLimitID,EdgeRateLimitAction,OriginResponseTime,OriginResponseStatus,OriginSSLProtocol,ZoneID,WAFAction,WAFFlags,WAFProfile,WAFMatchedVar,WAFRuleID,WAFRuleMessage";

			var zone = await this.cloudflareApi.GetZoneByName(zoneName);

			if (zone == null)
			{
				this.logger.LogWarning($"Unable to find Cloudflare zone for zone {zoneName} for CF RAY ID log retrieval");
				return new CloudflareResponse(OperationStatus.ZoneNotFound, zoneName);
			}

			var response = await this.cloudflareApi.GetRayIdLog(zone, rayId, RayIdLogFields);

			return string.IsNullOrWhiteSpace(response)
						? new CloudflareResponse(OperationStatus.Failed, zone.Name, zone.Id)
						: new CloudflareResponse(OperationStatus.Success, zone.Name, zone.Id, response);
		}
	}
}