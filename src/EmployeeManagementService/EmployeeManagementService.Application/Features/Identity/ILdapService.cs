namespace EmployeeManagementService.Application.Features.Identity;

public interface ILdapService
{
    Task<LdapUser?> AuthenticateAndGetUserAsync(string username, string password);
}
