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

			var moduleConfig = new ModuleConfiguration();
			configuration.Bind(moduleConfig);

			serviceCollection.AddSingleton(moduleConfig);

			return serviceCollection;
		}
	}
}
