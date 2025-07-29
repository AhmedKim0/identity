using Identity.Application.DTO.LoginDTOs;
using Identity.Application.Int;
using Identity.Application.Reposatory;
using Identity.DAL;
using Identity.Domain.Entities;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

public class TokenService: ITokenService
{
    private readonly JwtSettings _jwtSettings;
    private readonly UserManager<AppUser> _userManager;
    private readonly RoleManager<AppRole> _roleManager;
    private readonly IAsyncRepository<RolePermission> _rolePermissionRepo;

    public TokenService(JwtSettings jwtSettings, UserManager<AppUser> userManager, RoleManager<AppRole> roleManager, 
        IAsyncRepository<RolePermission> rolePermissionRepo)
    {
        _jwtSettings = jwtSettings ?? throw new ArgumentNullException(nameof(jwtSettings));
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        _roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
        _rolePermissionRepo = rolePermissionRepo ?? throw new ArgumentNullException(nameof(rolePermissionRepo));
    }

    public async Task<(string token, string refreshToken)> GenerateTokens(AppUser user)
    {
        var authClaims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.UserName!),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var roles = await _userManager.GetRolesAsync(user);
        var roleclaim = roles.Select(r=>new Claim(ClaimTypes.Role,r));
        authClaims.AddRange(roleclaim);
        var permissionNames = new List<string>();

        foreach (var roleName in roles)
        {
            var role = await _roleManager.FindByNameAsync(roleName);
            if(role==null)
                throw new NullReferenceException();

            var permissions = await _rolePermissionRepo.Dbset()
                .Where(rp => rp.RoleId == role.Id && rp.Permission != null)
                .Include(rp => rp.Permission)
                .Select(rp => rp.Permission.Name)
                .ToListAsync();

            permissionNames.AddRange(permissions);
        }

        //Add to Claims
       var claims = permissionNames
           .Distinct()
           .Select(p => new Claim("Permission", p))
           .ToList();
        authClaims.AddRange(claims);
        var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));
        
        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
            claims: authClaims,
            signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
        );

        var accessToken = new JwtSecurityTokenHandler().WriteToken(token);
        var refreshToken = Guid.NewGuid().ToString();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays);
        await _userManager.UpdateAsync(user);

        return (accessToken, refreshToken);
    }
}
