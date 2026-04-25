namespace EmployeeManagementService.Application.Features.BoardProposalRequests.Commands.ReorderBoardProposalTasks;

public class ReorderBoardProposalTasksRequest
{
    public int AgendaItemId { get; set; }

    public IReadOnlyCollection<ReorderBoardProposalTaskItem> Items { get; set; } = [];
}

public class ReorderBoardProposalTaskItem
{
    public int Id { get; set; }

    public int Order { get; set; }
}
