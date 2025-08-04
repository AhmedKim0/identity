using Identity.Domain.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class EmailVerificationConfig : IEntityTypeConfiguration<EmailVerification>
{
    public void Configure(EntityTypeBuilder<EmailVerification> builder)
    {
        builder.ToTable("EmailVerification");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Email)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(e => e.IsVerified)
            .IsRequired();

        builder.Property(e => e.BlockedUntil)
            .IsRequired(false); 

        builder.HasMany(e => e.OTPCodes)
            .WithOne(o => o.EmailVerification)
            .HasForeignKey(o => o.EmailVerificationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
