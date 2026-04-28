using EmployeeManagementService.Domain.Enums.BoardProposal;

namespace EmployeeManagementService.Application.Features.BoardProposalRequests.Commands.CreateBoardProposalRequest;

public class CreateBoardProposalRequest
{
    public DateTime MeetingDate { get; set; }

    public BoardProposalMeetingType MeetingType { get; set; }

    public BoardProposalMeetingFormat MeetingFormat { get; set; }

    public string SecretaryEmployeeId { get; set; } = null!;
}
