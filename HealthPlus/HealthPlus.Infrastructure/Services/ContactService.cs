using HealthPlus.Application.DTOs.Contact;
using HealthPlus.Application.Interfaces;
using HealthPlus.Infrastructure.Settings;
using Microsoft.Extensions.Options;

namespace HealthPlus.Infrastructure.Services;

// Gửi nội dung form Liên hệ về hộp thư đã cấu hình cho SMTP (appsettings:Email:SenderEmail) —
// dùng lại IEmailSender có sẵn thay vì thêm cấu hình người nhận riêng.
public class ContactService : IContactService
{
    private readonly IEmailSender _emailSender;
    private readonly EmailSettings _settings;

    public ContactService(IEmailSender emailSender, IOptions<EmailSettings> settings)
    {
        _emailSender = emailSender;
        _settings = settings.Value;
    }

    public async Task SendMessageAsync(ContactRequest request, CancellationToken ct = default)
    {
        var subject = $"[HealthPlus] Liên hệ mới từ {request.Name}";
        var body = $"Họ tên: {request.Name}\nEmail: {request.Email}\n\nNội dung:\n{request.Message}";
        await _emailSender.SendAsync(_settings.SenderEmail, subject, body, ct);
    }
}
