using EmployeeManagementService.Application.Features.Identity.Commands.LoginUser;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagementService.Application.Features.Identity.Commands.LdapLoginUser;

public class LdapLoginUserCommand : UserInputModel
{
    public LdapLoginUserCommand(string email, string password)
        : base(email, password)
    {
    }
}

public class LdapLoginUserService
{
    private readonly IIdentity _identity;

    public LdapLoginUserService(IIdentity identity)
    {
        _identity = identity;
    }

    public async Task<ActionResult<LoginOutputModel>> Handle(
        LdapLoginUserCommand request,
        CancellationToken cancellationToken)
        => new OkObjectResult(await _identity.LdapLogin(request));
}
