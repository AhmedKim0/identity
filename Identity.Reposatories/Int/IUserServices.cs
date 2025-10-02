using Identity.Application.DTO;
using Identity.Application.DTO.UserDTOs;

namespace Identity.Application.Int
{
    public interface IUserServices
    {
        Task<Response<List<UserDTO>>> GetAllUsers();

        Task<Response<string>> DeleteUserAsync(int userId);
        Task<Response<UserDTO>> CreateUserAsync(string? email, string password, string username, string? phone);
        Task<Response<UserDTO>> GetUserByIdAsync(int id);
        Task<Response<UserDTO>> UpdateUserAsync(int userId, string newEmail, string newFullName, string newPhone);
    }
}
