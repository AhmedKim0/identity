using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Application.DTO.PermissionDTOs
{
    public class AssignPermissionsDTO
    {
        public int RoleId { get; set; } = default!;
        public List<int> PermissionIds { get; set; } = new();
    }
}
