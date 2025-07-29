
using Identity.Application.DTO;
using Identity.Application.DTO.RoleDTOs;
using Identity.Application.Int;
using Identity.Domain.Entities;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Identity.Application.Repos
{
    public class RoleService : IRoleService
    {
        private readonly RoleManager<AppRole> _roleManager;
        private readonly UserManager<AppUser> _userManager;

        public RoleService(RoleManager<AppRole> roleManager, UserManager<AppUser> userManager)
        {
            _roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        }

        public async Task<Response<List<AppRole>>> GetAllAsync()
        {
            return Response<List<AppRole>>.SuccessResponse ( await _roleManager.Roles.ToListAsync());



        }

        public async Task<Response<AppRole?>> GetByIdAsync(int id)
        {
            return Response<AppRole>.SuccessResponse(await _roleManager.Roles.FirstOrDefaultAsync(r => r.Id == id));
        }

        public async Task<Response<RoleDTO>> CreateAsync(string roleName)
        {


                if (await _roleManager.RoleExistsAsync(roleName))
                    return Response<RoleDTO>.Failure(new Error("Role already exists"));

                var role = new AppRole { Name = roleName };
                var addedrole = await _roleManager.CreateAsync(role);
                if (!addedrole.Succeeded)
                {
                    var errors = addedrole.Errors.Select(e => new Error(
                    
                         e.Description,
                        e.Code
                    )).ToList();
                    return Response<RoleDTO>.Failure(errors);
                }

                return Response<RoleDTO>.SuccessResponse(new RoleDTO {Id=role.Id,Name=role.Name });
            


        }

        public async Task<Response<RoleDTO>> DeleteAsync(int id)
        {
            var role = await GetByIdAsync(id);
            if (role.Data == null)
                return Response<RoleDTO>.Failure(new Error("Role not found"));
            var deletedrole=await _roleManager.DeleteAsync(role.Data);
            if (!deletedrole.Succeeded)
            {
                var errors = deletedrole.Errors.Select(e => new Error(

                     e.Description,
                    e.Code
                )).ToList();
                return Response<RoleDTO>.Failure(errors);
            }
            return Response<RoleDTO>.SuccessResponse(new RoleDTO { Id = role.Data.Id, Name = role.Data.Name });

        }

        public async Task<Response<AssginRoleToUserDTO>> AssignRoleToUserAsync(int userId, string roleName)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
                return Response<AssginRoleToUserDTO>.Failure(new Error("User not found."));

            if (!await _roleManager.RoleExistsAsync(roleName))
                return Response<AssginRoleToUserDTO>.Failure(new Error("Role not found."));
           var addedRoleResponse= await _userManager.AddToRoleAsync(user, roleName);
            if (!addedRoleResponse.Succeeded)
            {
                var errors = addedRoleResponse.Errors.Select(e => new Error(

                    e.Description,
                   e.Code
               )).ToList();
                return Response<AssginRoleToUserDTO>.Failure(errors);

            }

            return Response<AssginRoleToUserDTO>.SuccessResponse(new AssginRoleToUserDTO { userName = user.UserName, roleName = roleName });
        }

        public async Task<Response<AssginRoleToUserDTO>> RemoveRoleFromUserAsync(int userId, string roleName)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
                return Response<AssginRoleToUserDTO>.Failure(new Error("User not found."));
            if (!await _roleManager.RoleExistsAsync(roleName))
                return Response<AssginRoleToUserDTO>.Failure(new Error("Role not found."));
            var addedRoleResponse = await _userManager.RemoveFromRoleAsync(user, roleName);
            if (!addedRoleResponse.Succeeded)
            {
                var errors = addedRoleResponse.Errors.Select(e => new Error(

                    e.Description,
                   e.Code
               )).ToList();
                return Response<AssginRoleToUserDTO>.Failure(errors);

            }

            return Response<AssginRoleToUserDTO>.SuccessResponse(new AssginRoleToUserDTO { userName = user.UserName, roleName = roleName });
        }
}
}