using EmployeeManagementService.Application.Features.Identity.Commands.CreateUser;
using EmployeeManagementService.Application.Features.Identity.Commands.LdapLoginUser;
using EmployeeManagementService.Application.Features.Identity.Commands.LoginUser;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagementService.Web.Features;

[ApiController]
[Route("[controller]")]
[HttpLogging(HttpLoggingFields.All & ~HttpLoggingFields.RequestBody & ~HttpLoggingFields.ResponseBody)]
public class IdentityController : ControllerBase
{
    [HttpPost]
    [Route(nameof(Register))]
    public async Task<ActionResult> Register(
        CreateUserCommand command,
        [FromServices] CreateUserService createUserService,
        CancellationToken cancellationToken)
        => await createUserService.Handle(command, cancellationToken);

    [HttpPost]
    [Route(nameof(Login))]
    public async Task<ActionResult<LoginOutputModel>> Login(
        LoginUserCommand command,
        [FromServices] LoginUserService loginUserService,
        CancellationToken cancellationToken)
        => await loginUserService.Handle(command, cancellationToken);

    [HttpPost]
    [Route(nameof(LdapLogin))]
    public async Task<ActionResult<LoginOutputModel>> LdapLogin(
        LdapLoginUserCommand command,
        [FromServices] LdapLoginUserService ldapLoginUserService,
        CancellationToken cancellationToken)
        => await ldapLoginUserService.Handle(command, cancellationToken);
}
