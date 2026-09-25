using HealthPlus.Domain.Entities;
using HealthPlus.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HealthPlus.Persistence.Configurations;

public class HealthRecordConfiguration : IEntityTypeConfiguration<HealthRecord>
{
    public void Configure(EntityTypeBuilder<HealthRecord> builder)
    {
        builder.ToTable("HealthRecords");
        builder.HasKey(hr => hr.Id);
        builder.Property(hr => hr.Id).HasDefaultValueSql("NEWSEQUENTIALID()");
        builder.Property(hr => hr.ProfileName).IsRequired().HasMaxLength(256);
        builder.Property(hr => hr.FullName).IsRequired().HasMaxLength(256);
        builder.Property(hr => hr.Gender).HasConversion<string>().HasMaxLength(20);
        builder.Property(hr => hr.BloodType).HasConversion<string>().HasMaxLength(10);
        builder.Property(hr => hr.Allergies).HasMaxLength(1000);
        builder.Property(hr => hr.ChronicDiseases).HasMaxLength(1000);
        builder.Property(hr => hr.InsuranceNumber).HasMaxLength(100);
        builder.Property(hr => hr.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
        builder.Property(hr => hr.UpdatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
        builder.HasIndex(hr => hr.UserId);

        builder.HasOne(hr => hr.User)
            .WithMany(u => u.HealthRecords)
            .HasForeignKey(hr => hr.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
