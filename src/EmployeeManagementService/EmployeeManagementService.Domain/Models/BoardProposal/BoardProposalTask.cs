using EmployeeManagementService.Domain.Common;

namespace EmployeeManagementService.Domain.Models.BoardProposal;

public sealed class BoardProposalTask : Auditable<int>
{
    public int AgendaItemId { get; set; }

    public BoardProposalAgendaItem AgendaItem { get; set; } = null!;

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public string ResponsibleEmployeeId { get; set; } = null!;

    public DateTime DueDate { get; set; }

    public int Order { get; set; }

    public string Status { get; set; } = null!;

    public DateTime? ExtendedDueDate { get; set; }

    public string? Comment { get; set; }
}
