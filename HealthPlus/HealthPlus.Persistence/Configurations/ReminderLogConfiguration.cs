using HealthPlus.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HealthPlus.Persistence.Configurations;

public class ReminderLogConfiguration : IEntityTypeConfiguration<ReminderLog>
{
    public void Configure(EntityTypeBuilder<ReminderLog> builder)
    {
        builder.ToTable("ReminderLogs");
        builder.HasKey(rl => rl.Id);
        builder.Property(rl => rl.Id).UseIdentityColumn();
        builder.Property(rl => rl.Channel).IsRequired().HasMaxLength(50);
        builder.Property(rl => rl.Status).IsRequired().HasMaxLength(50);
        builder.Property(rl => rl.ErrorMsg).HasMaxLength(1000);
        builder.Property(rl => rl.SentAt).HasDefaultValueSql("SYSUTCDATETIME()");

        builder.HasOne(rl => rl.Reminder)
            .WithMany(r => r.Logs)
            .HasForeignKey(rl => rl.ReminderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
