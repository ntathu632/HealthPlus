namespace HealthPlus.Domain.Entities;

public class ReminderLog
{
    public long Id { get; set; }
    public Guid ReminderId { get; set; }
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
    public string Channel { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? ErrorMsg { get; set; }

    public Reminder Reminder { get; set; } = null!;
}
