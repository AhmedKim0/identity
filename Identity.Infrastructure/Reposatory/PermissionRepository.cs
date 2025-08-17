namespace Identity.Infrastructure.Reposatory
{
    using global::Identity.DAL;
    using global::Identity.Domain.Entities;
    using global::Identity.Domain.IReposatory;



    namespace Identity.Infrastructure.Reposatory
    {
        public class PermissionRepository : AsyncReposatory<Permission>, IPermissionRepository
        {
            public PermissionRepository(AppDbContext context) : base(context) { }
        }
    }

}
