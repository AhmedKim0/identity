using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Application.DTO.PermissionDTOs
{
    public class RolePermissionDTO
    {
        public string RoleName { get; set; }
        public List<string> Permissions { get; set; }
    }
}
