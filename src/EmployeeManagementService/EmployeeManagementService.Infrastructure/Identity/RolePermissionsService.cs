using EmployeeManagementService.Application.Exceptions;
using EmployeeManagementService.Application.Features.Identity;
using EmployeeManagementService.Domain.Models;
using EmployeeManagementService.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagementService.Infrastructure.Identity;

internal class RolePermissionsService : IRolePermissions
{
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly EmployeeManagementServiceDbContext _db;

    public RolePermissionsService(
        RoleManager<IdentityRole> roleManager,
        EmployeeManagementServiceDbContext db)
    {
        _roleManager = roleManager;
        _db = db;
    }

    public async Task<IReadOnlyList<string>> GetAllPermissions(CancellationToken cancellationToken = default)
    {
        return await _db.Permissions
            .Select(p => p.Name)
            .OrderBy(n => n)
            .ToArrayAsync(cancellationToken);
    }

    public async Task CreatePermission(string permission, CancellationToken cancellationToken = default)
    {
        ValidatePermission(permission);
        await GetOrCreatePermission(permission, cancellationToken);
    }

    public async Task<IReadOnlyList<string>> GetPermissions(string roleName, CancellationToken cancellationToken = default)
    {
        var role = await GetRoleOrThrow(roleName);

        return await _db.RolePermissions
            .Where(rp => rp.RoleId == role.Id)
            .Select(rp => rp.Permission.Name)
            .OrderBy(n => n)
            .ToArrayAsync(cancellationToken);
    }

    public async Task AddPermission(string roleName, string permission, CancellationToken cancellationToken = default)
    {
        ValidatePermission(permission);

        var role = await GetRoleOrThrow(roleName);

        var alreadyBound = await _db.RolePermissions
            .AnyAsync(rp => rp.RoleId == role.Id && rp.Permission.Name == permission, cancellationToken);

        if (alreadyBound)
        {
            return;
        }

        var permissionEntity = await GetOrCreatePermission(permission, cancellationToken);

        _db.RolePermissions.Add(new RolePermission
        {
            RoleId = role.Id,
            PermissionId = permissionEntity.Id
        });

        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task RemovePermission(string roleName, string permission, CancellationToken cancellationToken = default)
    {
        ValidatePermission(permission);

        var role = await GetRoleOrThrow(roleName);

        var binding = await _db.RolePermissions
            .FirstOrDefaultAsync(rp => rp.RoleId == role.Id && rp.Permission.Name == permission, cancellationToken);

        if (binding is null)
        {
            return;
        }

        _db.RolePermissions.Remove(binding);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<string>> GetRoles(CancellationToken cancellationToken = default)
    {
        return await _roleManager.Roles
            .Select(r => r.Name!)
            .OrderBy(n => n)
            .ToArrayAsync(cancellationToken);
    }

    public async Task CreateRole(string roleName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(roleName))
        {
            throw new IdentityException("Role name is required.");
        }

        if (await _roleManager.RoleExistsAsync(roleName))
        {
            return;
        }

        var result = await _roleManager.CreateAsync(new IdentityRole(roleName));
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

    private async Task<IdentityRole> GetRoleOrThrow(string roleName)
    {
        var role = await _roleManager.FindByNameAsync(roleName);
        if (role is null)
        {
            throw new NotFoundException(nameof(IdentityRole), roleName);
        }

        return role;
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
