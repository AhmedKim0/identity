using Identity.Application.Int;

using Microsoft.AspNetCore.Authorization;

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
            var isAllowed = context.GetEndpoint()?.Metadata?.GetMetadata<AuthorizeAttribute>() ==null;
            var roles = context.User.Claims
                .Where(c => c.Type == ClaimTypes.Role)
                .Select(c => c.Value.ToLower())
                .ToList();
            if(isAllowed || roles.Contains("admin"))
            {
                await _next(context);
                return;
            }

            var policyStore = context.RequestServices.GetRequiredService<IPolicyStore>();

            var requiredPermission = GetRequiredPermission(context);

            //if (string.IsNullOrWhiteSpace(requiredPermission))
            //{
            //    await _next(context);
            //    return;
            //}

            //if (!context.Request.Headers.TryGetValue("Authorization", out var authHeader) || string.IsNullOrWhiteSpace(authHeader) || !context.User.Identity.IsAuthenticated)
            //{
            //    var permissions = await policyStore.GetPermissionsForRoleAsync("NoRole");
            //    if (permissions.Contains(requiredPermission))
            //    {
            //        await _next(context);
            //        return;
            //    }

            //    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            //    await context.Response.WriteAsync("Access token is missing or unauthorized");
            //    return;
            //}am




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
