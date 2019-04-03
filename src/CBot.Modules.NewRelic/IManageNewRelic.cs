using System.Collections.Generic;
using System.Threading.Tasks;

namespace CBot.Modules.NewRelic
{
	public interface IManageNewRelic
	{
		IEnumerable<string> GetAccountNames();

		Task<NewRelicResponse> GetAllApplications(string accountName = null);

		Task<NewRelicResponse> GetUnhealthyApplications(string accountName = null);

		Task<NewRelicResponse> GetApplicationsLikeName(string searchPattern, string accountName = null);
	}
}