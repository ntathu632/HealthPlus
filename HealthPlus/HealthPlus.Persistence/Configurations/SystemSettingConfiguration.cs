using HealthPlus.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HealthPlus.Persistence.Configurations;

public class SystemSettingConfiguration : IEntityTypeConfiguration<SystemSetting>
{
    public void Configure(EntityTypeBuilder<SystemSetting> builder)
    {
        builder.ToTable("SystemSettings");
        builder.HasKey(ss => ss.Id);
        builder.Property(ss => ss.Id).UseIdentityColumn();
        builder.Property(ss => ss.SettingKey).IsRequired().HasMaxLength(200);
        builder.Property(ss => ss.Description).HasMaxLength(500);
        builder.Property(ss => ss.UpdatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
        builder.HasIndex(ss => ss.SettingKey).IsUnique();
    }
}
