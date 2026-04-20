using EmployeeManagementService.Application.Features.Identity.Commands.LoginUser;

namespace EmployeeManagementService.Application.Features.Identity;

public interface IIdentity
{
    Task<IUser> Register(UserInputModel userInput);

    Task<LoginOutputModel> Login(UserInputModel userInput);
}
