using HealthPlus.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HealthPlus.Persistence.Configurations;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshTokens");
        builder.HasKey(rt => rt.Id);
        builder.Property(rt => rt.Id).HasDefaultValueSql("NEWSEQUENTIALID()");
        builder.Property(rt => rt.Token).IsRequired().HasMaxLength(512);
        builder.Property(rt => rt.DeviceInfo).HasMaxLength(500);
        builder.Property(rt => rt.IpAddress).HasMaxLength(45);
        builder.Property(rt => rt.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
        builder.HasIndex(rt => rt.Token).IsUnique();
        builder.HasIndex(rt => rt.UserId);
        builder.HasIndex(rt => rt.ExpiresAt);

        builder.HasOne(rt => rt.User)
            .WithMany(u => u.RefreshTokens)
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
