using System.Collections.Generic;

using CBot.Modules.NewRelic.API.Models;

namespace CBot.Modules.NewRelic
{
	public class NewRelicResponse
	{
		public NewRelicResponse(IEnumerable<Application> applications, OperationStatus operationStatus)
		{
			this.Applications = applications;
			this.OperationStatus = operationStatus;
		}

		public IEnumerable<Application> Applications { get; }

		public OperationStatus OperationStatus { get; }
	}
}
