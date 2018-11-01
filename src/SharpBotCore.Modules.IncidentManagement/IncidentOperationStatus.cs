namespace SharpBotCore.Modules.IncidentManagement
{
	enum IncidentOperationStatus
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
