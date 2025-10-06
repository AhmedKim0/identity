using Identity.Domain.SharedEntities;

namespace Identity.Domain.Entities
{
    public class Permission : AuditableEntity, IAuditableEntity, IBaseEntity
    {
        public string NameLogical { get; set; } = null!; // login.isloggedin // user.create // user.delete
        public string NameAr { get; set; } = null!;
        public string NameEn { get; set; } = null!;
        public ICollection< RolePermission> RolePermissions { get; set; } = null!;
    }

}
