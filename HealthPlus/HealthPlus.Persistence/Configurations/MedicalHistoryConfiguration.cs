using HealthPlus.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HealthPlus.Persistence.Configurations;

public class MedicalHistoryConfiguration : IEntityTypeConfiguration<MedicalHistory>
{
    public void Configure(EntityTypeBuilder<MedicalHistory> builder)
    {
        builder.ToTable("MedicalHistory");
        builder.HasKey(mh => mh.Id);
        builder.Property(mh => mh.Id).HasDefaultValueSql("NEWSEQUENTIALID()");
        builder.Property(mh => mh.DoctorName).HasMaxLength(256);
        builder.Property(mh => mh.Hospital).HasMaxLength(500);
        builder.Property(mh => mh.Specialty).HasMaxLength(200);
        builder.Property(mh => mh.Diagnosis).HasMaxLength(2000);
        builder.Property(mh => mh.Treatment).HasMaxLength(2000);
        builder.Property(mh => mh.Notes).HasMaxLength(2000);
        builder.Property(mh => mh.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
        builder.Property(mh => mh.UpdatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
        builder.HasIndex(mh => mh.UserId);
        builder.HasIndex(mh => mh.HealthRecordId);
        builder.HasIndex(mh => mh.VisitDate).IsDescending();

        builder.HasOne(mh => mh.User)
            .WithMany()
            .HasForeignKey(mh => mh.UserId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(mh => mh.HealthRecord)
            .WithMany(hr => hr.MedicalHistories)
            .HasForeignKey(mh => mh.HealthRecordId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
