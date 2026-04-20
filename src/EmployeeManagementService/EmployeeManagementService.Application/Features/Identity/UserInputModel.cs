namespace EmployeeManagementService.Application.Features.Identity;

public class UserInputModel
{
    internal UserInputModel(string email, string password)
    {
        Email = email;
        Password = password;
    }

    public string Email { get; }

    public string Password { get; }
}
