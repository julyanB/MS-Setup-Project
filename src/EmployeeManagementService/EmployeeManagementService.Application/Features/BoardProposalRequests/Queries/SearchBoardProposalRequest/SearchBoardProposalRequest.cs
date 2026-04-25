using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagementService.Application.Features.BoardProposalRequests.Queries.SearchBoardProposalRequest;

public class SearchBoardProposalRequest
{
    [FromRoute(Name = "id")]
    public int Id { get; set; }
}
