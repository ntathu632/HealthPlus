namespace HealthPlus.Application.Interfaces;

public interface IPushNotificationSender
{
    Task SendAsync(string fcmToken, string title, string body, CancellationToken ct = default);
}
