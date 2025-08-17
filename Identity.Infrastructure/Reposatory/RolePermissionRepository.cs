namespace Identity.Infrastructure.Reposatory
{
    using global::Identity.DAL;
    using global::Identity.Domain.Entities;
    using global::Identity.Domain.IReposatory;



    namespace Identity.Infrastructure.Reposatory
    {
        public class RolePermissionRepository : AsyncReposatory<RolePermission>, IRolePermissionRepository
        {
            public RolePermissionRepository(AppDbContext context) : base(context) { }
        }
    }

}
