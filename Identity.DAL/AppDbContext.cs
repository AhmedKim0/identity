using Identity.Domain.Entities;
using Identity.Domain.SharedEntities;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

using System.Linq.Expressions;
using System.Security.Claims;

namespace Identity.DAL
{
    public class AppDbContext : IdentityDbContext<AppUser, AppRole, int>
    {

        private readonly IHttpContextAccessor _httpContextAccessor;

        public AppDbContext(DbContextOptions<AppDbContext> options,
                                    IHttpContextAccessor httpContextAccessor)
            : base(options)
        {
            _httpContextAccessor = httpContextAccessor;
        }
        public DbSet<AppUser> Users { get; set; }
        public DbSet<UserToken> UserTokens { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }

        public DbSet<OTPCode> oTPCodes { get; set; }
        public DbSet<OTPTry> oTPTries { get; set; }
        public DbSet<EmailBody> emailBodies { get; set; }

        private int? GetCurrentUserId()
        {
            var userId = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return string.IsNullOrEmpty(userId) ? null : int.Parse(userId);
        }
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;
            var currentUserId = GetCurrentUserId();

            foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedAtUtc = now;
                    entry.Entity.CreatedBy = currentUserId;

                }
                else if (entry.State == EntityState.Modified)
                {

                    entry.Entity.UpdatedAtUtc = now;
                    entry.Entity.UpdatedBy = currentUserId;
                }
                else if (entry.State == EntityState.Deleted)
                {
                    if (entry.Entity is not IHardDelete)
                    {
                        entry.Entity.IsDeleted = true;
                        entry.State = EntityState.Modified;
                    }
                }

            }
            return await base.SaveChangesAsync(cancellationToken);

        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Apply IsDeleted filter to all entities inheriting BaseEntity
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
                {
                    var parameter = Expression.Parameter(entityType.ClrType, "e");
                    var propertyMethod = typeof(EF).GetMethod("Property")!
                        .MakeGenericMethod(typeof(bool));

                    var isDeletedProperty = Expression.Call(propertyMethod, parameter, Expression.Constant("IsDeleted"));
                    var compareExpression = Expression.Equal(isDeletedProperty, Expression.Constant(false));

                    var lambda = Expression.Lambda(compareExpression, parameter);
                    modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
                }
            }
        }

    }
}
