using EmployeeManagementService.Domain.Constants;
using EmployeeManagementService.Domain.Models;
using EmployeeManagementService.Infrastructure.Identity.Authorization;
using EmployeeManagementService.Infrastructure.Identity.UserData;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagementService.Infrastructure.Persistence.Seeding;

public static class IdentitySeeder
{
    public const string TestAdminEmail = "admin@digi.local";
    public const string TestAdminPassword = "Admin123!";

    public static async Task SeedAsync(
        EmployeeManagementServiceDbContext dbContext,
        RoleManager<IdentityRole> roleManager,
        UserManager<User> userManager,
        CancellationToken cancellationToken = default)
    {
        foreach (var roleName in GetAllRoles())
        {
            await EnsureRole(roleManager, roleName);
        }

        var permissions = await EnsurePermissions(dbContext, cancellationToken);
        await EnsureRolePermissions(dbContext, roleManager, permissions, cancellationToken);
        await EnsureTestAdmin(userManager);
    }

    private static IEnumerable<string> GetAllRoles()
        => new[] { "Admin" }.Concat(Roles.All);

    private static async Task EnsureRole(RoleManager<IdentityRole> roleManager, string roleName)
    {
        if (await roleManager.RoleExistsAsync(roleName))
        {
            return;
        }

        var result = await roleManager.CreateAsync(new IdentityRole(roleName));
        ThrowIfFailed(result);
    }

    private static async Task<Dictionary<string, Permission>> EnsurePermissions(
        EmployeeManagementServiceDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var seededPermissions = new[]
        {
            Permission(Permissions.BoardProposal_Read, "Read board proposal requests and materials"),
            Permission(Permissions.BoardProposal_CreateMeeting, "Create board proposal meetings"),
            Permission(Permissions.BoardProposal_AddAgendaItems, "Add agenda items to board proposal requests"),
            Permission(Permissions.BoardProposal_UploadMaterials, "Upload board proposal materials"),
            Permission(Permissions.BoardProposal_SendAgenda, "Send board proposal agenda packages"),
            Permission(Permissions.BoardProposal_WorkflowNextStep, "Move board proposal requests through workflow statuses"),
            Permission(Permissions.BoardProposal_RegisterDecisions, "Register board proposal decisions and votes"),
            Permission(Permissions.BoardProposal_CreateTasks, "Create board proposal tasks"),
            Permission(Permissions.BoardProposal_ViewTasks, "View board proposal tasks"),
            Permission(Permissions.BoardProposal_UpdateTaskStatus, "Update board proposal task statuses")
        };

        var names = seededPermissions.Select(x => x.Name).ToArray();
        var existing = await dbContext.Permissions
            .Where(x => names.Contains(x.Name))
            .ToDictionaryAsync(x => x.Name, StringComparer.OrdinalIgnoreCase, cancellationToken);

        foreach (var permission in seededPermissions)
        {
            if (existing.TryGetValue(permission.Name, out var current))
            {
                current.Description = permission.Description;
                continue;
            }

            dbContext.Permissions.Add(permission);
            existing[permission.Name] = permission;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return existing;
    }

    private static async Task EnsureRolePermissions(
        EmployeeManagementServiceDbContext dbContext,
        RoleManager<IdentityRole> roleManager,
        IReadOnlyDictionary<string, Permission> permissions,
        CancellationToken cancellationToken)
    {
        var rolePermissions = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            ["Admin"] = [PermissionClaims.RolesManage, .. Permissions.All],
            [Roles.BoardProposal_SecretaryAdmin] = Permissions.All,
            [Roles.BoardProposal_BoardMember] =
            [
                Permissions.BoardProposal_Read,
                Permissions.BoardProposal_ViewTasks
            ],
            [Roles.BoardProposal_TaskOwner] =
            [
                Permissions.BoardProposal_Read,
                Permissions.BoardProposal_ViewTasks,
                Permissions.BoardProposal_UpdateTaskStatus
            ],
            [Roles.BoardProposal_UserObserver] =
            [
                Permissions.BoardProposal_Read,
                Permissions.BoardProposal_ViewTasks,
                Permissions.BoardProposal_UpdateTaskStatus
            ],
            [Roles.BoardProposal_AuditObserver] =
            [
                Permissions.BoardProposal_Read,
                Permissions.BoardProposal_ViewTasks
            ]
        };

        foreach (var (roleName, permissionNames) in rolePermissions)
        {
            var role = await roleManager.FindByNameAsync(roleName);
            if (role is null)
            {
                continue;
            }

            foreach (var permissionName in permissionNames)
            {
                if (!permissions.TryGetValue(permissionName, out var permission))
                {
                    continue;
                }

                var alreadyBound = await dbContext.RolePermissions
                    .AnyAsync(
                        x => x.RoleId == role.Id && x.PermissionId == permission.Id,
                        cancellationToken);

                if (alreadyBound)
                {
                    continue;
                }

                dbContext.RolePermissions.Add(new RolePermission
                {
                    RoleId = role.Id,
                    PermissionId = permission.Id
                });
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static async Task EnsureTestAdmin(UserManager<User> userManager)
    {
        var user = await userManager.FindByEmailAsync(TestAdminEmail);
        if (user is null)
        {
            user = new User(TestAdminEmail);
            var createResult = await userManager.CreateAsync(user, TestAdminPassword);
            ThrowIfFailed(createResult);
        }

        foreach (var roleName in GetAllRoles())
        {
            if (await userManager.IsInRoleAsync(user, roleName))
            {
                continue;
            }

            var addRoleResult = await userManager.AddToRoleAsync(user, roleName);
            ThrowIfFailed(addRoleResult);
        }
    }

    private static Permission Permission(string name, string description)
        => new()
        {
            Name = name,
            Description = description,
            CreatedAt = DateTimeOffset.UtcNow
        };

    private static void ThrowIfFailed(IdentityResult result)
    {
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(
                string.Join("; ", result.Errors.Select(x => x.Description)));
        }
    }
}
