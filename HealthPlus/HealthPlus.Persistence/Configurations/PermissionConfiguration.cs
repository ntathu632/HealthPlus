using HealthPlus.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HealthPlus.Persistence.Configurations;

public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.ToTable("Permissions");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).UseIdentityColumn();
        builder.Property(p => p.Name).IsRequired().HasMaxLength(200);
        builder.Property(p => p.Resource).IsRequired().HasMaxLength(100);
        builder.Property(p => p.Action).IsRequired().HasMaxLength(50);
        builder.Property(p => p.Description).HasMaxLength(500);
        builder.HasIndex(p => p.Name).IsUnique();
    }
}
