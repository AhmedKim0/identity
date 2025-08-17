using Identity.Application.UOW;
using Identity.DAL;
using Identity.Domain.Entities;
using Identity.Domain.IReposatory;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

using System.Data;

namespace Identity.Infrastructure.UOW
{
    public class UnitOfWork : IUnitOfWork, IAsyncDisposable
    {
        private readonly AppDbContext _dbContext;
        private IDbContextTransaction? _transaction;

        public IEmailBodyRepository EmailBodies { get; }
        public IEmailVerificationRepository EmailVerifications { get; }
        public IOTPCodeRepository OTPCodes { get; }
        public IOTPTryRepository OTPTrys { get; }
        public IPermissionRepository Permissions { get; }
        public IRolePermissionRepository RolePermissions { get; }
        public IUserTokenRepository UserTokens { get; }
        public UserManager<AppUser> _UserManager { get; }
        public RoleManager<AppRole> _RoleManager { get; }



        public UnitOfWork(
            AppDbContext context,
            IEmailBodyRepository emailBodies,
            IEmailVerificationRepository emailVerifications,
            IOTPCodeRepository otpCodes,
            IOTPTryRepository otpTrys,
            IPermissionRepository permissions,
            IRolePermissionRepository rolePermissions,
            IUserTokenRepository userTokens,
            UserManager<AppUser> userManager,
            RoleManager<AppRole> roleManager
        )
        {
            _dbContext = context;
            EmailBodies = emailBodies;
            EmailVerifications = emailVerifications;
            OTPCodes = otpCodes;
            OTPTrys = otpTrys;
            Permissions = permissions;
            RolePermissions = rolePermissions;
            UserTokens = userTokens;
            _UserManager = userManager;
            _RoleManager = roleManager;
        }

        // ---- Transaction handling ----
        public async Task BeginTransactionAsync(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            if (_transaction != null)
            {
                await _transaction.DisposeAsync();
            }

            _transaction = await _dbContext.Database.BeginTransactionAsync(isolationLevel);
        }

        public async Task CommitTransactionAsync()
        {
            if (_transaction == null)
                throw new InvalidOperationException("Transaction has not been started.");

            // This ensures both repository changes and UserManager changes are saved
            await _dbContext.SaveChangesAsync();

            await _transaction.CommitAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }

        public async Task RollbackTransactionAsync()
        {
            if (_transaction == null)
                throw new InvalidOperationException("Transaction has not been started.");

            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }

        // ---- Save Changes ----
        public async Task<int> SaveChangesAsync()
        {
            return await _dbContext.SaveChangesAsync();
        }

        // ---- Disposal ----
        public void Dispose()
        {
            _transaction?.Dispose();
        }

        public async ValueTask DisposeAsync()
        {
            if (_transaction != null)
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }
    }
}
