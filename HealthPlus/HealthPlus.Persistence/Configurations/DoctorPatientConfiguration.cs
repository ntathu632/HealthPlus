using HealthPlus.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HealthPlus.Persistence.Configurations;

public class DoctorPatientConfiguration : IEntityTypeConfiguration<DoctorPatient>
{
    public void Configure(EntityTypeBuilder<DoctorPatient> builder)
    {
        builder.ToTable("DoctorPatients");
        builder.HasKey(dp => dp.Id);
        builder.HasIndex(dp => new { dp.DoctorId, dp.PatientId }).IsUnique();

        builder.HasOne(dp => dp.Doctor)
            .WithMany()
            .HasForeignKey(dp => dp.DoctorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(dp => dp.Patient)
            .WithMany()
            .HasForeignKey(dp => dp.PatientId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
