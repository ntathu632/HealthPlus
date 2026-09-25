using HealthPlus.Domain.Enums;

namespace HealthPlus.Application.DTOs.Reminders;

public class ReminderResponse
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public ReminderType ReminderType { get; set; }
    public Guid? RelatedEntityId { get; set; }
    public string? RelatedEntityType { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Message { get; set; }
    public DateTime RemindAt { get; set; }
    public bool IsEnabled { get; set; }
    public bool IsSent { get; set; }
    public DateTime? SentAt { get; set; }
    public ReminderChannel Channel { get; set; }
    public RepeatType? RepeatType { get; set; }
    public DateOnly? RepeatUntil { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class NotificationSettingResponse
{
    public Guid UserId { get; set; }
    public bool PushEnabled { get; set; }
    public bool EmailEnabled { get; set; }
    public bool SmsEnabled { get; set; }
    public bool VaccineReminderEnabled { get; set; }
    public bool MedicineReminderEnabled { get; set; }
    public bool FollowUpReminderEnabled { get; set; }
    public TimeOnly? QuietHourStart { get; set; }
    public TimeOnly? QuietHourEnd { get; set; }
    public string? FcmToken { get; set; }
    public DateTime UpdatedAt { get; set; }
}
