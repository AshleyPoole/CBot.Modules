namespace CBot.Modules.IncidentManagement
{
	enum OperationStatus
	{
		Success,
		NoWarroomAvailable,
		NoIncidentForChannel,
		IncidentAlreadyResolved,
		IncidentAlreadyClosed,
		IncidentNotResolved,
		IncidentMissingPostmortem
	}
}
