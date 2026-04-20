using BankingOperationsService.Application.Common;
using BankingOperationsService.Application.Contracts;
using Microsoft.AspNetCore.Http;

namespace BankingOperationsService.Web.Services;

public class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public bool IsAuthenticated => !string.IsNullOrEmpty(GetHeader(Headers.UserId));

    public string? UserId => GetHeader(Headers.UserId);

    public string? Email => GetHeader(Headers.UserEmail);

    public IReadOnlyCollection<string> Roles => SplitHeader(Headers.UserRoles);

    public IReadOnlyCollection<string> UserPermissions => SplitHeader(Headers.UserPermissions);

    private IReadOnlyCollection<string> SplitHeader(string name)
    {
        var raw = GetHeader(name);
        return string.IsNullOrEmpty(raw)
            ? Array.Empty<string>()
            : raw.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }

    private string? GetHeader(string name)
    {
        var headers = _httpContextAccessor.HttpContext?.Request.Headers;
        if (headers is null)
        {
            return null;
        }

        var value = headers[name].ToString();
        return string.IsNullOrEmpty(value) ? null : value;
    }
}
