using HealthPlus.Application.Interfaces;
using HealthPlus.Domain.Entities;
using HealthPlus.Domain.Interfaces.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HealthPlus.Infrastructure.BackgroundServices;

public class ReminderBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ReminderBackgroundService> _logger;
    private static readonly TimeSpan _interval = TimeSpan.FromMinutes(1);

    public ReminderBackgroundService(IServiceScopeFactory scopeFactory, ILogger<ReminderBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("ReminderBackgroundService started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessDueRemindersAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing reminders.");
            }

            await Task.Delay(_interval, stoppingToken);
        }
    }

    private async Task ProcessDueRemindersAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var notifier = scope.ServiceProvider.GetRequiredService<INotificationService>();

        var now = DateTime.UtcNow;
        var dueReminders = await uow.Reminders.FindAsync(
            r => r.IsEnabled && !r.IsSent && r.RemindAt <= now, ct);

        var reminderList = dueReminders.ToList();
        if (reminderList.Count == 0) return;

        _logger.LogInformation("Processing {Count} due reminder(s).", reminderList.Count);

        foreach (var reminder in reminderList)
        {
            var settings = await uow.UserNotificationSettings.GetByIdAsync(reminder.UserId, ct);

            if (!ShouldSend(reminder, settings))
            {
                MarkSent(reminder, "skipped — quiet hours or channel disabled");
                uow.Reminders.Update(reminder);
                continue;
            }

            await notifier.SendAsync(reminder, settings, ct);

            await uow.ReminderLogs.AddAsync(new ReminderLog
            {
                ReminderId = reminder.Id,
                Channel = reminder.Channel.ToString(),
                Status = "sent",
                SentAt = DateTime.UtcNow,
            }, ct);

            // Handle repeat
            if (reminder.RepeatType.HasValue && reminder.RepeatType.Value != Domain.Enums.RepeatType.None)
            {
                var next = reminder.RepeatType.Value == Domain.Enums.RepeatType.Daily
                    ? reminder.RemindAt.AddDays(1)
                    : reminder.RemindAt.AddDays(7);

                if (reminder.RepeatUntil == null || next <= reminder.RepeatUntil.Value.ToDateTime(TimeOnly.MinValue))
                {
                    reminder.RemindAt = next;
                    reminder.IsSent = false;
                }
                else
                {
                    MarkSent(reminder, "repeat ended");
                }
            }
            else
            {
                MarkSent(reminder, "sent");
            }

            uow.Reminders.Update(reminder);
        }

        await uow.SaveChangesAsync(ct);
    }

    private static bool ShouldSend(Reminder reminder, UserNotificationSetting? settings)
    {
        if (settings is null) return true;

        // Kiểm tra Quiet Hours
        if (settings.QuietHourStart.HasValue && settings.QuietHourEnd.HasValue)
        {
            var now = TimeOnly.FromDateTime(DateTime.Now);
            var start = settings.QuietHourStart.Value;
            var end = settings.QuietHourEnd.Value;

            bool inQuiet = start < end
                ? now >= start && now <= end
                : now >= start || now <= end;

            if (inQuiet) return false;
        }

        return reminder.Channel switch
        {
            Domain.Enums.ReminderChannel.Push => settings.PushEnabled,
            Domain.Enums.ReminderChannel.Email => settings.EmailEnabled,
            Domain.Enums.ReminderChannel.Sms => settings.SmsEnabled,
            _ => true
        };
    }

    private static void MarkSent(Reminder reminder, string reason)
    {
        reminder.IsSent = true;
        reminder.SentAt = DateTime.UtcNow;
    }
}
