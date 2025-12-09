using Identity.Application.DTO.LoginDTOs;
using Identity.Application.Int;

using Microsoft.AspNetCore.Authorization;

using System.Linq;
using System.Security.Claims;

namespace Identity.API.Middleware
{
    public class DynamicAuthorizationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IInMemory _inMemory;

        public DynamicAuthorizationMiddleware(RequestDelegate next , IInMemory inMemory)
        {
            _next = next;
            _inMemory = inMemory;
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

            var permissions = context.User.Claims
                .Where(c => c.Type == "Permission")
                .Select(c => c.Value.ToLower())
                .ToList();
            var requiredPermission = GetRequiredPermission(context);
            var permissionFromCach = await _inMemory.GetPermissionByNameAsync(requiredPermission);

                if (permissions.Contains(permissionFromCach.Id.ToString()))
                {
                    await _next(context);
                    return;
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
