using EmployeeManagementService.Application.Features.Identity.Commands.CreateUser;
using NUnit.Framework;

namespace EmployeeManagementService.Application.UnitTests.Features.Identity;

[TestFixture]
public class CreateUserCommandValidatorTests
{
    private CreateUserCommandValidator _validator = null!;

    [SetUp]
    public void SetUp() => _validator = new CreateUserCommandValidator();

    [Test]
    public async Task Validate_WithValidCommand_ShouldPass()
    {
        var command = new CreateUserCommand("user@example.com", "Secret123");

        var result = await _validator.ValidateAsync(command);

        Assert.That(result.IsValid, Is.True);
    }

    [TestCase("")]
    [TestCase("notanemail")]
    [TestCase("ab")]
    public async Task Validate_WithInvalidEmail_ShouldFail(string email)
    {
        var command = new CreateUserCommand(email, "Secret123");

        var result = await _validator.ValidateAsync(command);

        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors, Has.Some.Matches<FluentValidation.Results.ValidationFailure>(
            e => e.PropertyName == nameof(command.Email)));
    }

    [Test]
    public async Task Validate_WithEmptyPassword_ShouldFail()
    {
        var command = new CreateUserCommand("user@example.com", "");

        var result = await _validator.ValidateAsync(command);

        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors, Has.Some.Matches<FluentValidation.Results.ValidationFailure>(
            e => e.PropertyName == nameof(command.Password)));
    }
}
