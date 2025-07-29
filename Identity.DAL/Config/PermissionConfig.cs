using Identity.Domain.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Identity.DAL.Config
{
    public class PermissionConfig : IEntityTypeConfiguration<Permission>
    {
        public void Configure(EntityTypeBuilder<Permission> builder)
        {
            builder.HasKey(x => x.Id);
            builder.HasMany(p => p.RolePermissions)
       .WithOne(rp => rp.Permission)
       .HasForeignKey(rp => rp.PermissionId)
       .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
