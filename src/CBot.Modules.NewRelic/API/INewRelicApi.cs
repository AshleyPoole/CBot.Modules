using System.Collections.Generic;
using System.Threading.Tasks;

using CBot.Modules.NewRelic.API.Models;

namespace CBot.Modules.NewRelic.API
{
	public interface INewRelicApi
	{
		IEnumerable<string> GetAccountNames();

		Task<IEnumerable<Application>> GetAllApplicationsSummary(string accountName);

		Task<IEnumerable<Application>> GetFilteredApplicationsSummaryByName(string searchTerm, string accountName);
	}
}
