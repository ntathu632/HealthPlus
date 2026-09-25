using HealthPlus.Application.Interfaces;
using HealthPlus.Infrastructure.Settings;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace HealthPlus.Infrastructure.Services;

public class SmtpEmailSender : IEmailSender
{
    private readonly EmailSettings _settings;
    private readonly ILogger<SmtpEmailSender> _logger;

    public SmtpEmailSender(IOptions<EmailSettings> settings, ILogger<SmtpEmailSender> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task SendAsync(string toEmail, string subject, string body, CancellationToken ct = default)
    {
        if (!_settings.IsConfigured)
        {
            _logger.LogWarning("[EMAIL] Chưa cấu hình SMTP (appsettings:Email), bỏ qua gửi email tới {To}.", toEmail);
            return;
        }

        if (string.IsNullOrWhiteSpace(toEmail))
        {
            _logger.LogWarning("[EMAIL] Không có địa chỉ email hợp lệ để gửi \"{Subject}\".", subject);
            return;
        }

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_settings.SenderName, _settings.SenderEmail));
        message.To.Add(MailboxAddress.Parse(toEmail));
        message.Subject = subject;
        message.Body = new TextPart("plain") { Text = body };

        using var client = new SmtpClient();
        try
        {
            await client.ConnectAsync(_settings.Host, _settings.Port,
                _settings.UseSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.Auto, ct);

            if (!string.IsNullOrWhiteSpace(_settings.Username))
                await client.AuthenticateAsync(_settings.Username, _settings.Password, ct);

            await client.SendAsync(message, ct);
            await client.DisconnectAsync(true, ct);

            _logger.LogInformation("[EMAIL] Đã gửi tới {To}: {Subject}", toEmail, subject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[EMAIL] Gửi thất bại tới {To}.", toEmail);
        }
    }
}
