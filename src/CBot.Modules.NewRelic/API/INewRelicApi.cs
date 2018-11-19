using System.Collections.Generic;
using System.Threading.Tasks;

using CBot.Modules.NewRelic.API.Models;

namespace CBot.Modules.NewRelic.API
{
	public interface INewRelicApi
	{
		Task<IEnumerable<Application>> GetAllApplicationsSummary();

		Task<IEnumerable<Application>> GetFilteredApplicationsSummaryByName(string searchTerm);
	}
}
