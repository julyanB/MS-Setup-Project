using EmployeeManagementService.Application.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagementService.Application.Features.Identity.Commands.CreateUser;

public class CreateUserCommand : UserInputModel
{
    public CreateUserCommand(string email, string password)
        : base(email, password)
    {
    }
}

public class CreateUserService
{
    private readonly IIdentity _identity;
    private readonly IValidator<CreateUserCommand> _validator;

    public CreateUserService(
        IIdentity identity,
        IValidator<CreateUserCommand> validator)
    {
        _identity = identity;
        _validator = validator;
    }

    public async Task<ActionResult> Handle(
        CreateUserCommand request,
        CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
        {
            throw new ModelValidationException(validationResult.Errors);
        }

        await _identity.Register(request);

        return new OkResult();
    }
}
