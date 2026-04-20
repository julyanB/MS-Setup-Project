using EmployeeManagementService.Application.Exceptions;
using EmployeeManagementService.Application.Features.Identity;
using EmployeeManagementService.Domain.Models;
using EmployeeManagementService.Infrastructure.Identity.UserData;
using EmployeeManagementService.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagementService.Infrastructure.Identity;

internal class UserPermissionsService : IUserPermissions
{
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly EmployeeManagementServiceDbContext _db;

    public UserPermissionsService(
        UserManager<User> userManager,
        RoleManager<IdentityRole> roleManager,
        EmployeeManagementServiceDbContext db)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _db = db;
    }

    public async Task<IReadOnlyList<string>> GetPermissions(string userId, CancellationToken cancellationToken = default)
    {
        await GetUserOrThrow(userId); // validate user exists

        return await _db.UserPermissions
            .Where(up => up.UserId == userId)
            .Select(up => up.Permission.Name)
            .OrderBy(n => n)
            .ToArrayAsync(cancellationToken);
    }

    public async Task AddPermission(string userId, string permission, CancellationToken cancellationToken = default)
    {
        ValidatePermission(permission);

        await GetUserOrThrow(userId); // validate user exists

        var alreadyBound = await _db.UserPermissions
            .AnyAsync(up => up.UserId == userId && up.Permission.Name == permission, cancellationToken);

        if (alreadyBound)
        {
            return;
        }

        var permissionEntity = await GetOrCreatePermission(permission, cancellationToken);

        _db.UserPermissions.Add(new UserPermission
        {
            UserId = userId,
            PermissionId = permissionEntity.Id
        });

        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task RemovePermission(string userId, string permission, CancellationToken cancellationToken = default)
    {
        ValidatePermission(permission);

        await GetUserOrThrow(userId); // validate user exists

        var binding = await _db.UserPermissions
            .FirstOrDefaultAsync(up => up.UserId == userId && up.Permission.Name == permission, cancellationToken);

        if (binding is null)
        {
            return;
        }

        _db.UserPermissions.Remove(binding);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<string>> GetRoles(string userId, CancellationToken cancellationToken = default)
    {
        var user = await GetUserOrThrow(userId);
        var roles = await _userManager.GetRolesAsync(user);
        return roles.OrderBy(r => r).ToArray();
    }

    public async Task AddRole(string userId, string roleName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(roleName))
        {
            throw new IdentityException("Role name is required.");
        }

        if (!await _roleManager.RoleExistsAsync(roleName))
        {
            throw new NotFoundException(nameof(IdentityRole), roleName);
        }

        var user = await GetUserOrThrow(userId);

        if (await _userManager.IsInRoleAsync(user, roleName))
        {
            return;
        }

        var result = await _userManager.AddToRoleAsync(user, roleName);
        ThrowIfFailed(result);
    }

    public async Task RemoveRole(string userId, string roleName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(roleName))
        {
            throw new IdentityException("Role name is required.");
        }

        var user = await GetUserOrThrow(userId);

        if (!await _userManager.IsInRoleAsync(user, roleName))
        {
            return;
        }

        var result = await _userManager.RemoveFromRoleAsync(user, roleName);
        ThrowIfFailed(result);
    }

    private async Task<Permission> GetOrCreatePermission(string name, CancellationToken cancellationToken)
    {
        var existing = await _db.Permissions
            .FirstOrDefaultAsync(p => p.Name == name, cancellationToken);

        if (existing is not null)
        {
            return existing;
        }

        var newPermission = new Permission
        {
            Name = name,
            CreatedAt = DateTimeOffset.UtcNow
        };

        _db.Permissions.Add(newPermission);
        await _db.SaveChangesAsync(cancellationToken);

        return newPermission;
    }

    private async Task<User> GetUserOrThrow(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
        {
            throw new NotFoundException(nameof(User), userId);
        }

        return user;
    }

    private static void ValidatePermission(string permission)
    {
        if (string.IsNullOrWhiteSpace(permission))
        {
            throw new IdentityException("Permission is required.");
        }
    }

    private static void ThrowIfFailed(IdentityResult result)
    {
        if (!result.Succeeded)
        {
            throw new IdentityException(result.Errors.Select(e => e.Description));
        }
    }
}
