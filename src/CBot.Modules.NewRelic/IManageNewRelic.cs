using System.Threading.Tasks;

namespace CBot.Modules.NewRelic
{
	public interface IManageNewRelic
	{
		Task<NewRelicResponse> GetAllApplications();

		Task<NewRelicResponse> GetUnhealthyApplications();

		Task<NewRelicResponse> GetApplicationsLikeName(string searchPattern);
	}
}