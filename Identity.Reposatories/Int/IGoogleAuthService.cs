using Identity.Application.DTO;
using Identity.Application.DTO.GoogleDTOs;
using Identity.Application.DTO.LoginDTOs;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Application.Int
{
    public interface IGoogleAuthService
    {
        string GetGoogleAuthUrl();
        Task<Response<TokenDTO?>> GetUserInfoAsync(string code);
    }
}
