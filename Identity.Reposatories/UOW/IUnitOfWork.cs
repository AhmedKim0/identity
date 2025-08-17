using Identity.Domain.Entities;
using Identity.Domain.IReposatory;

using Microsoft.AspNetCore.Identity;

using System.Data;

namespace Identity.Application.UOW
{

    public interface IUnitOfWork : IAsyncDisposable, IDisposable
    {
        Task BeginTransactionAsync(IsolationLevel isolationLevel);

        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
        Task<int> SaveChangesAsync();



        IEmailBodyRepository EmailBodies { get; }
        IEmailVerificationRepository EmailVerifications { get; }
        IOTPCodeRepository OTPCodes { get; }
        IOTPTryRepository OTPTrys { get; }
        IPermissionRepository Permissions { get; }
        IRolePermissionRepository RolePermissions { get; }
        IUserTokenRepository UserTokens { get; }
        UserManager<AppUser> _UserManager { get; }
        RoleManager<AppRole> _RoleManager { get; }
    }


}
