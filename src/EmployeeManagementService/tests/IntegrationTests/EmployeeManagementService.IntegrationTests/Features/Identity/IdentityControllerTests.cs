using EmployeeManagementService.Application.Features.Identity.Commands.CreateUser;
using NUnit.Framework;
using EmployeeManagementService.Application.Features.Identity.Commands.LoginUser;
using EmployeeManagementService.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagementService.IntegrationTests.Features.Identity;

[TestFixture]
public class IdentityControllerTests : IntegrationTestBase
{
    private IIdentityApi _api = null!;

    [OneTimeSetUp]
    public void SetUpApi()
    {
        _api = Api<IIdentityApi>();
    }

    [Test]
    public async Task Register_WithValidCredentials_ShouldReturn200()
    {
        var command = new CreateUserCommand("newuser@example.com", "Secret123!");

        var response = await _api.Register(command);

        Assert.That(response.IsSuccessStatusCode, Is.True);
    }

    [Test]
    public async Task Register_WithInvalidEmail_ShouldReturn400()
    {
        var command = new CreateUserCommand("not-an-email", "Secret123!");

        var response = await _api.Register(command);

        Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.BadRequest));
    }

    [Test, Order(1)]
    public async Task Login_WithValidCredentials_ShouldReturnToken()
    {
        var registerCommand = new CreateUserCommand("existinguser@example.com", "Secret123!");
        await _api.Register(registerCommand);

        var command = new LoginUserCommand("existinguser@example.com", "Secret123!");

        var response = await _api.Login(command);

        Assert.That(response.IsSuccessStatusCode, Is.True);
        Assert.That(response.Content?.Token, Is.Not.Null.And.Not.Empty);
    }

    [Test]
    public async Task Register_ShouldPersistUserInDatabase()
    {
        var email = "dbcheck@example.com";
        var command = new CreateUserCommand(email, "Secret123!");

        var response = await _api.Register(command);

        Assert.That(response.IsSuccessStatusCode, Is.True);

        var user = await DbContext.Users.FirstOrDefaultAsync(u => u.Email == email);

        Assert.That(user, Is.Not.Null);
        Assert.That(user!.Email, Is.EqualTo(email));
        Assert.That(user.UserName, Is.EqualTo(email));
    }

    [Test]
    public async Task Register_ShouldIncrementUserCount()
    {
        var initialCount = await DbContext.Users.AsNoTracking().CountAsync();

        var email = $"cnt{Guid.NewGuid():N}"[..12] + "@test.com";
        var command = new CreateUserCommand(email, "Secret123!");

        var response = await _api.Register(command);

        Assert.That(response.IsSuccessStatusCode, Is.True,
            $"Registration failed with status {response.StatusCode}");

        var newCount = await DbContext.Users.AsNoTracking().CountAsync();

        Assert.That(newCount, Is.EqualTo(initialCount + 1));
    }
}
