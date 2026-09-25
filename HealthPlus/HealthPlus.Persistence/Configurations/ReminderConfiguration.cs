using HealthPlus.Domain.Entities;
using HealthPlus.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HealthPlus.Persistence.Configurations;

public class ReminderConfiguration : IEntityTypeConfiguration<Reminder>
{
    public void Configure(EntityTypeBuilder<Reminder> builder)
    {
        builder.ToTable("Reminders");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).HasDefaultValueSql("NEWSEQUENTIALID()");
        builder.Property(r => r.Title).IsRequired().HasMaxLength(500);
        builder.Property(r => r.Message).HasMaxLength(1000);
        builder.Property(r => r.RelatedEntityType).HasMaxLength(100);
        builder.Property(r => r.ReminderType).HasConversion<string>().HasMaxLength(50);
        builder.Property(r => r.Channel).HasConversion<string>().HasMaxLength(50).HasDefaultValue(ReminderChannel.Push);
        builder.Property(r => r.RepeatType).HasConversion<string>().HasMaxLength(50);
        builder.Property(r => r.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
        builder.HasIndex(r => r.UserId);
        builder.HasIndex(r => r.RemindAt);
        builder.HasIndex(r => r.IsSent);
        builder.HasIndex(r => r.ReminderType);

        builder.HasOne(r => r.User)
            .WithMany()
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
