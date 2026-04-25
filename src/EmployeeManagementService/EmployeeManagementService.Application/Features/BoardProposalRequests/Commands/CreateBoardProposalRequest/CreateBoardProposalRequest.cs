namespace EmployeeManagementService.Application.Features.BoardProposalRequests.Commands.CreateBoardProposalRequest;

public class CreateBoardProposalRequest
{
    public DateTime MeetingDate { get; set; }

    public string MeetingType { get; set; } = null!;

    public string MeetingFormat { get; set; } = null!;

    public string SecretaryEmployeeId { get; set; } = null!;
}
