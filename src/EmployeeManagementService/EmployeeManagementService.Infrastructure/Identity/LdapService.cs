using System.Text;
using EmployeeManagementService.Application.Features.Identity;
using EmployeeManagementService.Infrastructure.Configuration;
using Microsoft.Extensions.Options;
using Novell.Directory.Ldap;

namespace EmployeeManagementService.Infrastructure.Identity;

internal sealed class LdapService : ILdapService
{
    private static readonly string[] _userAttributes = ["*"];

    private readonly LdapOptions _options;

    public LdapService(IOptions<LdapOptions> options)
    {
        _options = options.Value;
    }

    public async Task<LdapUser?> AuthenticateAndGetUserAsync(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(_options.Host)
            || string.IsNullOrWhiteSpace(_options.Domain)
            || string.IsNullOrWhiteSpace(_options.BaseDn))
        {
            return null;
        }

        var normalizedUsername = NormalizeUsername(username);
        var ldapUsername = $"{_options.Domain}\\{normalizedUsername}";

        try
        {
            using var connection = new LdapConnection
            {
                SecureSocketLayer = _options.UseSsl
            };

            await connection.ConnectAsync(_options.Host, _options.Port);
            await connection.BindAsync(ldapUsername, password);

            if (!connection.Bound)
            {
                return null;
            }

            var filter = $"(&(objectClass=user)(sAMAccountName={EscapeLdapFilterValue(normalizedUsername)}))";
            var results = await connection.SearchAsync(
                _options.BaseDn,
                LdapConnection.ScopeSub,
                filter,
                _userAttributes,
                false);

            if (!await results.HasMoreAsync())
            {
                return null;
            }

            var entry = await results.NextAsync();
            var attributes = entry.GetAttributeSet();

            var userAttributes = GetAttributes(attributes);

            return new LdapUser
            {
                Username = GetSingleAttributeValue(userAttributes, "sAMAccountName"),
                DistinguishedName = GetSingleAttributeValue(userAttributes, "distinguishedName") ?? entry.Dn,
                DisplayName = GetSingleAttributeValue(userAttributes, "displayName"),
                Email = GetSingleAttributeValue(userAttributes, "mail"),
                Groups = GetAttributeValues(userAttributes, "memberOf"),
                Attributes = userAttributes
            };
        }
        catch (LdapException)
        {
            return null;
        }
    }

    private static string NormalizeUsername(string username)
    {
        var value = username.Trim();

        var slashIndex = value.LastIndexOf('\\');
        if (slashIndex >= 0 && slashIndex < value.Length - 1)
        {
            value = value[(slashIndex + 1)..];
        }

        var atIndex = value.IndexOf('@');
        if (atIndex > 0)
        {
            value = value[..atIndex];
        }

        return value;
    }

    private static string EscapeLdapFilterValue(string value)
    {
        var escaped = new StringBuilder(value.Length);

        foreach (var character in value)
        {
            escaped.Append(character switch
            {
                '\\' => "\\5c",
                '*' => "\\2a",
                '(' => "\\28",
                ')' => "\\29",
                '\0' => "\\00",
                _ => character
            });
        }

        return escaped.ToString();
    }

    private static IReadOnlyDictionary<string, IReadOnlyList<string>> GetAttributes(LdapAttributeSet attributes)
    {
        var values = new Dictionary<string, IReadOnlyList<string>>(StringComparer.OrdinalIgnoreCase);

        foreach (LdapAttribute attribute in attributes)
        {
            values[attribute.Name] = attribute.StringValueArray;
        }

        return values;
    }

    private static string GetSingleAttributeValue(
        IReadOnlyDictionary<string, IReadOnlyList<string>> attributes,
        string name)
        => GetAttributeValues(attributes, name).FirstOrDefault() ?? "";

    private static IReadOnlyList<string> GetAttributeValues(
        IReadOnlyDictionary<string, IReadOnlyList<string>> attributes,
        string name)
        => attributes.TryGetValue(name, out var values) ? values : [];
}
