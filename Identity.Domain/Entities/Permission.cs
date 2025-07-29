using Identity.Domain.SharedEntities;

namespace Identity.Domain.Entities
{
    public class Permission : AuditableEntity
    {
        public string Name { get; set; } = null!;
        public ICollection< RolePermission> RolePermissions { get; set; } = null!;
    }

}
