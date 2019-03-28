using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

using CBot.Modules.NewRelic.API.Models;

using Microsoft.Extensions.Logging;


namespace CBot.Modules.NewRelic.API
{
	public class NewRelicApi : INewRelicApi
	{
		private readonly IEnumerable<ModuleConfiguration> configurations;

		private readonly ILogger<NewRelicApi> logger;

		public NewRelicApi(IEnumerable<ModuleConfiguration> configurations, ILogger<NewRelicApi> logger)
		{
			this.configurations = configurations;
			this.logger = logger;
		}

		public IEnumerable<string> GetAccountNames()
		{
			return configurations.OrderByDescending(c => c.IsDefault).ThenBy(c => c.AccountName).Select(c => c.AccountName);
		}

		public async Task<IEnumerable<Application>> GetAllApplicationsSummary(string accountName)
		{
			const string RelativeUrl = "applications.json";

			var client = this.GetHttpClientWithBaseAddress(accountName);
			var result = await client.GetAsync(RelativeUrl);

			if (!result.IsSuccessStatusCode)
			{
				return null;
			}

			var apiResponse = ApiResponse.FromJson(await result.Content.ReadAsStringAsync());
			return apiResponse.Applications;
		}

		public async Task<IEnumerable<Application>> GetFilteredApplicationsSummaryByName(string searchTerm, string accountName)
		{
			var relativeUrl = "applications.json" + $"?filter[name]={searchTerm}";

			var client = this.GetHttpClientWithBaseAddress(accountName);
			var result = await client.GetAsync(relativeUrl);

			if (!result.IsSuccessStatusCode)
			{
				return null;
			}

			var apiResponse = ApiResponse.FromJson(await result.Content.ReadAsStringAsync());
			return apiResponse.Applications;
		}

		private HttpClient GetHttpClientWithBaseAddress(string accountName)
		{
			ModuleConfiguration configuration;
			if (string.IsNullOrWhiteSpace(accountName))
			{
				configuration = configurations.First(c => c.IsDefault);
			}
			else
			{
				configuration = configurations.First(c => c.AccountName.Equals(accountName, StringComparison.InvariantCultureIgnoreCase));
			}

			if (configuration is null)
			{
				logger.LogError($"Failed to find New Relic account by name: {accountName}");
			}

			var httpClient = new HttpClient { BaseAddress = new Uri(configuration.ApiUrl) };
			httpClient.DefaultRequestHeaders.Add("X-API-Key", configuration.ApiKey);
			return httpClient;
		}
	}
}
