namespace HealthPlus.Domain.Entities;

public class UserNotificationSetting
{
    public Guid UserId { get; set; }
    public bool PushEnabled { get; set; } = true;
    public bool EmailEnabled { get; set; } = true;
    public bool SmsEnabled { get; set; } = false;
    public bool VaccineReminderEnabled { get; set; } = true;
    public bool MedicineReminderEnabled { get; set; } = true;
    public bool FollowUpReminderEnabled { get; set; } = true;
    public TimeOnly? QuietHourStart { get; set; }
    public TimeOnly? QuietHourEnd { get; set; }
    public string? FcmToken { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
}
