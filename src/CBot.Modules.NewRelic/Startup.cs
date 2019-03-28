using CBot.Middleware.Domain;
using CBot.Modules.NewRelic.API;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CBot.Modules.NewRelic
{
	public static class Startup
	{
		public static IServiceCollection RegisterNewRelicModule(
			this IServiceCollection serviceCollection,
			IConfigurationSection configuration)
		{
			serviceCollection.AddSingleton<INewRelicApi, NewRelicApi>();
			serviceCollection.AddTransient<IManageNewRelic, NewRelicManager>();
			serviceCollection.AddSingleton<IMiddleware, NewRelicMiddleware>();

			var apiUrl = configuration.GetValue<string>("ApiUrl");
			var applicationsAccountBaseUrl = configuration.GetValue<string>("ApplicationsAccountBaseUrl");

			foreach (var newRelicAccountConfig in configuration.GetValue<string>("Accounts").Split('|'))
			{
				var accountParts = newRelicAccountConfig.Split(',');
				var accountId = accountParts[0];
				var apiKey = accountParts[1];
				var accountName = accountParts[2];
				bool.TryParse(accountParts[3], out bool isDefault);

				serviceCollection.AddSingleton(new ModuleConfiguration
				{
					AccountName = accountName,
					IsDefault = isDefault,
					ApiKey = apiKey,
					ApiUrl = apiUrl,
					ApplicationsAccountBaseUrl = applicationsAccountBaseUrl.Replace("{accountId}", accountId)
				});
			}

			return serviceCollection;
		}
	}
}
