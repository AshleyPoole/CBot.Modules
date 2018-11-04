using System.Threading.Tasks;

namespace SharpBotCore.Modules.Cloudflare
{
	public interface IManageCloudflare
	{
		Task<CloudflareResponse> PurgeZone(string zoneName, string requestedBy);

		Task<CloudflareResponse> PurgeZoneCacheTag(string zoneName, string cacheTag, string requestedBy);
	}
}
