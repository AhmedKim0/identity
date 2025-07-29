using Identity.Application.DTO;
using Identity.Application.DTO.UserDTOs;

namespace Identity.Application.Int
{
    public interface IUserServices
    {
        Task<Response<List<UserDTO>>> GetAllUsers();
        Task<Response<string>> CreateUserAsync(string email, string password, string fullName);
        Task<Response<string>> UpdateUserAsync(int userId, string newEmail, string newFullName);
        Task<Response<string>> DeleteUserAsync(int userId);
    }
}
