using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

using CBot.Modules.NewRelic.API.Models;


namespace CBot.Modules.NewRelic.API
{
	public class NewRelicApi : INewRelicApi
	{
		private readonly ModuleConfiguration configuration;

		public NewRelicApi(ModuleConfiguration configuration)
		{
			this.configuration = configuration;
		}

		public async Task<IEnumerable<Application>> GetAllApplicationsSummary()
		{
			const string RelativeUrl = "applications.json";

			var client = this.GetHttpClientWithBaseAddress();
			var result = await client.GetAsync(RelativeUrl);

			if (!result.IsSuccessStatusCode)
			{
				return null;
			}

			var apiResponse = ApiResponse.FromJson(await result.Content.ReadAsStringAsync());
			return apiResponse.Applications;
		}

		public async Task<IEnumerable<Application>> GetFilteredApplicationsSummaryByName(string searchTerm)
		{
			var relativeUrl = "applications.json" + $"?filter[name]={searchTerm}";

			var client = this.GetHttpClientWithBaseAddress();
			var result = await client.GetAsync(relativeUrl);

			if (!result.IsSuccessStatusCode)
			{
				return null;
			}

			var apiResponse = ApiResponse.FromJson(await result.Content.ReadAsStringAsync());
			return apiResponse.Applications;
		}

		private HttpClient GetHttpClientWithBaseAddress()
		{
			var httpClient = new HttpClient { BaseAddress = new Uri("https://api.newrelic.com/v2/") };
			httpClient.DefaultRequestHeaders.Add("X-API-Key", this.configuration.ApiKey);
			return httpClient;
		}
	}
}
