using Identity.Application.DTO;
using Identity.Application.DTO.UserDTOs;
using Identity.Application.Int;
using Identity.Application.Reposatory;
using Identity.Application.UOW;
using Identity.Domain.Entities;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using System.Text;

namespace Identity.Application.Imp
{
    public class UsersServices : IUserServices
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAsyncRepository<OTPCode> _otpRepo;

        public UsersServices(UserManager<AppUser> userManager, IUnitOfWork unitOfWork)
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task<Response<List<UserDTO>>> GetAllUsers()
        {
            var users = await _userManager.Users
                .Select(u => new UserDTO
                {
                    Id = u.Id,
                    Email = u.Email,
                    UserName = u.UserName
                })
                .ToListAsync();

            return Response<List<UserDTO>>.SuccessResponse(users);
        }

        public async Task<Response<string>> CreateUserAsync(string email, string password, string fullName)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var userExists = await _userManager.FindByEmailAsync(email);
                if (userExists != null)
                {
                    return Response<string>.Failure(new Error("User already exists"));
                }

                var user = new AppUser
                {
                    UserName = email,
                    Email = email
                };

                var result = await _userManager.CreateAsync(user, password);
                if (!result.Succeeded)
                {
                    var errors = result.Errors.Select(e => new Error
                    (
                        e.Description,
                         e.Code
                    )).ToList();

                    return Response<string>.Failure(errors);
                }
                await _userManager.AddToRoleAsync(user, "norole"); // Add user to default role

                await _unitOfWork.CommitTransactionAsync();


                return Response<string>.SuccessResponse("User created successfully");
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return Response<string>.Failure(new Error(ex.Message));
            }
        }

        public async Task<Response<string>> UpdateUserAsync(int userId, string newEmail, string newFullName)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                return Response<string>.Failure(new Error("User not found"));
            }

            user.Email = newEmail;
            user.UserName = newEmail;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => new Error(

                     e.Description,
                     e.Code
               )).ToList();

                return Response<string>.Failure(errors);
            }

            return Response<string>.SuccessResponse("User updated successfully");
        }


        public async Task<Response<string>> DeleteUserAsync(int userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                return Response<string>.Failure(new Error("User not found"));
            }

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => new Error(

                     e.Description,
                     e.Code
                )).ToList();

                return Response<string>.Failure(errors);
            }

            return Response<string>.SuccessResponse("User deleted successfully");
        }
    }
}
