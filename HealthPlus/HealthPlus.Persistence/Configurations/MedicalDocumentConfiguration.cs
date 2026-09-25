using HealthPlus.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HealthPlus.Persistence.Configurations;

public class MedicalDocumentConfiguration : IEntityTypeConfiguration<MedicalDocument>
{
    public void Configure(EntityTypeBuilder<MedicalDocument> builder)
    {
        builder.ToTable("MedicalDocuments");
        builder.HasKey(md => md.Id);
        builder.Property(md => md.Id).HasDefaultValueSql("NEWSEQUENTIALID()");
        builder.Property(md => md.FileName).IsRequired().HasMaxLength(500);
        builder.Property(md => md.StorageKey).IsRequired().HasMaxLength(1000);
        builder.Property(md => md.FileUrl).HasMaxLength(2000);
        builder.Property(md => md.FileType).IsRequired().HasMaxLength(100);
        builder.Property(md => md.DocumentType).HasMaxLength(100);
        builder.Property(md => md.UploadedAt).HasDefaultValueSql("SYSUTCDATETIME()");

        builder.HasOne(md => md.MedicalHistory)
            .WithMany(mh => mh.Documents)
            .HasForeignKey(md => md.MedicalHistoryId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
