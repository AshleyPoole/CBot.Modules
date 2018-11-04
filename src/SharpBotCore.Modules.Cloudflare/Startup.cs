using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using SharpBotCore.Middleware.Domain;
using SharpBotCore.Modules.Cloudflare.API;

namespace SharpBotCore.Modules.Cloudflare
{
	public static class Startup
	{
		public static IServiceCollection RegisterCloudflareModule(
			this IServiceCollection serviceCollection,
			IConfigurationSection configuration)
		{
			serviceCollection.AddSingleton<IMiddleware, CloudflareMiddleware>();
			serviceCollection.AddTransient<IManageCloudflare, CloudflareManager>();
			serviceCollection.AddSingleton<ICloudflareApi, CloudflareApi>();

			var moduleConfig = new ModuleConfiguration();
			configuration.Bind(moduleConfig);

			serviceCollection.AddSingleton(moduleConfig);

			return serviceCollection;
		}
	}
}
