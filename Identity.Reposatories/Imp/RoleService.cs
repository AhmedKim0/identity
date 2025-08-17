using Identity.Application.DTO;
using Identity.Application.DTO.RoleDTOs;
using Identity.Application.Int;
using Identity.Application.UOW;
using Identity.DAL;
using Identity.Domain.Entities;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using System.Data;

namespace Identity.Application.Imp
{
    public class RoleService : IRoleService
    {

        private readonly IUnitOfWork _unitOfWork ;
        private readonly AppDbContext _context;

        public RoleService(RoleManager<AppRole> roleManager, UserManager<AppUser> userManager, IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task<Response<List<AppRole>>> GetAllAsync()
        {
            return Response<List<AppRole>>.SuccessResponse ( await _unitOfWork._RoleManager.Roles.ToListAsync());



        }

        public async Task<Response<AppRole?>> GetByIdAsync(int id)
        {
            return Response<AppRole>.SuccessResponse(await _unitOfWork._RoleManager.Roles.FirstOrDefaultAsync(r => r.Id == id));
        }

        public async Task<Response<RoleDTO>> CreateAsync(string roleName)
        {


                if (await _unitOfWork._RoleManager.RoleExistsAsync(roleName))
                    return Response<RoleDTO>.Failure(new Error("Role already exists"));

                var role = new AppRole { Name = roleName };
                var addedrole = await _unitOfWork._RoleManager.CreateAsync(role);
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
            var deletedrole=await _unitOfWork._RoleManager.DeleteAsync(role.Data);
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
        public async Task<Response<bool>> AssignRolesToUserAsync(int UserId, List<int> rolesIds)
        {
            await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted);
            try
            {
                
                var user = await _unitOfWork._UserManager.FindByIdAsync(UserId.ToString());
                if (user == null || user.Email== "admin@admin.com")
                { return Response<bool>.Failure(new Error("user not found")); }

                var Listroles =await _unitOfWork._RoleManager.Roles
                    .Where(r => rolesIds.Contains(r.Id))
                    .ToListAsync();
                var ListrolesIds= Listroles.Select(r => r.Id).ToList();
                var isAllRoleExist = rolesIds.All(id => ListrolesIds.Contains(id));
                if (!isAllRoleExist)
                { return Response<bool>.Failure(new Error("one Role or all not exist")); }



                var userRoles = _context.UserRoles?.Where(ur => ur.UserId == UserId)?.ToList();
                _context.UserRoles.RemoveRange(userRoles);

                var roleNames = Listroles.Select(r => r.Name).ToList();
                await _unitOfWork._UserManager.AddToRolesAsync(user, roleNames);
                await _unitOfWork.CommitTransactionAsync();
                return Response<bool>.SuccessResponse(true);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();

                throw ex;
            }

        }

        public async Task<Response<AssginRoleToUserDTO>> AssignRoleToUserAsync(int userId, string roleName)
        {
            var user = await _unitOfWork._UserManager.FindByIdAsync(userId.ToString());
            if (user == null)
                return Response<AssginRoleToUserDTO>.Failure(new Error("User not found."));

            if (!await _unitOfWork._RoleManager.RoleExistsAsync(roleName))
                return Response<AssginRoleToUserDTO>.Failure(new Error("Role not found."));
           var addedRoleResponse= await _unitOfWork._UserManager.AddToRoleAsync(user, roleName);
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
            var user = await _unitOfWork._UserManager.FindByIdAsync(userId.ToString());
            if (user == null)
                return Response<AssginRoleToUserDTO>.Failure(new Error("User not found."));
            if (!await _unitOfWork._RoleManager.RoleExistsAsync(roleName))
                return Response<AssginRoleToUserDTO>.Failure(new Error("Role not found."));
            var addedRoleResponse = await _unitOfWork._UserManager.RemoveFromRoleAsync(user, roleName);
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