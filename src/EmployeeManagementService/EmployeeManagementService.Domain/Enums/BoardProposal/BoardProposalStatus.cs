namespace EmployeeManagementService.Domain.Enums.BoardProposal;

public enum BoardProposalStatus
{
    Draft = 1,
    AgendaPreparation = 2,
    SecretaryReview = 3,
    ChairpersonReview = 4,
    ReadyForSending = 5,
    Sent = 6,
    Held = 7,
    DecisionsAndTasks = 8,
    DeadlineMonitoring = 9,
    Closed = 10,
    Cancelled = 11
}
