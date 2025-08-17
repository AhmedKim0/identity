using Identity.Application.DTO;
using Identity.Application.DTO.PermissionDTOs;
using Identity.Application.Int;
using Identity.Application.UOW;
using Identity.Domain.Entities;

using Microsoft.EntityFrameworkCore;

using System.Data;

namespace Identity.Application.Imp
{
    public class PermissionService : IPermissionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(10);

        public PermissionService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));

        }

        public async Task<Response<List<PermissionDTO>>> GetAllAsync()
        {
            return Response<List<PermissionDTO>>.SuccessResponse(await _unitOfWork.Permissions.Dbset()
                .Select(p => new PermissionDTO
                {
                    Id = p.Id,
                    Name = p.Name,
                }).ToListAsync());
        }

        public async Task<Response<PermissionDTO?>> GetByIdAsync(int id)
        {
            var permission = await _unitOfWork.Permissions.FirstOrDefaultAsync(x=>x.Id==id);
            return Response<PermissionDTO?>.SuccessResponse(new PermissionDTO
            {
                Id = permission?.Id,
                Name = permission?.Name,
            });
        }

        public async Task<Response<PermissionDTO>> CreateAsync(string name)
        {
            var permission = new Permission
            {
                Name = name,
            };
            await _unitOfWork.Permissions.AddAsync(permission);
            await _unitOfWork.Permissions.SaveChangesAsync();
            var dto = new PermissionDTO
            {
                Name = permission.Name,
            };
            dto.Id = permission.Id;
            return Response<PermissionDTO>.SuccessResponse(dto);
        }

        public async Task<Response<PermissionDTO?>> UpdateAsync(PermissionDTO dto)
        {
            var permission = await _unitOfWork.Permissions.FirstOrDefaultAsync(x => x.Id == dto.Id);
            if (permission == null) return Response<PermissionDTO>.Failure(new Error("Permission not found"));

            permission.Name = dto.Name;
            await _unitOfWork.Permissions.UpdateAsync(permission);
            await _unitOfWork.Permissions.SaveChangesAsync();

            if (permission == null) return Response<PermissionDTO>.Failure(new Error("Permission not found"));
            return Response<PermissionDTO>.SuccessResponse( dto);
        }

        public async Task<Response<bool>> DeleteAsync(int id)
        {
            var permission = await _unitOfWork.Permissions.FirstOrDefaultAsync(x => x.Id == id);
            if (permission == null) return Response<bool>.Failure(new Error("Permission not found"));
                
           await  _unitOfWork.Permissions.DeleteAsync(permission);
            await _unitOfWork.Permissions.SaveChangesAsync();
            return Response<bool>.SuccessResponse(true);
        }
        public async Task<Response<bool>> AssignPermissionsToRoleAsync(int roleId, List<int> permissionIds)
        {
            await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted);
            try
            {
                var role = await _unitOfWork._RoleManager.FindByIdAsync(roleId.ToString());
                if (role == null)
                { return Response<bool>.Failure(new Error("Role not found")); }
                var existingPermissionIds = await _unitOfWork.Permissions
                    .Dbset()
                    .Where(p => permissionIds.Contains(p.Id))
                    .Select(p => p.Id)
                    .ToListAsync();

                var isAllPermissionExist = permissionIds.All(id => existingPermissionIds.Contains(id));
                if (!isAllPermissionExist)
                { return Response<bool>.Failure(new Error("one permission or all not exist")); }

                var existing = await _unitOfWork.RolePermissions.Dbset().AsNoTracking().Where(rp => rp.RoleId == roleId).FirstOrDefaultAsync();
                if (existing != null) 
                await _unitOfWork.RolePermissions.DeleteAsync(existing);

                var newAssignments = permissionIds.Select(pid => new RolePermission
                {
                    RoleId = roleId,
                    PermissionId = pid
                });

                _unitOfWork.RolePermissions.Dbset().AddRange(newAssignments);

               await _unitOfWork.CommitTransactionAsync();
                return Response<bool>.SuccessResponse(true);
            }
            catch (Exception ex)
            {
              await  _unitOfWork.RollbackTransactionAsync();

                throw ex;
            }

        }

        public async Task<Response<List<PermissionDTO>>> GetPermissionsByRoleAsync(int roleId)
        {
            return Response<List<PermissionDTO>>.SuccessResponse(await _unitOfWork.RolePermissions.Dbset()
                .Where(rp => rp.RoleId == roleId)
                .Include(rp => rp.Permission)
                .Select(rp => new PermissionDTO
                {
                    Id = rp.Permission.Id,
                    Name = rp.Permission.Name,
                }).ToListAsync());
        }
    }

}
