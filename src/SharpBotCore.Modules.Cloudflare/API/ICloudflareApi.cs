using System.Threading.Tasks;

using SharpBotCore.Modules.Cloudflare.API.Models;

namespace SharpBotCore.Modules.Cloudflare.API
{
	public interface ICloudflareApi
	{
		Task<Zone> GetZoneByName(string zoneName);

		Task<bool> PurgeZone(Zone zone);

		Task<bool> PurgeZoneCacheTag(Zone zone, string cacheTag);
	}
}