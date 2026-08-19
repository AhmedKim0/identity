using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Application.DTO.RoleDTOs
{
    public  class RoleDTO
    {
        public int Id { get; set; }
        public string LogicalName { get; set; } = default!;

        public string NameEn { get; set; } = default!;
        public string NameAr { get; set; } = default!;

    }
}
