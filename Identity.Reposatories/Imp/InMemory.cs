using Identity.Application.Int;
using Identity.Domain.Entities;
using Identity.Domain.IReposatory;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
namespace Identity.Application.Imp
{
    public class InMemory : IInMemory
    {
        private readonly IMemoryCache _cache;
        private readonly IServiceProvider _serviceProvider;
        private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(60);

        public InMemory(IMemoryCache cache, IServiceProvider serviceProvider)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public async Task<Permission?> GetPermissionByNameAsync(string permissionName)
        {
            if(string.IsNullOrWhiteSpace(permissionName))
                return null;

            if(_cache.TryGetValue(permissionName, out Permission? cachedPermission))
                return cachedPermission;

            using var scope = _serviceProvider.CreateScope();

            var repo = scope.ServiceProvider
                .GetRequiredService<IPermissionRepository>();

            var permission = await repo.Dbset()
                .FirstOrDefaultAsync(x => x.NameLogical == permissionName);

            if(permission != null)
            {
                _cache.Set(
                    permissionName,
                    permission,
                    new MemoryCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = _cacheDuration
                    });
            }

            return permission;
        }

        public async Task LoadAllPermissionsToCacheAsync()
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var repo = scope.ServiceProvider.GetRequiredService<IPermissionRepository>();
                var allPermissions = await repo.Dbset().ToListAsync();

                foreach (var perm in allPermissions)
                {
                    _cache.Set(perm.NameLogical, perm, new MemoryCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = _cacheDuration
                    });
                }
            }
        }
    }
}