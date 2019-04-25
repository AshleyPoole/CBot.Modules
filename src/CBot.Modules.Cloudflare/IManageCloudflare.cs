using System.Threading.Tasks;

namespace CBot.Modules.Cloudflare
{
	public interface IManageCloudflare
	{
		Task<CloudflareResponse> PurgeZone(string zoneName, string requestedBy);

		Task<CloudflareResponse> PurgeZoneCacheTag(string zoneName, string cacheTag, string requestedBy);

		Task<CloudflareResponse> GetZoneRayIdLogs(string zoneName, string rayId);
	}
}
