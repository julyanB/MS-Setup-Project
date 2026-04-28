namespace EmployeeManagementService.Application.Features.Identity;

public sealed class LdapUser
{
    public string Username { get; set; } = "";

    public string DistinguishedName { get; set; } = "";

    public string DisplayName { get; set; } = "";

    public string Email { get; set; } = "";

    public IReadOnlyList<string> Groups { get; set; } = [];

    public IReadOnlyDictionary<string, IReadOnlyList<string>> Attributes { get; set; }
        = new Dictionary<string, IReadOnlyList<string>>();
}
