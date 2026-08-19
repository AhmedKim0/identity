using Identity.Domain.SharedEntities;

using Microsoft.AspNetCore.Identity;

namespace Identity.Domain.Entities;

public class AppUser : IdentityUser<int>, IAuditableEntity, IBaseEntity
{
    private AppUser()
    {
        // Required by EF Core
    }

    public AppUser(
        string userName,
        string email)
    {
        UserName = userName;
        NormalizedUserName = userName.ToUpperInvariant();

        Email = email;
        NormalizedEmail = email.ToUpperInvariant();

        IsDeleted = false;
    }
    public AppUser(
    string userName,
    string email,
    string phone)
    {
        UserName = userName;
        NormalizedUserName = userName.ToUpperInvariant();

        Email = email;
        NormalizedEmail = email.ToUpperInvariant();
        phone=phone;
        IsDeleted = false;
    }


    // Refresh Token

    public string? RefreshToken { get; private set; }

    public DateTime? RefreshTokenExpiryTime { get; private set; }

    public void SetRefreshToken(
        string refreshToken,
        DateTime expiryTime)
    {
        if(string.IsNullOrWhiteSpace(refreshToken))
            throw new ArgumentException(
                "Refresh token is required.",
                nameof(refreshToken));

        RefreshToken = refreshToken;
        RefreshTokenExpiryTime = expiryTime;
    }

    public void ClearRefreshToken()
    {
        RefreshToken = null;
        RefreshTokenExpiryTime = null;
    }

    public bool HasValidRefreshToken()
    {
        return !string.IsNullOrWhiteSpace(RefreshToken)
               && RefreshTokenExpiryTime.HasValue
               && RefreshTokenExpiryTime.Value > DateTime.UtcNow;
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

    // Soft Delete

    public bool IsDeleted { get;  set; }

    public void SoftDelete()
    {
        IsDeleted = true;
    }

    public void Restore()
    {
        IsDeleted = false;
    }



    public DateTime CreatedAtUtc { get;  set; }

    public int CreatedBy { get;  set; }

    public DateTime? UpdatedAtUtc { get;  set; }

    public int? UpdatedBy { get;  set; }
}