using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Application.DTO.LoginDTOs
{
    public class UserInfoDTO
    {
        public string Email { get; set; }
        public string UserName { get; set; }    
        public bool TwoFactorEnabled { get; set; } = false;
        public string? PhoneNumber { get; set; }
        public List<string> Roles { get; set; } = new List<string>();
    }
}
