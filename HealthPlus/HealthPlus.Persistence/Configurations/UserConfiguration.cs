using HealthPlus.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HealthPlus.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id).HasDefaultValueSql("NEWSEQUENTIALID()");
        builder.Property(u => u.Email).IsRequired().HasMaxLength(256);
        builder.Property(u => u.PasswordHash).IsRequired().HasMaxLength(512);
        builder.Property(u => u.FullName).IsRequired().HasMaxLength(256);
        builder.Property(u => u.PhoneNumber).HasMaxLength(20);
        builder.Property(u => u.AvatarUrl).HasMaxLength(1000);
        builder.Property(u => u.Specialty).HasMaxLength(200);
        builder.Property(u => u.ConsultationFee).HasColumnType("decimal(12,2)");
        builder.Property(u => u.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
        builder.Property(u => u.UpdatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
        builder.HasIndex(u => u.Email).IsUnique();
        builder.HasIndex(u => u.IsActive);

        builder.HasOne(u => u.Hospital)
            .WithMany()
            .HasForeignKey(u => u.HospitalId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
