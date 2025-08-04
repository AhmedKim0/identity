using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Application.DTO.RoleDTOs
{
    public class AssignRolesToUserDTO
    {
        public int userId { get; set; }

        public List<int> ids { get; set;}
    }
}
