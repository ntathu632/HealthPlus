using HealthPlus.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HealthPlus.Persistence.Configurations;

public class PrescriptionItemConfiguration : IEntityTypeConfiguration<PrescriptionItem>
{
    public void Configure(EntityTypeBuilder<PrescriptionItem> builder)
    {
        builder.ToTable("PrescriptionItems");
        builder.HasKey(pi => pi.Id);
        builder.Property(pi => pi.Id).HasDefaultValueSql("NEWSEQUENTIALID()");
        builder.Property(pi => pi.MedicineName).IsRequired().HasMaxLength(500);
        builder.Property(pi => pi.Dosage).HasMaxLength(200);
        builder.Property(pi => pi.Timing).HasMaxLength(200);
        builder.Property(pi => pi.Instructions).HasMaxLength(1000);

        builder.HasOne(pi => pi.Prescription)
            .WithMany(p => p.Items)
            .HasForeignKey(pi => pi.PrescriptionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
