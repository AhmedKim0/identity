using Identity.Application.DTO;
using Identity.Application.DTO.UserDTOs;
using Identity.Application.Int;
using Identity.Application.UOW;
using Identity.Domain.Entities;

using Microsoft.EntityFrameworkCore;

using System.Data;

namespace Identity.Application.Imp
{
    public class UsersServices : IUserServices
    {
        private readonly IUnitOfWork _unitOfWork;

        public UsersServices( IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
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
                email = SharedFunctions.NormalizeEmail(email);
                var userExists = await _unitOfWork._UserManager.FindByEmailAsync(email);
                if (userExists != null)
                {
                    return Response<UserDTO>.Failure(new Error("User already exists"));
                }
                if (!SharedFunctions.IsValidEmail(email))
                {
                    return Response<UserDTO>.Failure(new Error("Invalid email format"));
                }


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
            newEmail = SharedFunctions.NormalizeEmail(newEmail);
            var user = await _unitOfWork._UserManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                return Response<UserDTO>.Failure(new Error("User not found"));
            }
            if (!SharedFunctions.IsValidEmail(newEmail))
            {
                return Response<UserDTO>.Failure(new Error("Invalid email format"));
            }
            newEmail = SharedFunctions. NormalizeEmail(newEmail);

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
        
    }
}
