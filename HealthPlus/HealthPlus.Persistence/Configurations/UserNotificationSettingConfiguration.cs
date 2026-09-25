using HealthPlus.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HealthPlus.Persistence.Configurations;

public class UserNotificationSettingConfiguration : IEntityTypeConfiguration<UserNotificationSetting>
{
    public void Configure(EntityTypeBuilder<UserNotificationSetting> builder)
    {
        builder.ToTable("UserNotificationSettings");
        builder.HasKey(uns => uns.UserId);
        builder.Property(uns => uns.FcmToken).HasMaxLength(1000);
        builder.Property(uns => uns.UpdatedAt).HasDefaultValueSql("SYSUTCDATETIME()");

        builder.HasOne(uns => uns.User)
            .WithOne(u => u.NotificationSetting)
            .HasForeignKey<UserNotificationSetting>(uns => uns.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
