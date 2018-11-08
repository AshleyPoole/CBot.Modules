using System.Threading.Tasks;

using CBot.Modules.Cloudflare.API.Models;

namespace CBot.Modules.Cloudflare.API
{
	public interface ICloudflareApi
	{
		Task<Zone> GetZoneByName(string zoneName);

		Task<bool> PurgeZone(Zone zone);

		Task<bool> PurgeZoneCacheTag(Zone zone, string cacheTag);
	}
}