using HealthPlus.Application.DTOs.Reminders;

namespace HealthPlus.Application.Interfaces;

public interface IReminderService
{
    Task<IEnumerable<ReminderResponse>> GetAllAsync(Guid userId, bool? isEnabled, CancellationToken ct = default);
    Task<ReminderResponse> GetByIdAsync(Guid id, Guid userId, CancellationToken ct = default);
    Task<ReminderResponse> CreateAsync(Guid userId, CreateReminderRequest request, CancellationToken ct = default);
    Task<ReminderResponse> UpdateAsync(Guid id, Guid userId, UpdateReminderRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid id, Guid userId, CancellationToken ct = default);
    Task<IEnumerable<ReminderResponse>> GetUpcomingAsync(Guid userId, int hours = 24, CancellationToken ct = default);

    Task<NotificationSettingResponse> GetNotificationSettingAsync(Guid userId, CancellationToken ct = default);
    Task<NotificationSettingResponse> UpdateNotificationSettingAsync(Guid userId, UpdateNotificationSettingRequest request, CancellationToken ct = default);
}
