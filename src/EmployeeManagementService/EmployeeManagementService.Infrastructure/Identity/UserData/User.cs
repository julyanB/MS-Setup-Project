using EmployeeManagementService.Application.Features.Identity;
using Microsoft.AspNetCore.Identity;

namespace EmployeeManagementService.Infrastructure.Identity.UserData;

public class User : IdentityUser, IUser
{
    internal User(string email)
        : base(email)
    {
        Email = email;
    }

    public bool IsExternal { get; protected set; }
}
