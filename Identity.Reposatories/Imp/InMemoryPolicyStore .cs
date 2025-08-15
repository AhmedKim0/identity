//using Identity.Application.Int;
//using Identity.Application.Reposatory;
//using Identity.Domain.Entities;

//using Microsoft.EntityFrameworkCore;
//using Microsoft.Extensions.Caching.Memory;

//namespace Identity.Application.Imp
//{
//    public class InMemoryPolicyStore : IPolicyStore
//    {
//        private readonly IMemoryCache _cache;
//        private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(10);
//        private readonly IAsyncRepository<RolePermission> _rolePermissionRepo;

//        public InMemoryPolicyStore(IMemoryCache cache, IAsyncRepository<RolePermission> rolePermissionRepo)
//        {
//            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
//            _rolePermissionRepo = rolePermissionRepo ?? throw new ArgumentNullException(nameof(rolePermissionRepo));
//        }

//        public async Task<List<string>> GetPermissionsForRoleAsync(string role)
//        {
//            if (string.IsNullOrEmpty(role))
//            {
//                return new List<string>();
//            }
//            if(_cache.TryGetValue($"{role}", out List<string?> permissions))
//            {
//                return permissions;
//            }

//            var data = await _rolePermissionRepo.Dbset().Where(r => r.Role.Name.ToLower() == role.ToLower()).Select(p => p.Permission.Name).ToListAsync();
//            _cache.CreateEntry($"{role}").AbsoluteExpirationRelativeToNow = _cacheDuration;
//            _cache.Set($"{role}", data);
//            return data;


//        }
//    }

//}
