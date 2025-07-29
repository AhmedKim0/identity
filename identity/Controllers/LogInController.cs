using Identity.Application.DTO;
using Identity.Application.DTO.LoginDTOs;
using Identity.Application.Int;
using Identity.Domain.Entities;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Identity.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LogInController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly ITokenService _tokenService;
        private readonly JwtSettings _jwtSettings;

        public LogInController(UserManager<AppUser> userManager, ITokenService tokenService, JwtSettings jwtSettings)
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
            _jwtSettings = jwtSettings ?? throw new ArgumentNullException(nameof(jwtSettings));
        }
        [HttpPost("Login")]

        public async Task<IActionResult> Login([FromBody] LoginDTO model)
        {
            var user = await _userManager.FindByEmailAsync(model.Username);
            if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
                return Unauthorized(Response<TokenDTO>.Failure( new Error("Invalid username or password")));

            var (accessToken, refreshToken) = await _tokenService.GenerateTokens(user);

            return Ok(Response<TokenDTO>.SuccessResponse( new TokenDTO
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpireAt = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes)
            }));
        }
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDTO model)
        {
            var principal = GetPrincipalFromExpiredToken(model.AccessToken);
            var username = principal?.Identity?.Name;

            var user = await _userManager.FindByNameAsync(username!);
            if (user == null || user.RefreshToken != model.RefreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
                return Unauthorized(Response<LoginDTO>.Failure(new Error("Invalid refresh token")));

            var (newAccessToken, newRefreshToken) = await _tokenService.GenerateTokens(user);

            return Ok(Response<TokenDTO>.SuccessResponse(new TokenDTO
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
                ExpireAt = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes)
            }));
        }

        private ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret)),
                ValidateLifetime = false // Accept expired token
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            try
            {
                return tokenHandler.ValidateToken(token, tokenValidationParameters, out _);
            }
            catch
            {
                return null;
            }
        }

    }
}
