using Identity.Application.DTO;
using Identity.Application.DTO.PermissionDTOs;

namespace Identity.Application.Int
{
    public interface IPermissionService
    {
        Task<Response<List<PermissionDTO>>> GetAllAsync();
        Task<Response<PermissionDTO?>> GetByIdAsync(int id);
        Task<Response<PermissionDTO>> CreateAsync(string name);
        Task<Response<PermissionDTO?>> UpdateAsync(PermissionDTO dto);
        Task<Response<bool>> DeleteAsync(int id);
        Task<Response<bool>> AssignPermissionsToRoleAsync(int roleId, List<int> permissionIds);
        Task<Response<List<PermissionDTO>>> GetPermissionsByRoleAsync(int roleId);

    }
}
