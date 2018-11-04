namespace SharpBotCore.Modules.IncidentManagement.Models
{
	internal class IncidentResponse
	{
		public IncidentResponse(Incident incident, OperationStatus operationStatus)
		{
			this.Incident = incident;
			this.OperationStatus = operationStatus;
		}

		public Incident Incident { get; }

		public OperationStatus OperationStatus { get; }
	}
}
