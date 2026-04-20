using EmployeeManagementService.Application.Exceptions;
using NUnit.Framework;
using EmployeeManagementService.Application.Features.Identity;
using EmployeeManagementService.Application.Features.Identity.Commands.CreateUser;
using NSubstitute;

namespace EmployeeManagementService.Application.UnitTests.Features.Identity;

[TestFixture]
public class CreateUserServiceTests
{
    private IIdentity _identity = null!;
    private CreateUserService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _identity = Substitute.For<IIdentity>();
        _service = new CreateUserService(_identity, new CreateUserCommandValidator());
    }

    [Test]
    public async Task Handle_WithValidCommand_ShouldCallRegister()
    {
        var command = new CreateUserCommand("user@example.com", "Secret123");
        _identity.Register(command).Returns(Substitute.For<IUser>());

        await _service.Handle(command, CancellationToken.None);

        await _identity.Received(1).Register(command);
    }

    [Test]
    public async Task Handle_WithInvalidEmail_ShouldThrowModelValidationException()
    {
        var command = new CreateUserCommand("not-an-email", "Secret123");

        Assert.ThrowsAsync<ModelValidationException>(() =>
            _service.Handle(command, CancellationToken.None));

        await _identity.DidNotReceive().Register(Arg.Any<UserInputModel>());
    }

    [Test]
    public async Task Handle_WithEmptyPassword_ShouldThrowModelValidationException()
    {
        var command = new CreateUserCommand("user@example.com", "");

        Assert.ThrowsAsync<ModelValidationException>(() =>
            _service.Handle(command, CancellationToken.None));

        await _identity.DidNotReceive().Register(Arg.Any<UserInputModel>());
    }
}
