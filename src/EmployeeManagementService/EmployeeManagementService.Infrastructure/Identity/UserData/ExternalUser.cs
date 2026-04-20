namespace EmployeeManagementService.Infrastructure.Identity.UserData;

public class ExternalUser : User
{
    internal ExternalUser(string email)
        : base(email)
    {
        IsExternal = true;
    }
}
