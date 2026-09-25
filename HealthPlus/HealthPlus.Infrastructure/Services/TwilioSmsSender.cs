using HealthPlus.Application.Interfaces;
using HealthPlus.Infrastructure.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace HealthPlus.Infrastructure.Services;

public class TwilioSmsSender : ISmsSender
{
    private readonly TwilioSettings _settings;
    private readonly ILogger<TwilioSmsSender> _logger;

    public TwilioSmsSender(IOptions<TwilioSettings> settings, ILogger<TwilioSmsSender> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task SendAsync(string phoneNumber, string message, CancellationToken ct = default)
    {
        if (!_settings.IsConfigured)
        {
            _logger.LogWarning("[SMS] Chưa cấu hình Twilio (appsettings:Twilio), bỏ qua gửi SMS.");
            return;
        }

        if (string.IsNullOrWhiteSpace(phoneNumber))
        {
            _logger.LogWarning("[SMS] Không có số điện thoại hợp lệ để gửi.");
            return;
        }

        try
        {
            TwilioClient.Init(_settings.AccountSid, _settings.AuthToken);

            var result = await MessageResource.CreateAsync(
                body: message,
                from: new PhoneNumber(_settings.FromPhoneNumber),
                to: new PhoneNumber(ToE164(phoneNumber)));

            _logger.LogInformation("[SMS] Đã gửi {Sid} tới {Phone}", result.Sid, Mask(phoneNumber));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[SMS] Gửi thất bại tới {Phone}.", Mask(phoneNumber));
        }
    }

    // Số điện thoại trong DB lưu dạng nội địa VN (vd "0912345678", theo RegisterRequestValidator),
    // còn Twilio cần chuẩn E.164 (vd "+84912345678").
    internal static string ToE164(string phoneNumber)
    {
        if (phoneNumber.StartsWith('+')) return phoneNumber;
        if (phoneNumber.StartsWith('0')) return "+84" + phoneNumber[1..];
        return "+84" + phoneNumber;
    }

    private static string Mask(string phoneNumber) =>
        phoneNumber.Length <= 4 ? "****" : $"***{phoneNumber[^4..]}";
}
