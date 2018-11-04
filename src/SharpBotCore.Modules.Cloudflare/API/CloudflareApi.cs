using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

using SharpBotCore.Modules.Cloudflare.API.Models;

namespace SharpBotCore.Modules.Cloudflare.API
{
	internal class CloudflareApi : ICloudflareApi
	{
		private readonly ModuleConfiguration configuration;

		public CloudflareApi(ModuleConfiguration configuration)
		{
			this.configuration = configuration;
		}

		public async Task<ApiMultipleResourceResponse> GetRequest(string queryString)
		{
			using (var handler = new HttpClientHandler())
			{
				using (var client = new HttpClient(handler))
				{
					client.DefaultRequestHeaders.Add("X-Auth-Email", this.configuration.AuthEmail);
					client.DefaultRequestHeaders.Add("X-Auth-Key", this.configuration.AuthKey);

					var request = await client.GetAsync(this.configuration.ApiUrl + queryString);
					var jsonString = await request.Content.ReadAsStringAsync();
					return JsonConvert.DeserializeObject<ApiMultipleResourceResponse>(jsonString, SerializerSettings.Settings);
				}
			}
		}

		public async Task<ApiSingleResourceResponse> DeleteRequest(string queryString, string requestBody)
		{
			using (var handler = new HttpClientHandler())
			{
				using (var client = new HttpClient(handler))
				{
					client.DefaultRequestHeaders.Add("X-Auth-Email", this.configuration.AuthEmail);
					client.DefaultRequestHeaders.Add("X-Auth-Key", this.configuration.AuthKey);

					var requestMessage =
						new HttpRequestMessage(HttpMethod.Delete, new Uri(this.configuration.ApiUrl + queryString))
						{
							Content = new StringContent(requestBody, Encoding.UTF8, "application/json")
						};

					var request = await client.SendAsync(requestMessage);
					var jsonString = await request.Content.ReadAsStringAsync();
					return JsonConvert.DeserializeObject<ApiSingleResourceResponse>(jsonString, SerializerSettings.Settings);
				}
			}
		}
	}
}