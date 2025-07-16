using Identity.Domain.Entities;

using Microsoft.AspNetCore.Identity;

namespace Identity.Reposatories.Repos
{
    public class UsersServices
    {
        private readonly UserManager<AppUser> _userManager;

        public UsersServices(UserManager<AppUser> userManager)
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        }
        public async Task<IdentityResult> CreateUserAsync(string email, string password, string fullName)
        {
            var userExists = await _userManager.FindByEmailAsync(email);

            var user = new AppUser
            {
                UserName = email,
                Email = email,
            };

            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded)
                {
                return IdentityResult.Failed(new IdentityError { Description = string.Join(" \n ", result.Errors.Select(x => x.Description)) });

            }

            return result;
        }
        //otp

        public async Task<IdentityResult> UpdateUserAsync(int userId, string newEmail, string newFullName /*,string otp*/)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());

            if (user == null) return IdentityResult.Failed(new IdentityError { Description = "User not found" });

            user.Email= newEmail;
            user.UserName = newEmail;

           var result = await _userManager.UpdateAsync(user);
            return result;
        }
        public async Task<IdentityResult> DeleteUserAsync(int userId /*,string otp*/ )
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());

            if (user == null) return IdentityResult.Failed(new IdentityError { Description = "User not found" });

            // otp check
            var result = await _userManager.DeleteAsync(user);
            return result;
        }


    }
}
