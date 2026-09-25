using HealthPlus.Domain.Common;
using HealthPlus.Domain.Enums;

namespace HealthPlus.Domain.Entities;

public class Reminder : BaseEntity
{
    public Guid UserId { get; set; }
    public ReminderType ReminderType { get; set; }
    public Guid? RelatedEntityId { get; set; }
    public string? RelatedEntityType { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Message { get; set; }
    public DateTime RemindAt { get; set; }
    public bool IsEnabled { get; set; } = true;
    public bool IsSent { get; set; } = false;
    public DateTime? SentAt { get; set; }
    public ReminderChannel Channel { get; set; } = ReminderChannel.Push;
    public RepeatType? RepeatType { get; set; }
    public DateOnly? RepeatUntil { get; set; }

    public User User { get; set; } = null!;
    public ICollection<ReminderLog> Logs { get; set; } = new List<ReminderLog>();
}
