using Identity.Domain.SharedEntities;

namespace Identity.Domain.Entities
{
    public class Permission : AuditableEntity, IBaseEntity
    {
        private readonly List<RolePermission> _rolePermissions = new();

        private Permission()
        {
            // Required by EF Core
        }

        public Permission(
            string nameLogical,
            string nameAr,
            string nameEn)
        {
            NameLogical = nameLogical;
            NameAr = nameAr;
            NameEn = nameEn;
        }

        public string NameLogical { get; private set; } = null!;

        public string NameAr { get; private set; } = null!;

        public string NameEn { get; private set; } = null!;

        public IReadOnlyCollection<RolePermission> RolePermissions =>
            _rolePermissions.AsReadOnly();

        public void Update(
            string nameLogical,
            string nameAr,
            string nameEn)
        {
            NameLogical = nameLogical;
            NameAr = nameAr;
            NameEn = nameEn;
        }

        public void AddRolePermission(RolePermission rolePermission)
        {
            ArgumentNullException.ThrowIfNull(rolePermission);

            if(_rolePermissions.Any(x =>
                x.RoleId == rolePermission.RoleId))
            {
                return;
            }

            _rolePermissions.Add(rolePermission);
        }

        public void RemoveRolePermission(int roleId)
        {
            var rolePermission = _rolePermissions
                .FirstOrDefault(x => x.RoleId == roleId);

            if(rolePermission is not null)
            {
                _rolePermissions.Remove(rolePermission);
            }
        }
    }

}
