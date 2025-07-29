//using Microsoft.EntityFrameworkCore.Metadata.Builders;
//using Microsoft.EntityFrameworkCore;

//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Identity.Domain.Entities;

//namespace Identity.DAL.Config
//{
//    public class AppUserConfig : IEntityTypeConfiguration<AppUser>
//    {
//        public void Configure(EntityTypeBuilder<AppUser> builder)
//        {
//            builder.HasMany()
//            //builder.HasOne(rp => rp.Role)
//            //        .WithMany(r => r.RolePermissions)
//            //        .HasForeignKey(rp => rp.RoleId);
//            //builder.HasOne(rp => rp.Permission)
//            //       .WithMany()
//            //       .HasForeignKey(rp => rp.PermissionId);

//            //builder.HasKey(rp => new { rp.RoleId, rp.PermissionId });

//        }
//    }
//}
