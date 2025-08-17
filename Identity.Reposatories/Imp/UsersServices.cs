using Identity.Application.DTO;
using Identity.Application.DTO.UserDTOs;
using Identity.Application.Int;
using Identity.Application.UOW;
using Identity.Domain.Entities;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using System.Data;
using System.Text;

namespace Identity.Application.Imp
{
    public class UsersServices : IUserServices
    {
        private readonly IUnitOfWork _unitOfWork;

        public UsersServices( IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }
        public async Task<Response<bool>> CreateTest(string email, string password, string fullName)
        {
            await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted);
            try
            {
                var userExists = await _unitOfWork._UserManager.FindByEmailAsync(email);
                if (userExists != null)
                {
                    throw new Exception("User already exists");
                }
                if (!IsValidEmail(email))
                {
                    throw new Exception("Invalid email format");
                }
                email = NormalizeEmail(email);
                var user = new AppUser
                {
                    UserName = email,
                    Email = email
                };
                var result = await _unitOfWork._UserManager.CreateAsync(user, password);
                throw new Exception("User not created successfully");

                if (!result.Succeeded)
                {
                    var errors = result.Errors.Select(e => new Error
                    (
                        e.Description,
                         e.Code
                    )).ToList();
                }
                await _unitOfWork.CommitTransactionAsync();
                return Response<bool>.SuccessResponse(true);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw new Exception(ex.Message);
            }
        }

        public async Task<Response<List<UserDTO>>> GetAllUsers()
        {
            var users = await _unitOfWork._UserManager.Users
                .Select(u => new UserDTO
                {
                    Id = u.Id,
                    Email = u.Email,
                    UserName = u.UserName
                })
                .ToListAsync();

            return Response<List<UserDTO>>.SuccessResponse(users);
        }

        public async Task<Response<UserDTO>> CreateUserAsync(string email, string password, string fullName)
        {
            await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted);
            try
            {
                var userExists = await _unitOfWork._UserManager.FindByEmailAsync(email);
                if (userExists != null)
                {
                    return Response<UserDTO>.Failure(new Error("User already exists"));
                }
                if (!IsValidEmail(email))
                {
                    return Response<UserDTO>.Failure(new Error("Invalid email format"));
                }
                email = NormalizeEmail(email);

                var user = new AppUser
                {
                    UserName = email,
                    Email = email
                };

                var result = await _unitOfWork._UserManager.CreateAsync(user, password);
                if (!result.Succeeded)
                {
                    var errors = result.Errors.Select(e => new Error
                    (
                        e.Description,
                         e.Code
                    )).ToList();

                    return Response<UserDTO>.Failure(errors);
                }
                //await _userManager.AddToRoleAsync(user, "norole"); // Add user to default role

                await _unitOfWork.CommitTransactionAsync();
                var userDto = new UserDTO
                {
                    Id = user.Id,
                    Email = user.Email,
                    UserName = user.UserName,
                };

                return Response<UserDTO>.SuccessResponse(userDto);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return Response<UserDTO>.Failure(new Error(ex.Message));
            }
        }

        public async Task<Response<UserDTO>> UpdateUserAsync(int userId, string newEmail, string newFullName)
        {
            var user = await _unitOfWork._UserManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                return Response<UserDTO>.Failure(new Error("User not found"));
            }
            if (!IsValidEmail(newEmail))
            {
                return Response<UserDTO>.Failure(new Error("Invalid email format"));
            }
            newEmail = NormalizeEmail(newEmail);

            user.Email = newEmail;
            user.UserName = newEmail;

            var result = await _unitOfWork._UserManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => new Error(

                     e.Description,
                     e.Code
               )).ToList();

                return Response<UserDTO>.Failure(errors);
            }
            var userDto = new UserDTO
            {
                Id = user.Id,
                Email = user.Email,
                UserName = user.UserName,
            };

            return Response<UserDTO>.SuccessResponse(userDto);
        }


        public async Task<Response<string>> DeleteUserAsync(int userId)
        {
            var user = await _unitOfWork._UserManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                return Response<string>.Failure(new Error("User not found"));
            }

            var result = await _unitOfWork._UserManager.DeleteAsync(user);
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
        private bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
        public string NormalizeEmail(string email)
        {
            var parts = email.Split('@');
            if (parts.Length != 2)
                return email;

            var local = parts[0];
            var domain = parts[1].ToLower();

            if (domain == "gmail.com" || domain == "googlemail.com")
            {
                // Remove everything after +
                var plusIndex = local.IndexOf('+');
                if (plusIndex >= 0)
                    local = local.Substring(0, plusIndex);

                // Remove dots (Gmail ignores dots in username)
                local = local.Replace(".", "");
            }

            return $"{local}@{domain}";
        }
    }
}
