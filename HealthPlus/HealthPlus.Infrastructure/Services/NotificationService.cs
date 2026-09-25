using HealthPlus.Application.Interfaces;
using HealthPlus.Domain.Entities;
using HealthPlus.Domain.Enums;
using HealthPlus.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

namespace HealthPlus.Infrastructure.Services;

public class NotificationService : INotificationService
{
    private readonly IEmailSender _emailSender;
    private readonly IPushNotificationSender _pushSender;
    private readonly ISmsSender _smsSender;
    private readonly IUnitOfWork _uow;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        IEmailSender emailSender,
        IPushNotificationSender pushSender,
        ISmsSender smsSender,
        IUnitOfWork uow,
        ILogger<NotificationService> logger)
    {
        _emailSender = emailSender;
        _pushSender = pushSender;
        _smsSender = smsSender;
        _uow = uow;
        _logger = logger;
    }

    public async Task SendAsync(Reminder reminder, UserNotificationSetting? settings, CancellationToken ct = default)
    {
        var body = reminder.Message ?? string.Empty;

        switch (reminder.Channel)
        {
            case ReminderChannel.Push:
                await _pushSender.SendAsync(settings?.FcmToken ?? string.Empty, reminder.Title, body, ct);
                break;

            case ReminderChannel.Email:
                var emailUser = await _uow.Users.GetByIdAsync(reminder.UserId, ct);
                await _emailSender.SendAsync(emailUser?.Email ?? string.Empty, reminder.Title, body, ct);
                break;

            case ReminderChannel.Sms:
                var smsUser = await _uow.Users.GetByIdAsync(reminder.UserId, ct);
                var smsBody = string.IsNullOrWhiteSpace(body) ? reminder.Title : $"{reminder.Title}: {body}";
                await _smsSender.SendAsync(smsUser?.PhoneNumber ?? string.Empty, smsBody, ct);
                break;
        }
    }
}
