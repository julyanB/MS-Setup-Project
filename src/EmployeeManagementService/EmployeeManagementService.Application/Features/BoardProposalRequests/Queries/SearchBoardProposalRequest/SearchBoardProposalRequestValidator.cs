using FluentValidation;

namespace EmployeeManagementService.Application.Features.BoardProposalRequests.Queries.SearchBoardProposalRequest;

public class SearchBoardProposalRequestValidator : AbstractValidator<SearchBoardProposalRequest>
{
    public SearchBoardProposalRequestValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0);
    }
}
