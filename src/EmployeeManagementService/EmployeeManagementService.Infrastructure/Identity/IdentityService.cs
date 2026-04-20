using EmployeeManagementService.Application.Exceptions;
using EmployeeManagementService.Application.Features.Identity;
using EmployeeManagementService.Application.Features.Identity.Commands.LoginUser;
using EmployeeManagementService.Infrastructure.Identity.UserData;
using Microsoft.AspNetCore.Identity;

namespace EmployeeManagementService.Infrastructure.Identity;

internal class IdentityService : IIdentity
{
    private const string InvalidLoginErrorMessage = "Invalid credentials.";

    private readonly UserManager<User> _userManager;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public IdentityService(UserManager<User> userManager, IJwtTokenGenerator jwtTokenGenerator)
    {
        _userManager = userManager;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<IUser> Register(UserInputModel userInput)
    {
        var user = new User(userInput.Email);

        var identityResult = await _userManager.CreateAsync(user, userInput.Password);

        if (!identityResult.Succeeded)
        {
            var errors = identityResult.Errors.Select(e => e.Description);
            throw new IdentityException(errors);
        }

        return user;
    }

    public async Task<LoginOutputModel> Login(UserInputModel userInput)
    {
        var user = await _userManager.FindByEmailAsync(userInput.Email);
        if (user == null)
        {
            throw new IdentityException(InvalidLoginErrorMessage);
        }

        var passwordValid = await _userManager.CheckPasswordAsync(user, userInput.Password);
        if (!passwordValid)
        {
            throw new IdentityException(InvalidLoginErrorMessage);
        }

        var token = await _jwtTokenGenerator.GenerateToken(user);

        return new LoginOutputModel(token);
    }
}
