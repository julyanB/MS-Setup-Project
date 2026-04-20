using EmployeeManagementService.Infrastructure.Identity.UserData;

namespace EmployeeManagementService.Infrastructure.Identity;

public interface IJwtTokenGenerator
{
    Task<string> GenerateToken(User user);
}
