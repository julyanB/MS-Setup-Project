using EmployeeManagementService.Domain.Common;
using EmployeeManagementService.Domain.Enums.BoardProposal;

namespace EmployeeManagementService.Domain.Models.BoardProposal;

public sealed class BoardProposalRequest : RequestBase<int, BoardProposalStatus>
{
    public string MeetingCode { get; set; } = null!;

    public DateTime MeetingDate { get; set; }

    public string MeetingType { get; set; } = null!;

    public string MeetingFormat { get; set; } = null!;

    public string SecretaryEmployeeId { get; set; } = null!;

    public DateTime? SentAt { get; set; }

    public DateTime? HeldAt { get; set; }

    public DateTime? ClosedAt { get; set; }

    public ICollection<BoardProposalAgendaItem> AgendaItems { get; set; } = [];
}
