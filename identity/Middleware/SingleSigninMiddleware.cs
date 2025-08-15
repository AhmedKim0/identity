using global::Identity.Application.DTO.LoginDTOs;
using global::Identity.Application.Reposatory;
using global::Identity.Domain.Entities;

using Identity.Application.Int;

using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

using System.Security.Claims;
namespace Identity.API.Middleware
{


    public class SingleSigninMiddleware
    {
        private readonly RequestDelegate _next;

        public SingleSigninMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var jwtsettings = context.RequestServices.GetRequiredService<JwtSettings>();

            if (jwtsettings.SingleSignon)
            {
                var isAuthorized = context.GetEndpoint()?.Metadata?.GetMetadata<AuthorizeAttribute>() != null;

                //var userTokenRepo = context.RequestServices.GetRequiredService<IAsyncRepository<UserToken>>();
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

                    //var userToken = await userTokenRepo.Dbset().FirstOrDefaultAsync(ut => ut.UserId == int.Parse(userId));
                    if (userToken == null) {
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        await context.Response.WriteAsync("Unauthorized");
                        return;
                    }
                    if(userToken.ATExpiryDate<=DateTime.UtcNow)
                    {
                        await redisCacheService.RemoveAsync($"UserToken:{userId}");
                        //userTokenRepo.Dbset().Remove(userToken);
                        //await userTokenRepo.SaveChangesAsync();
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

