using FluentValidation;

namespace EmployeeManagementService.Application.Features.Identity.Queries.GetEmployees;

public class GetEmployeesRequestValidator : AbstractValidator<GetEmployeesRequest>
{
    public GetEmployeesRequestValidator()
    {
        RuleFor(x => x.Role)
            .MaximumLength(256);

        RuleFor(x => x.Search)
            .MaximumLength(256);

        RuleFor(x => x.Limit)
            .InclusiveBetween(1, 250);
    }
}
