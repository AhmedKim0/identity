using Identity.Domain.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class OTPTryConfig : IEntityTypeConfiguration<OTPTry>
{
    public void Configure(EntityTypeBuilder<OTPTry> builder)
    {
        builder.ToTable("OTPTry");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.IsSuccess)
            .IsRequired();

        builder.HasOne(t => t.OTPCode)
            .WithMany(c => c.OTPTries)
            .HasForeignKey(t => t.OTPCodeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
