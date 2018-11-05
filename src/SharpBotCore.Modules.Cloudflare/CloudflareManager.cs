using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using SharpBotCore.Modules.Cloudflare.API;

namespace SharpBotCore.Modules.Cloudflare
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
	}
}