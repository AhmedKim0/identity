using Identity.Application.DTO;
using Identity.Application.DTO.PermissionDTOs;
using Identity.Application.Int;
using Identity.Application.Reposatory;
using Identity.Application.UOW;
using Identity.Domain.Entities;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Identity.Application.Repos
{
    public class PermissionService : IPermissionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAsyncRepository<Permission> _permissionRepo;
        private readonly RoleManager<AppRole> _roleManager;
        private readonly IAsyncRepository<RolePermission> _rolePermissionRepo;
        private readonly IMemoryCache _cache;
        private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(10);

        public PermissionService(IUnitOfWork unitOfWork, IAsyncRepository<Permission> permissionRepo, RoleManager<AppRole> roleManager, IAsyncRepository<RolePermission> rolePermissionRepo, IMemoryCache cache)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _permissionRepo = permissionRepo ?? throw new ArgumentNullException(nameof(permissionRepo));
            _roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
            _rolePermissionRepo = rolePermissionRepo ?? throw new ArgumentNullException(nameof(rolePermissionRepo));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        public async Task<Response<List<PermissionDTO>>> GetAllAsync()
        {
            return Response<List<PermissionDTO>>.SuccessResponse(await _permissionRepo.Dbset()
                .Select(p => new PermissionDTO
                {
                    Id = p.Id,
                    Name = p.Name,
                }).ToListAsync());
        }

        public async Task<Response<PermissionDTO?>> GetByIdAsync(int id)
        {
            var permission = await _permissionRepo.FirstOrDefaultAsync(x=>x.Id==id);
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
            await _permissionRepo.AddAsync(permission);
            await _permissionRepo.SaveChangesAsync();
            var dto = new PermissionDTO
            {
                Name = permission.Name,
            };
            dto.Id = permission.Id;
            return Response<PermissionDTO>.SuccessResponse(dto);
        }

        public async Task<Response<PermissionDTO?>> UpdateAsync(PermissionDTO dto)
        {
            var permission = await _permissionRepo.FirstOrDefaultAsync(x => x.Id == dto.Id);
            if (permission == null) return Response<PermissionDTO>.Failure(new Error("Permission not found"));

            permission.Name = dto.Name;
            await _permissionRepo.UpdateAsync(permission);
            await _permissionRepo.SaveChangesAsync();

            if (permission == null) return Response<PermissionDTO>.Failure(new Error("Permission not found"));
            return Response<PermissionDTO>.SuccessResponse( dto);
        }

        public async Task<Response<bool>> DeleteAsync(int id)
        {
            var permission = await _permissionRepo.FirstOrDefaultAsync(x => x.Id == id);
            if (permission == null) return Response<bool>.Failure(new Error("Permission not found"));

           await  _permissionRepo.DeleteAsync(permission);
            await _permissionRepo.SaveChangesAsync();
            return Response<bool>.SuccessResponse(true);
        }
        public async Task<Response<bool>> AssignPermissionsToRoleAsync(int roleId, List<int> permissionIds)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var role = await _roleManager.FindByIdAsync(roleId.ToString());
                if (role == null)
                { return Response<bool>.Failure(new Error("Role not found")); }
                var existingPermissionIds = await _permissionRepo
                    .Dbset()
                    .Where(p => permissionIds.Contains(p.Id))
                    .Select(p => p.Id)
                    .ToListAsync();

                var isAllPermissionExist = permissionIds.All(id => existingPermissionIds.Contains(id));
                if (!isAllPermissionExist)
                { return Response<bool>.Failure(new Error("one permission or all not exist")); }

                var existing = await _rolePermissionRepo.Dbset().AsNoTracking().Where(rp => rp.RoleId == roleId).FirstOrDefaultAsync();
                if (existing != null) 
                await _rolePermissionRepo.DeleteAsync(existing);

                var newAssignments = permissionIds.Select(pid => new RolePermission
                {
                    RoleId = roleId,
                    PermissionId = pid
                });

                _rolePermissionRepo.Dbset().AddRange(newAssignments);
                var data = await _permissionRepo.Dbset()
                    .Where(p => permissionIds.Contains(p.Id)).Select(p=>p.Name).ToListAsync();

                _cache.Remove($"{role.Name}");
                _cache.CreateEntry($"{role.Name}").AbsoluteExpirationRelativeToNow = _cacheDuration;
                _cache.Set($"{role}", data);

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
            return Response<List<PermissionDTO>>.SuccessResponse(await _rolePermissionRepo.Dbset()
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
