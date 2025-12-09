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
                    NameLogical = p.NameLogical,
                    NameAr = p.NameAr,
                    NameEn = p.NameEn,
                }).ToListAsync());
        }

        public async Task<Response<PermissionDTO?>> GetByIdAsync(int id)
        {
            var permission = await _unitOfWork.Permissions.FirstOrDefaultAsync(x=>x.Id==id);
            return Response<PermissionDTO?>.SuccessResponse(new PermissionDTO
            {
                Id = permission?.Id,
                NameLogical = permission?.NameLogical,
                NameAr = permission?.NameAr,
                NameEn = permission?.NameEn,

            });
        }

        public async Task<Response<PermissionDTO>> CreateAsync(CreatePermissionDTO dTO)
        {
            var permission = new Permission
            {
                NameLogical = dTO.NameLogical,
                NameAr = dTO.NameAr,
                NameEn = dTO.NameEn,
            };
            await _unitOfWork.Permissions.AddAsync(permission);
            await _unitOfWork.Permissions.SaveChangesAsync();
            var dto = new PermissionDTO
            {
                NameLogical = permission.NameLogical,
                NameAr = permission.NameAr,
                NameEn = permission.NameEn,
                Id = permission.Id
            };
            return Response<PermissionDTO>.SuccessResponse(dto);
        }

        public async Task<Response<PermissionDTO?>> UpdateAsync(PermissionDTO dto)
        {
            var permission = await _unitOfWork.Permissions.FirstOrDefaultAsync(x => x.Id == dto.Id);
            if (permission == null) return Response<PermissionDTO>.Failure(new Error("Permission not found"));

            permission.NameLogical = dto.NameLogical;
            permission.NameAr = dto.NameAr;
            permission.NameEn = dto.NameEn;

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
        // need to change here
        public async Task<Response<bool>> AssignPermissionsToRoleAsync(int roleId, List<int> NewPermissionIds)
        {
            await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted);
            try
            {
                NewPermissionIds= NewPermissionIds.Distinct().ToList();
                var role = await _unitOfWork._RoleManager.FindByIdAsync(roleId.ToString());
                if (role == null)
                { return Response<bool>.Failure(new Error("Role not found")); }
                var existingPermissionIds = await _unitOfWork.RolePermissions
                    .Dbset().Where(rp=>rp.RoleId==roleId)
                    .Select(p => p.PermissionId)
                    .ToListAsync();

                var countexist = await _unitOfWork.Permissions
                    .Dbset()
                    .CountAsync(x => NewPermissionIds.Contains(x.Id));
                if (!(countexist == NewPermissionIds.Count()))
                { return Response<bool>.Failure(new Error("one permission or all not exist")); }
               var rolesToAdd = NewPermissionIds.Except(existingPermissionIds).ToList();
                var rolesToRemove = existingPermissionIds.Except(NewPermissionIds).ToList();
                await _unitOfWork.RolePermissions.DeleteRangeAsync(rolesToRemove);
                
                var newAssignments = rolesToAdd.Select(pid => new RolePermission
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
                    NameLogical = rp.Permission.NameLogical,
                    NameAr = rp.Permission.NameAr,
                    NameEn = rp.Permission.NameEn,

                }).ToListAsync());
        }
    }

}
