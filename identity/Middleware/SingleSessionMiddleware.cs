using global::Identity.Application.DTO.LoginDTOs;
using global::Identity.Domain.Entities;

using Identity.Application.Int;

using Microsoft.AspNetCore.Authorization;

using System.Security.Claims;
namespace Identity.API.Middleware
{


    public class SingleSessionMiddleware
    {
        private readonly RequestDelegate _next;

        public SingleSessionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var jwtsettings = context.RequestServices.GetRequiredService<JwtSettings>();

            if (jwtsettings.SingleSession)
            {
                var isAuthorized = context.GetEndpoint()?.Metadata?.GetMetadata<AuthorizeAttribute>() != null;

                var redisCacheService = context.RequestServices.GetRequiredService<IRedisCacheService?>();
                var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (isAuthorized)
                {
                    if (userId == null)//isauthrized
                    {
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        await context.Response.WriteAsync("Unauthorized");
                        return;
                    }
                    var userToken=await redisCacheService.GetAsync<UserToken>($"UserToken:{userId}");

                    if (userToken == null) {
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        await context.Response.WriteAsync("Unauthorized");
                        return;
                    }
                    if(userToken.ATExpiryDate<=DateTime.UtcNow)
                    {
                        await redisCacheService.RemoveAsync($"UserToken:{userId}");

                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        await context.Response.WriteAsync("Unauthorized");
                        return;

                    }
                }
            }
            await _next(context);
        }

    }

}

