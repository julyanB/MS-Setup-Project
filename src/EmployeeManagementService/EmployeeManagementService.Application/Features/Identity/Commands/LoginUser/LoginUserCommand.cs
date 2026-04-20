using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagementService.Application.Features.Identity.Commands.LoginUser;

public class LoginUserCommand : UserInputModel
{
    public LoginUserCommand(string email, string password)
        : base(email, password)
    {
    }
}

public class LoginUserService
{
    private readonly IIdentity _identity;

    public LoginUserService(IIdentity identity)
    {
        _identity = identity;
    }

    public async Task<ActionResult<LoginOutputModel>> Handle(
        LoginUserCommand request,
        CancellationToken cancellationToken)
        => new OkObjectResult(await _identity.Login(request));
}
