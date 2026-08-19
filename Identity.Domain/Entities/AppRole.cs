using Identity.Domain.SharedEntities;

using Microsoft.AspNetCore.Identity;

namespace Identity.Domain.Entities;

public class AppRole : IdentityRole<int>,  IAuditableEntity, IBaseEntity
{
    private readonly List<RolePermission> _rolePermissions = new();

    private AppRole()
    {
        // Required by EF Core
    }

    public AppRole(string LogicalName, string nameEn, string nameAr)
    {
        SetNames(LogicalName,nameEn, nameAr);

        IsDeleted = false;
    }

    public string NameAr { get; private set; } = null!;

    public string NameEn { get; private set; } = null!;

    public DateTime CreatedAtUtc { get;  set; }

    public int CreatedBy { get;  set; }

    public DateTime? UpdatedAtUtc { get;  set; }

    public int? UpdatedBy { get;  set; }

    public bool IsDeleted { get;  set; }

    public IReadOnlyCollection<RolePermission> RolePermissions =>
        _rolePermissions.AsReadOnly();

    public void UpdateNames(string LogicalName, string nameEn, string nameAr)
    {
        SetNames(LogicalName,nameEn, nameAr);
    }
    public void SetCreatedAudit(
    int userId,
    DateTime createdAtUtc)
    {
        CreatedBy = userId;
        CreatedAtUtc = createdAtUtc;
    }

    public void SetUpdatedAudit(
        int userId,
        DateTime updatedAtUtc)
    {
        UpdatedBy = userId;
        UpdatedAtUtc = updatedAtUtc;
    }

    public void AddPermission(RolePermission rolePermission)
    {
        ArgumentNullException.ThrowIfNull(rolePermission);

        if(_rolePermissions.Any(x =>
                x.PermissionId == rolePermission.PermissionId))
        {
            return;
        }

        _rolePermissions.Add(rolePermission);
    }

    public void RemovePermission(int permissionId)
    {
        var rolePermission = _rolePermissions
            .FirstOrDefault(x => x.PermissionId == permissionId);

        if(rolePermission is not null)
        {
            _rolePermissions.Remove(rolePermission);
        }
    }

    public void SoftDelete()
    {
        IsDeleted = true;
    }

    public void Restore()
    {
        IsDeleted = false;
    }

    private void SetNames(string LogicalName,string nameEn, string nameAr)
    {
        if(string.IsNullOrWhiteSpace(nameEn))
            throw new ArgumentException(
                "English role name is required.",
                nameof(nameEn));

        if(string.IsNullOrWhiteSpace(nameAr))
            throw new ArgumentException(
                "Arabic role name is required.",
                nameof(nameAr));

        NameEn = nameEn.Trim();
        NameAr = nameAr.Trim();

        // ASP.NET Identity
        Name = LogicalName;
        NormalizedName = LogicalName.ToUpperInvariant();
    }
}