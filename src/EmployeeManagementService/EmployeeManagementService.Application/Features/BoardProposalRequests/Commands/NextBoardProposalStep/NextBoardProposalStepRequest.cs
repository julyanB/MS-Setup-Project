using EmployeeManagementService.Domain.Enums;

namespace EmployeeManagementService.Application.Features.BoardProposalRequests.Commands.NextBoardProposalStep;

public class NextBoardProposalStepRequest
{
    public int Id { get; set; }

    public EmployeeRequestAction Action { get; set; }

    public string? Comment { get; set; }
}
