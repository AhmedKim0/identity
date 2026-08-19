using Identity.Domain.SharedEntities;

namespace Identity.Domain.Entities
{
    public class RolePermission : AuditableEntity, IBaseEntity
    {
        private RolePermission()
        {
            // Required by EF Core
        }

        public RolePermission(
            int roleId,
            int permissionId)
        {
            RoleId = roleId;
            PermissionId = permissionId;
        }

        public int RoleId { get; private set; }

        public AppRole Role { get; private set; } = null!;

        public int PermissionId { get; private set; }

        public Permission Permission { get; private set; } = null!;
    }

}
