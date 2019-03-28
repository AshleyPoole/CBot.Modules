using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CBot.Modules.NewRelic.API;
using CBot.Modules.NewRelic.API.Models;

using Microsoft.Extensions.Logging;

namespace CBot.Modules.NewRelic
{
	public class NewRelicManager : IManageNewRelic
	{
		private readonly INewRelicApi newRelicApi;

		private readonly ILogger<NewRelicManager> logger;

		public NewRelicManager(INewRelicApi newRelicApi, ILogger<NewRelicManager> logger)
		{
			this.newRelicApi = newRelicApi;
			this.logger = logger;
		}

		public IEnumerable<string> GetAccountNames()
		{
			return newRelicApi.GetAccountNames();
		}

		public async Task<NewRelicResponse> GetAllApplications(string accountName = null)
		{
			IEnumerable<Application> newRelicApplications;

			try
			{
				newRelicApplications = await this.newRelicApi.GetAllApplicationsSummary(accountName);
			}
			catch (Exception e)
			{
				this.logger.LogError("Failed to get or parse applications from NewRelic.", e);
				return new NewRelicResponse(null, OperationStatus.Failed);
			}

			return new NewRelicResponse(newRelicApplications, OperationStatus.Success);
			
		}

		public async Task<NewRelicResponse> GetUnhealthyApplications(string accountName = null)
		{
			IList<Application> unhealthyApplications;

			try
			{
				unhealthyApplications = (await this.newRelicApi.GetAllApplicationsSummary(accountName))
					.Where(x => x.HealthStatus == Parameters.NewRelicBadStatus).ToList();
			}
			catch (Exception e)
			{
				this.logger.LogError("Failed to get unhealthy applications from NewRelic.", e);
				return new NewRelicResponse(null, OperationStatus.Failed);
			}

			return new NewRelicResponse(unhealthyApplications, OperationStatus.Success);
		}

		public async Task<NewRelicResponse> GetApplicationsLikeName(string searchPattern, string accountName = null)
		{
			const string WildcardCharacter = "%";
			List<Application> filteredApplications;

			var applicationNameForChecking = searchPattern.Replace(WildcardCharacter, string.Empty).ToLower();
			var allApplications = await this.newRelicApi.GetFilteredApplicationsSummaryByName(applicationNameForChecking, accountName);

			if (searchPattern.StartsWith(WildcardCharacter) && searchPattern.EndsWith(WildcardCharacter))
			{
				filteredApplications =
					allApplications.Where(x => x.Name.ToLower().Contains(applicationNameForChecking)).ToList();
			}
			else if (searchPattern.StartsWith(WildcardCharacter))
			{
				filteredApplications =
					allApplications.Where(x => x.Name.ToLower().EndsWith(applicationNameForChecking)).ToList();
			}
			else if (searchPattern.EndsWith(WildcardCharacter))
			{
				filteredApplications =
					allApplications.Where(x => x.Name.ToLower().StartsWith(applicationNameForChecking)).ToList();
			}
			else
			{
				filteredApplications = allApplications.Where(x => x.Name.ToLower() == applicationNameForChecking).ToList();
			}

			return new NewRelicResponse(filteredApplications, OperationStatus.Success);
		}
	}
}