using HealthPlus.Domain.Entities;
using HealthPlus.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HealthPlus.Persistence.Configurations;

public class VaccineConfiguration : IEntityTypeConfiguration<Vaccine>
{
    public void Configure(EntityTypeBuilder<Vaccine> builder)
    {
        builder.ToTable("Vaccines");
        builder.HasKey(v => v.Id);
        builder.Property(v => v.Id).HasDefaultValueSql("NEWSEQUENTIALID()");
        builder.Property(v => v.VaccineName).IsRequired().HasMaxLength(500);
        builder.Property(v => v.Manufacturer).HasMaxLength(500);
        builder.Property(v => v.LotNumber).HasMaxLength(200);
        builder.Property(v => v.Location).HasMaxLength(500);
        builder.Property(v => v.AdministeredBy).HasMaxLength(256);
        builder.Property(v => v.SideEffects).HasMaxLength(1000);
        builder.Property(v => v.Notes).HasMaxLength(1000);
        builder.Property(v => v.Status).HasConversion<string>().HasMaxLength(50).HasDefaultValue(VaccineStatus.Completed);
        builder.Property(v => v.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
        builder.Property(v => v.UpdatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
        builder.HasIndex(v => v.UserId);
        builder.HasIndex(v => v.HealthRecordId);
        builder.HasIndex(v => v.Status);

        builder.HasOne(v => v.User)
            .WithMany()
            .HasForeignKey(v => v.UserId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(v => v.HealthRecord)
            .WithMany(hr => hr.Vaccines)
            .HasForeignKey(v => v.HealthRecordId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
