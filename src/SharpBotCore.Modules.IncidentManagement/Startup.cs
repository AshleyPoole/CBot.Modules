using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using SharpBotCore.Middleware.Domain;

namespace SharpBotCore.Modules.IncidentManagement
{
	public static class Startup
	{
		public static IServiceCollection RegisterIncidentManagementModule(
			this IServiceCollection serviceCollection,
			IConfigurationSection configuration)
		{
			serviceCollection.AddSingleton<IIncidentStorage, StorageClient>();
			serviceCollection.AddSingleton<IManageIncidents, IncidentManager>();
			serviceCollection.AddSingleton<IRetrieveIncidents, IncidentManager>();
			serviceCollection.AddSingleton<IMiddleware, IncidentManagementMiddleware>();
			serviceCollection.AddSingleton<IInteractWithSlack, SlackInteractionService>();

			var moduleConfig = new ModuleConfiguration();
			configuration.Bind(moduleConfig);

			serviceCollection.AddSingleton(moduleConfig);

			return serviceCollection;
		}

	}
}
