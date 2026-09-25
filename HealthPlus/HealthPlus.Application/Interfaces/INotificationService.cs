using HealthPlus.Domain.Entities;
using HealthPlus.Domain.Enums;

namespace HealthPlus.Application.Interfaces;

public interface INotificationService
{
    Task SendAsync(Reminder reminder, UserNotificationSetting? settings, CancellationToken ct = default);
}
