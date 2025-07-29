using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Application.DTO.UserDTOs
{
    public class CreateUserDTO
    {
        public string email { get; set; }
        public string password { get; set; }
        public string fullName { get; set; }
    }
}
