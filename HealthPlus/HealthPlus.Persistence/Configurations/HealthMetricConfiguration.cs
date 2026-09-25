using HealthPlus.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HealthPlus.Persistence.Configurations;

public class HealthMetricConfiguration : IEntityTypeConfiguration<HealthMetric>
{
    public void Configure(EntityTypeBuilder<HealthMetric> builder)
    {
        builder.ToTable("HealthMetrics");
        builder.HasKey(hm => hm.Id);
        builder.Property(hm => hm.Id).HasDefaultValueSql("NEWSEQUENTIALID()");
        builder.Property(hm => hm.MeasuredAt).HasDefaultValueSql("SYSUTCDATETIME()");
        builder.Property(hm => hm.HeightCm).HasColumnType("decimal(5,2)");
        builder.Property(hm => hm.WeightKg).HasColumnType("decimal(5,2)");
        builder.Property(hm => hm.Bmi).HasColumnType("decimal(5,2)");
        builder.Property(hm => hm.BloodSugar).HasColumnType("decimal(5,2)");
        builder.Property(hm => hm.Temperature).HasColumnType("decimal(4,1)");
        builder.Property(hm => hm.Notes).HasMaxLength(500);
        builder.Property(hm => hm.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
        builder.HasIndex(hm => hm.HealthRecordId);
        builder.HasIndex(hm => hm.MeasuredAt).IsDescending();

        builder.HasOne(hm => hm.HealthRecord)
            .WithMany(hr => hr.HealthMetrics)
            .HasForeignKey(hm => hm.HealthRecordId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
