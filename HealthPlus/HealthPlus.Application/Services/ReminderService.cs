using HealthPlus.Application.DTOs.Reminders;
using HealthPlus.Application.Interfaces;
using HealthPlus.Domain.Entities;
using HealthPlus.Domain.Interfaces.Repositories;

namespace HealthPlus.Application.Services;

public class ReminderService : IReminderService
{
    private readonly IUnitOfWork _uow;
    public ReminderService(IUnitOfWork uow) => _uow = uow;

    public async Task<IEnumerable<ReminderResponse>> GetAllAsync(Guid userId, bool? isEnabled, CancellationToken ct = default)
    {
        var reminders = await _uow.Reminders.FindAsync(
            r => r.UserId == userId && (isEnabled == null || r.IsEnabled == isEnabled), ct);
        return reminders.OrderBy(r => r.RemindAt).Select(Map);
    }

    public async Task<ReminderResponse> GetByIdAsync(Guid id, Guid userId, CancellationToken ct = default)
        => Map(await GetOwnedAsync(id, userId, ct));

    public async Task<ReminderResponse> CreateAsync(Guid userId, CreateReminderRequest req, CancellationToken ct = default)
    {
        var entity = new Reminder
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            ReminderType = req.ReminderType,
            RelatedEntityId = req.RelatedEntityId,
            RelatedEntityType = req.RelatedEntityType?.Trim(),
            Title = req.Title.Trim(),
            Message = req.Message?.Trim(),
            RemindAt = req.RemindAt,
            IsEnabled = true,
            IsSent = false,
            Channel = req.Channel,
            RepeatType = req.RepeatType,
            RepeatUntil = req.RepeatUntil,
        };
        await _uow.Reminders.AddAsync(entity, ct);
        await _uow.SaveChangesAsync(ct);
        return Map(entity);
    }

    public async Task<ReminderResponse> UpdateAsync(Guid id, Guid userId, UpdateReminderRequest req, CancellationToken ct = default)
    {
        var entity = await GetOwnedAsync(id, userId, ct);
        entity.Title = req.Title.Trim();
        entity.Message = req.Message?.Trim();
        entity.RemindAt = req.RemindAt;
        entity.IsEnabled = req.IsEnabled;
        entity.Channel = req.Channel;
        entity.RepeatType = req.RepeatType;
        entity.RepeatUntil = req.RepeatUntil;
        _uow.Reminders.Update(entity);
        await _uow.SaveChangesAsync(ct);
        return Map(entity);
    }

    public async Task DeleteAsync(Guid id, Guid userId, CancellationToken ct = default)
    {
        var entity = await GetOwnedAsync(id, userId, ct);
        _uow.Reminders.Remove(entity);
        await _uow.SaveChangesAsync(ct);
    }

    public async Task<IEnumerable<ReminderResponse>> GetUpcomingAsync(Guid userId, int hours = 24, CancellationToken ct = default)
    {
        var until = DateTime.UtcNow.AddHours(hours);
        var reminders = await _uow.Reminders.FindAsync(
            r => r.UserId == userId && r.IsEnabled && !r.IsSent && r.RemindAt <= until, ct);
        return reminders.OrderBy(r => r.RemindAt).Select(Map);
    }

    public async Task<NotificationSettingResponse> GetNotificationSettingAsync(Guid userId, CancellationToken ct = default)
    {
        var setting = await _uow.UserNotificationSettings.GetByIdAsync(userId, ct);
        if (setting is null)
        {
            setting = new UserNotificationSetting { UserId = userId };
            await _uow.UserNotificationSettings.AddAsync(setting, ct);
            await _uow.SaveChangesAsync(ct);
        }
        return MapSetting(setting);
    }

    public async Task<NotificationSettingResponse> UpdateNotificationSettingAsync(Guid userId, UpdateNotificationSettingRequest req, CancellationToken ct = default)
    {
        var setting = await _uow.UserNotificationSettings.GetByIdAsync(userId, ct);
        if (setting is null)
        {
            setting = new UserNotificationSetting { UserId = userId };
            await _uow.UserNotificationSettings.AddAsync(setting, ct);
        }
        setting.PushEnabled = req.PushEnabled;
        setting.EmailEnabled = req.EmailEnabled;
        setting.SmsEnabled = req.SmsEnabled;
        setting.VaccineReminderEnabled = req.VaccineReminderEnabled;
        setting.MedicineReminderEnabled = req.MedicineReminderEnabled;
        setting.FollowUpReminderEnabled = req.FollowUpReminderEnabled;
        setting.QuietHourStart = req.QuietHourStart;
        setting.QuietHourEnd = req.QuietHourEnd;
        setting.FcmToken = req.FcmToken;
        setting.UpdatedAt = DateTime.UtcNow;
        _uow.UserNotificationSettings.Update(setting);
        await _uow.SaveChangesAsync(ct);
        return MapSetting(setting);
    }

    private async Task<Reminder> GetOwnedAsync(Guid id, Guid userId, CancellationToken ct)
    {
        var entity = await _uow.Reminders.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException("Không tìm thấy nhắc nhở.");
        if (entity.UserId != userId)
            throw new UnauthorizedAccessException("Bạn không có quyền truy cập.");
        return entity;
    }

    private static ReminderResponse Map(Reminder r) => new()
    {
        Id = r.Id, UserId = r.UserId, ReminderType = r.ReminderType,
        RelatedEntityId = r.RelatedEntityId, RelatedEntityType = r.RelatedEntityType,
        Title = r.Title, Message = r.Message, RemindAt = r.RemindAt,
        IsEnabled = r.IsEnabled, IsSent = r.IsSent, SentAt = r.SentAt,
        Channel = r.Channel, RepeatType = r.RepeatType, RepeatUntil = r.RepeatUntil,
        CreatedAt = r.CreatedAt,
    };

    private static NotificationSettingResponse MapSetting(UserNotificationSetting s) => new()
    {
        UserId = s.UserId, PushEnabled = s.PushEnabled, EmailEnabled = s.EmailEnabled,
        SmsEnabled = s.SmsEnabled, VaccineReminderEnabled = s.VaccineReminderEnabled,
        MedicineReminderEnabled = s.MedicineReminderEnabled, FollowUpReminderEnabled = s.FollowUpReminderEnabled,
        QuietHourStart = s.QuietHourStart, QuietHourEnd = s.QuietHourEnd,
        FcmToken = s.FcmToken, UpdatedAt = s.UpdatedAt,
    };
}
