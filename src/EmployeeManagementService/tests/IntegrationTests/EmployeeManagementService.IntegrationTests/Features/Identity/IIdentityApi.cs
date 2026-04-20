using EmployeeManagementService.Application.Features.Identity.Commands.CreateUser;
using EmployeeManagementService.Application.Features.Identity.Commands.LoginUser;
using Refit;

namespace EmployeeManagementService.IntegrationTests.Features.Identity;

public interface IIdentityApi
{
    [Post("/Identity/Register")]
    Task<IApiResponse> Register([Body] CreateUserCommand body);

    [Post("/Identity/Login")]
    Task<IApiResponse<LoginOutputModel>> Login([Body] LoginUserCommand body);
}
