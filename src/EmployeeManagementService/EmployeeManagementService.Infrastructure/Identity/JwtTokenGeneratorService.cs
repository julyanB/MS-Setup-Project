using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using EmployeeManagementService.Application;
using EmployeeManagementService.Infrastructure.Identity.Authorization;
using EmployeeManagementService.Infrastructure.Identity.UserData;
using EmployeeManagementService.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace EmployeeManagementService.Infrastructure.Identity;

public class JwtTokenGeneratorService : IJwtTokenGenerator
{
    private readonly ApplicationSettings _applicationSettings;
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly EmployeeManagementServiceDbContext _db;

    public JwtTokenGeneratorService(
        IOptions<ApplicationSettings> applicationSettings,
        UserManager<User> userManager,
        RoleManager<IdentityRole> roleManager,
        EmployeeManagementServiceDbContext db)
    {
        _applicationSettings = applicationSettings.Value;
        _userManager = userManager;
        _roleManager = roleManager;
        _db = db;
    }

    public async Task<string> GenerateToken(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_applicationSettings.Secret);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name, user.Email!)
        };

        var roles = await _userManager.GetRolesAsync(user);
        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

        var roleIds = await _roleManager.Roles
            .Where(r => roles.Contains(r.Name!))
            .Select(r => r.Id)
            .ToListAsync();

        var rolePermissions = await _db.RolePermissions
            .Where(rp => roleIds.Contains(rp.RoleId))
            .Select(rp => rp.Permission.Name)
            .Distinct()
            .ToListAsync();

        claims.AddRange(rolePermissions.Select(p => new Claim(PermissionClaims.Type, p)));

        var userPermissions = await _db.UserPermissions
            .Where(up => up.UserId == user.Id)
            .Select(up => up.Permission.Name)
            .ToListAsync();

        foreach (var permission in userPermissions)
        {
            claims.Add(new Claim(PermissionClaims.Type, permission));
            claims.Add(new Claim(PermissionClaims.UserPermissionType, permission));
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTimeOffset.UtcNow.AddDays(7).UtcDateTime,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}
