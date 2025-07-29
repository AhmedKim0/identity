using Identity.Application.Int;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

using System.Security.Claims;

namespace Identity.Application.Repos
{
    public class DynamicAuthorizationMiddleware
    {
        private readonly RequestDelegate _next;

        public DynamicAuthorizationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var roles = context.User.Claims
                .Where(c => c.Type == ClaimTypes.Role)
                .Select(c => c.Value.ToLower())
                .ToList();
            if ( roles.Contains("admin") )
            {
                await _next(context);
                return;
            }

            var policyStore = context.RequestServices.GetRequiredService<IPolicyStore>();

            var requiredPermission = GetRequiredPermission(context);

            if (string.IsNullOrWhiteSpace(requiredPermission))
            {
                await _next(context);
                return;
            }

            if (!context.Request.Headers.TryGetValue("Authorization", out var authHeader) || string.IsNullOrWhiteSpace(authHeader)||!context.User.Identity.IsAuthenticated)
            {
                var permissions = await policyStore.GetPermissionsForRoleAsync("NoRole");
                if (permissions.Contains(requiredPermission))
                {
                    await _next(context);
                    return;
                }

                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Access token is missing or unauthorized");
                return;
            }




            foreach (var role in roles)
            {
                var permissions = await policyStore.GetPermissionsForRoleAsync(role);
                if (permissions.Contains(requiredPermission))
                {
                    await _next(context);
                    return;
                }
            }

            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsync("Forbidden");
        }

        private string? GetRequiredPermission(HttpContext context)
        {
            var path = context.Request.Path.Value?.Trim('/').ToLower().Replace("api/", "");
           
            if (string.IsNullOrWhiteSpace(path))
                return null;

            // for example: api/cars/create → Cars.Create
            return path.Replace("/", ".");
        }
    }
}
