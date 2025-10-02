using Identity.Domain.Enums;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Application.DTO.LoginDTOs
{
    public class LoginDTO
    {
        public string LoginKey { get; set; }
        public string Password { get; set; }
        public LoginBy loginBy { get; set; }
    }

    public class RefreshTokenDTO
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
    }

}
