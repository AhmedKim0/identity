using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace MCDRServices.Requests.Api.Mobile.Controllers
{
    public class BaseController : ControllerBase
    {
        protected string? GetUserIdFromToken()
        {
            if (!Request.Headers.TryGetValue(HeaderNames.Authorization, out var authHeader))
                return null;

            var token = authHeader.ToString().Split(" ").LastOrDefault();
            if (string.IsNullOrWhiteSpace(token))
                return null;

            var handler = new JwtSecurityTokenHandler();
            var jwtSecurityToken = handler.ReadJwtToken(token);

            var userId = jwtSecurityToken.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier)?.Value;
            return userId;
        }

    }
}
