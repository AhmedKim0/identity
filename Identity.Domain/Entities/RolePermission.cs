using Identity.Domain.SharedEntities;

namespace Identity.Domain.Entities
{
    public class RolePermission:AuditableEntity, IAuditableEntity, IBaseEntity
    {
        public int RoleId { get; set; }
        public AppRole Role { get; set; }

        public int PermissionId { get; set; }
        public Permission Permission { get; set; }
    }

}
