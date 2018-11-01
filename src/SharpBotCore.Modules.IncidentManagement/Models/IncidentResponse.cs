namespace SharpBotCore.Modules.IncidentManagement.Models
{
	internal class IncidentResponse
	{
		public IncidentResponse(Incident incident, IncidentOperationStatus operationStatus)
		{
			this.Incident = incident;
			this.OperationStatus = operationStatus;
		}

		public Incident Incident { get; }

		public IncidentOperationStatus OperationStatus { get; }
	}
}
