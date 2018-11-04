using System.Threading.Tasks;

using SharpBotCore.Modules.Cloudflare.API.Models;

namespace SharpBotCore.Modules.Cloudflare.API
{
	public interface ICloudflareApi
	{
		Task<ApiMultipleResourceResponse> GetRequest(string queryString);

		Task<ApiSingleResourceResponse> DeleteRequest(string queryString, string requestBody);
	}
}