using Identity.Domain.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class OTPCodeConfig : IEntityTypeConfiguration<OTPCode>
{
    public void Configure(EntityTypeBuilder<OTPCode> builder)
    {
        builder.ToTable("OTPCode");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.Code)
            .IsRequired()
            .HasMaxLength(6); 

        builder.Property(o => o.IsExpired)
            .IsRequired();

        builder.Property(o => o.ExpireAt)
            .IsRequired();

        builder.HasMany(o => o.OTPTries)
            .WithOne(t => t.OTPCode)
            .HasForeignKey(t => t.OTPCodeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
