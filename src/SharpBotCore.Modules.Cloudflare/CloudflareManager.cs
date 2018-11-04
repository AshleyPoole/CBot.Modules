using System;
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
			throw new NotImplementedException();
		}

		public async Task<CloudflareResponse> PurgeZoneCacheTag(string zoneName, string cacheTag, string requestedBy)
		{
			throw new NotImplementedException();
		}
	}
}