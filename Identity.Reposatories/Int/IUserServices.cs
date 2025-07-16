using Microsoft.AspNetCore.Identity;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Application.Int
{
    internal interface IUserServices
    {
        Task<IdentityResult> DeleteUserAsync(int userId /*,string otp*/ );
        Task<IdentityResult> UpdateUserAsync(int userId, string newEmail, string newFullName /*,string otp*/);
        Task<IdentityResult> CreateUserAsync(string email, string password, string fullName);
    }
}
