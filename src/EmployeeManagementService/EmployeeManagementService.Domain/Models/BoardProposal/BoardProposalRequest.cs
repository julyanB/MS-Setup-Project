using System.ComponentModel;
using EmployeeManagementService.Domain.Common;
using EmployeeManagementService.Domain.Enums.BoardProposal;

namespace EmployeeManagementService.Domain.Models.BoardProposal;

public sealed class BoardProposalRequest : RequestBase<int, BoardProposalStatus>
{
    public string MeetingCode { get; set; } = null!;

    public DateTimeOffset MeetingDate { get; set; }

    public BoardProposalMeetingType MeetingType { get; set; }

    public BoardProposalMeetingFormat MeetingFormat { get; set; }

    [Description("UserId")]
    public string SecretaryEmployeeId { get; set; } = null!;

    public DateTimeOffset? SentAt { get; set; }

    public DateTimeOffset? HeldAt { get; set; }

    public DateTimeOffset? ClosedAt { get; set; }

    public ICollection<BoardProposalAgendaItem> AgendaItems { get; set; } = [];
}
