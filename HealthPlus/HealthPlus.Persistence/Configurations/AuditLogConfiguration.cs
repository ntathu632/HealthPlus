using HealthPlus.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HealthPlus.Persistence.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs");
        builder.HasKey(al => al.Id);
        builder.Property(al => al.Id).UseIdentityColumn();
        builder.Property(al => al.Action).IsRequired().HasMaxLength(100);
        builder.Property(al => al.Entity).HasMaxLength(100);
        builder.Property(al => al.EntityId).HasMaxLength(100);
        builder.Property(al => al.IpAddress).HasMaxLength(45);
        builder.Property(al => al.UserAgent).HasMaxLength(500);
        builder.Property(al => al.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
        builder.HasIndex(al => al.UserId);
        builder.HasIndex(al => al.CreatedAt).IsDescending();

        builder.HasOne(al => al.User)
            .WithMany()
            .HasForeignKey(al => al.UserId)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);
    }
}
