using EmployeeManagementService.Domain.Enums.BoardProposal;

namespace EmployeeManagementService.Application.Features.BoardProposalRequests.Queries.SearchBoardProposalRequest;

public class BoardProposalRequestDetails
{
    public int Id { get; set; }

    public string MeetingCode { get; set; } = null!;

    public DateTimeOffset MeetingDate { get; set; }

    public BoardProposalMeetingType MeetingType { get; set; }

    public BoardProposalMeetingFormat MeetingFormat { get; set; }

    public string SecretaryEmployeeId { get; set; } = null!;

    public BoardProposalStatus Status { get; set; }

    public IReadOnlyCollection<BoardProposalAgendaItemDetails> AgendaItems { get; set; } = [];

    public IReadOnlyCollection<AttachmentDetails> Attachments { get; set; } = [];

    public IReadOnlyCollection<RequestApprovalTargetDetails> ActiveApprovalTargets { get; set; } = [];
}

public class RequestApprovalTargetDetails
{
    public string TargetType { get; set; } = null!;

    public string TargetValue { get; set; } = null!;
}

public class BoardProposalAgendaItemDetails
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public string InitiatorEmployeeId { get; set; } = null!;

    public string ResponsibleBoardMemberEmployeeId { get; set; } = null!;

    public string PresenterEmployeeId { get; set; } = null!;

    public BoardProposalAgendaCategory Category { get; set; }

    public string? Description { get; set; }

    public int Order { get; set; }

    public BoardProposalDecisionStatus? DecisionStatus { get; set; }

    public string? DecisionText { get; set; }

    public BoardProposalFinalVote? FinalVote { get; set; }

    public string? Notes { get; set; }

    public IReadOnlyCollection<BoardProposalVoteDetails> Votes { get; set; } = [];

    public IReadOnlyCollection<BoardProposalTaskDetails> Tasks { get; set; } = [];
}

public class BoardProposalVoteDetails
{
    public int Id { get; set; }

    public string BoardMemberEmployeeId { get; set; } = null!;

    public BoardProposalVoteType VoteType { get; set; }

    public string? Notes { get; set; }
}

public class BoardProposalTaskDetails
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public string ResponsibleEmployeeId { get; set; } = null!;

    public DateTimeOffset DueDate { get; set; }

    public int Order { get; set; }

    public BoardProposalTaskStatus Status { get; set; }

    public DateTimeOffset? ExtendedDueDate { get; set; }

    public string? Comment { get; set; }
}

public class AttachmentDetails
{
    public int Id { get; set; }

    public string FileName { get; set; } = null!;

    public string DocumentType { get; set; } = null!;

    public string DocumentName { get; set; } = null!;

    public string? Section { get; set; }

    public int? SectionEntityId { get; set; }

    public long SizeInBytes { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}
