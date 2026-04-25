namespace EmployeeManagementService.Application.Features.BoardProposalRequests.Commands.AddBoardProposalAgendaItem;

public class AddBoardProposalAgendaItemRequest
{
    public int BoardProposalRequestId { get; set; }

    public string Title { get; set; } = null!;

    public string InitiatorEmployeeId { get; set; } = null!;

    public string ResponsibleBoardMemberEmployeeId { get; set; } = null!;

    public string PresenterEmployeeId { get; set; } = null!;

    public string Category { get; set; } = null!;

    public string? Description { get; set; }
}
