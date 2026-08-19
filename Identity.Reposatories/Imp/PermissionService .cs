using Identity.Application.DTO;
using Identity.Application.DTO.PermissionDTOs;
using Identity.Application.Int;
using Identity.Application.UOW;
using Identity.Domain.Entities;

using Microsoft.EntityFrameworkCore;

using System.Data;

namespace Identity.Application.Imp;

public class PermissionService : IPermissionService
{
    private readonly IUnitOfWork _unitOfWork;

    public PermissionService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork
            ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<Response<List<PermissionDTO>>> GetAllAsync()
    {
        var permissions = await _unitOfWork.Permissions
            .Dbset()
            .Select(p => new PermissionDTO
            {
                Id = p.Id,
                NameLogical = p.NameLogical,
                NameAr = p.NameAr,
                NameEn = p.NameEn
            })
            .ToListAsync();

        return Response<List<PermissionDTO>>
            .SuccessResponse(permissions);
    }

    public async Task<Response<PermissionDTO?>> GetByIdAsync(int id)
    {
        var permission = await _unitOfWork.Permissions
            .FirstOrDefaultAsync(x => x.Id == id);

        if(permission == null)
        {
            return Response<PermissionDTO?>.Failure(
                new Error("Permission not found"));
        }

        var dto = new PermissionDTO
        {
            Id = permission.Id,
            NameLogical = permission.NameLogical,
            NameAr = permission.NameAr,
            NameEn = permission.NameEn
        };

        return Response<PermissionDTO?>
            .SuccessResponse(dto);
    }

    public async Task<Response<PermissionDTO>> CreateAsync(
        CreatePermissionDTO dto)
    {
        var permission = new Permission(
            dto.NameLogical,
            dto.NameAr,
            dto.NameEn);

        await _unitOfWork.Permissions.AddAsync(permission);

        await _unitOfWork.Permissions.SaveChangesAsync();

        var result = new PermissionDTO
        {
            Id = permission.Id,
            NameLogical = permission.NameLogical,
            NameAr = permission.NameAr,
            NameEn = permission.NameEn
        };

        return Response<PermissionDTO>
            .SuccessResponse(result);
    }

    public async Task<Response<PermissionDTO?>> UpdateAsync(
        PermissionDTO dto)
    {
        var permission = await _unitOfWork.Permissions
            .FirstOrDefaultAsync(x => x.Id == dto.Id);

        if(permission == null)
        {
            return Response<PermissionDTO?>.Failure(
                new Error("Permission not found"));
        }

        permission.Update(
            dto.NameLogical,
            dto.NameAr,
            dto.NameEn);

        await _unitOfWork.Permissions.UpdateAsync(permission);

        await _unitOfWork.Permissions.SaveChangesAsync();

        var result = new PermissionDTO
        {
            Id = permission.Id,
            NameLogical = permission.NameLogical,
            NameAr = permission.NameAr,
            NameEn = permission.NameEn
        };

        return Response<PermissionDTO?>
            .SuccessResponse(result);
    }

    public async Task<Response<bool>> DeleteAsync(int id)
    {
        var permission = await _unitOfWork.Permissions
            .FirstOrDefaultAsync(x => x.Id == id);

        if(permission == null)
        {
            return Response<bool>.Failure(
                new Error("Permission not found"));
        }

        permission.SoftDelete();

        await _unitOfWork.Permissions.SaveChangesAsync();

        return Response<bool>.SuccessResponse(true);
    }

    public async Task<Response<bool>> AssignPermissionsToRoleAsync(
        int roleId,
        List<int> newPermissionIds)
    {
        await _unitOfWork.BeginTransactionAsync(
            IsolationLevel.ReadCommitted);

        try
        {
            var permissionIds = newPermissionIds
                .Distinct()
                .ToList();

            var role = await _unitOfWork._RoleManager
                .FindByIdAsync(roleId.ToString());

            if(role == null)
            {
                await _unitOfWork.RollbackTransactionAsync();

                return Response<bool>.Failure(
                    new Error("Role not found"));
            }

            var existingPermissionIds = await _unitOfWork.RolePermissions
                .Dbset()
                .Where(rp => rp.RoleId == roleId)
                .Select(rp => rp.PermissionId)
                .ToListAsync();

            var existingPermissionIdSet =
                existingPermissionIds.ToHashSet();

            var validPermissionCount = await _unitOfWork.Permissions
                .Dbset()
                .CountAsync(p => permissionIds.Contains(p.Id));

            if(validPermissionCount != permissionIds.Count)
            {
                await _unitOfWork.RollbackTransactionAsync();

                return Response<bool>.Failure(
                    new Error("One or more permissions do not exist"));
            }

            var permissionsToAdd = permissionIds
                .Except(existingPermissionIdSet)
                .ToList();

            var permissionsToRemove = existingPermissionIdSet
                .Except(permissionIds)
                .ToList();

            if(permissionsToRemove.Count > 0)
            {
                await _unitOfWork.RolePermissions
                    .DeleteRangeAsync(permissionsToRemove);
            }

            var newAssignments = permissionsToAdd
                .Select(permissionId =>
                    new RolePermission(roleId, permissionId))
                .ToList();

            if(newAssignments.Count > 0)
            {
                _unitOfWork.RolePermissions
                    .Dbset()
                    .AddRange(newAssignments);
            }

            await _unitOfWork.CommitTransactionAsync();

            return Response<bool>.SuccessResponse(true);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    public async Task<Response<List<PermissionDTO>>> GetPermissionsByRoleAsync(
        int roleId)
    {
        var permissions = await _unitOfWork.RolePermissions
            .Dbset()
            .Where(rp => rp.RoleId == roleId)
            .Select(rp => new PermissionDTO
            {
                Id = rp.Permission.Id,
                NameLogical = rp.Permission.NameLogical,
                NameAr = rp.Permission.NameAr,
                NameEn = rp.Permission.NameEn
            })
            .ToListAsync();

        return Response<List<PermissionDTO>>
            .SuccessResponse(permissions);
    }
}