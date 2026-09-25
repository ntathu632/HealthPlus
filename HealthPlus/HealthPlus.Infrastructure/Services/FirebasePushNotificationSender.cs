using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using HealthPlus.Application.Interfaces;
using HealthPlus.Infrastructure.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HealthPlus.Infrastructure.Services;

public class FirebasePushNotificationSender : IPushNotificationSender
{
    private static readonly object _initLock = new();

    private readonly FirebaseSettings _settings;
    private readonly ILogger<FirebasePushNotificationSender> _logger;

    public FirebasePushNotificationSender(IOptions<FirebaseSettings> settings, ILogger<FirebasePushNotificationSender> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task SendAsync(string fcmToken, string title, string body, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(fcmToken))
        {
            _logger.LogWarning("[FCM] User chưa có FcmToken, bỏ qua gửi push \"{Title}\".", title);
            return;
        }

        if (!EnsureInitialized())
        {
            _logger.LogWarning(
                "[FCM] Chưa cấu hình Firebase credentials (appsettings:Firebase:CredentialsPath={Path}), bỏ qua gửi push.",
                _settings.CredentialsPath);
            return;
        }

        try
        {
            // Message.Token is the registration token the Angular client gets from
            // Firebase Messaging getToken() and stores as UserNotificationSetting.FcmToken.
            // Fid (Firebase Installation ID) is a different identifier the client doesn't collect.
#pragma warning disable CS0618
            var message = new Message
            {
                Token = fcmToken,
                Notification = new Notification { Title = title, Body = body }
            };
#pragma warning restore CS0618

            var messageId = await FirebaseMessaging.DefaultInstance.SendAsync(message, ct);
            _logger.LogInformation("[FCM] Đã gửi push {MessageId} tới token {Token}", messageId, Mask(fcmToken));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[FCM] Gửi push thất bại.");
        }
    }

    private bool EnsureInitialized()
    {
        if (FirebaseApp.DefaultInstance != null) return true;
        if (string.IsNullOrWhiteSpace(_settings.CredentialsPath) || !File.Exists(_settings.CredentialsPath))
            return false;

        lock (_initLock)
        {
            if (FirebaseApp.DefaultInstance != null) return true;

#pragma warning disable CS0618
            FirebaseApp.Create(new AppOptions
            {
                Credential = GoogleCredential.FromFile(_settings.CredentialsPath)
            });
#pragma warning restore CS0618
            return true;
        }
    }

    private static string Mask(string token) =>
        token.Length <= 8 ? "****" : $"{token[..4]}...{token[^4..]}";
}
