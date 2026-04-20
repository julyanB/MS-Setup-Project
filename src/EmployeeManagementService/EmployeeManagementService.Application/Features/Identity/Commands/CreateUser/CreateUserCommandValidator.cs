using FluentValidation;

using static EmployeeManagementService.Domain.Models.ModelConstants.Common;

namespace EmployeeManagementService.Application.Features.Identity.Commands.CreateUser;

public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(u => u.Email)
            .MinimumLength(MinEmailLength)
            .MaximumLength(MaxEmailLength)
            .EmailAddress()
            .NotEmpty();

        RuleFor(u => u.Password)
            .MaximumLength(MaxNameLength)
            .NotEmpty();
    }
}
