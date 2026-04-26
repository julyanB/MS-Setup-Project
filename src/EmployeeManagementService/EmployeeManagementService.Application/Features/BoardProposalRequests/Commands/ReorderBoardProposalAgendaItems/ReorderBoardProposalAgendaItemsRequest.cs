namespace EmployeeManagementService.Application.Features.BoardProposalRequests.Commands.ReorderBoardProposalAgendaItems;

public class ReorderBoardProposalAgendaItemsRequest
{
    public int BoardProposalRequestId { get; set; }

    public IReadOnlyCollection<ReorderBoardProposalAgendaItem> Items { get; set; } = [];
}

public class ReorderBoardProposalAgendaItem
{
    public int Id { get; set; }

    public int Order { get; set; }
}
