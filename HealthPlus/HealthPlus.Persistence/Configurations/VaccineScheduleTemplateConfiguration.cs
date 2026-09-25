using HealthPlus.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HealthPlus.Persistence.Configurations;

public class VaccineScheduleTemplateConfiguration : IEntityTypeConfiguration<VaccineScheduleTemplate>
{
    public void Configure(EntityTypeBuilder<VaccineScheduleTemplate> builder)
    {
        builder.ToTable("VaccineScheduleTemplates");
        builder.HasKey(vst => vst.Id);
        builder.Property(vst => vst.Id).UseIdentityColumn();
        builder.Property(vst => vst.VaccineName).IsRequired().HasMaxLength(500);
        builder.Property(vst => vst.RecommendedAge).HasMaxLength(100);
        builder.Property(vst => vst.Notes).HasMaxLength(500);
    }
}
