using Identity.Application.DTO;
using Identity.Application.DTO.RoleDTOs;
using Identity.Domain.Entities;

using Microsoft.AspNetCore.Identity;

namespace Identity.Application.Int
{


    public interface IRoleService
    {
        Task<Response<List<AppRole>>> GetAllAsync();
        Task<Response<AppRole?>> GetByIdAsync(int id);
        Task<Response<RoleDTO>> CreateAsync(string roleName);
        Task<Response<RoleDTO>> DeleteAsync(int id);
        Task<Response<AssginRoleToUserDTO>> AssignRoleToUserAsync(int userId, string roleName);
        Task<Response<AssginRoleToUserDTO>> RemoveRoleFromUserAsync(int userId, string roleName);
    }

}
