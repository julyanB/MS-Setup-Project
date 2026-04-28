namespace EmployeeManagementService.Infrastructure.Configuration;

public sealed class LdapOptions
{
    public const string SectionName = "Ldap";

    public required string Host { get; set; }

    public required int Port { get; set; } = 636;

    public required bool UseSsl { get; set; } = true;

    public required string BaseDn { get; set; }

    public required string Domain { get; set; }
}
