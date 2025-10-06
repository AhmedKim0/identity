using Identity.Domain.SharedEntities;

using Microsoft.AspNetCore.Identity;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Domain.Entities
{
    public class AppRole : IdentityRole<int>, IAuditableEntity, IBaseEntity
    {
        public string NameAr { get; set; } = null!;
        public string NameEn { get; set; } = null!;
        public DateTime CreatedAtUtc { get; set; }
        public int CreatedBy { get; set; }
        public DateTime? UpdatedAtUtc { get; set; }
        public int? UpdatedBy { get; set; }
        public bool IsDeleted { get; set; } = false;

        public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    }

}
