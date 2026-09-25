using HealthPlus.Domain.Entities;
using HealthPlus.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HealthPlus.Persistence.Configurations;

public class PrescriptionConfiguration : IEntityTypeConfiguration<Prescription>
{
    public void Configure(EntityTypeBuilder<Prescription> builder)
    {
        builder.ToTable("Prescriptions");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasDefaultValueSql("NEWSEQUENTIALID()");
        builder.Property(p => p.ImageStorageKey).HasMaxLength(1000);
        builder.Property(p => p.ImageUrl).HasMaxLength(2000);
        builder.Property(p => p.OcrConfidenceScore).HasColumnType("decimal(5,4)");
        builder.Property(p => p.Status).HasConversion<string>().HasMaxLength(50).HasDefaultValue(PrescriptionStatus.Pending);
        builder.Property(p => p.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
        builder.HasIndex(p => p.UserId);
        builder.HasIndex(p => p.Status);

        builder.HasOne(p => p.User)
            .WithMany()
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(p => p.MedicalHistory)
            .WithMany(mh => mh.Prescriptions)
            .HasForeignKey(p => p.MedicalHistoryId)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);
    }
}
