using HealthPlus.Application.Interfaces;
using HealthPlus.Infrastructure.BackgroundServices;
using HealthPlus.Infrastructure.Services;
using HealthPlus.Infrastructure.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HealthPlus.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtSettings>(configuration.GetSection("Jwt"));
        services.Configure<EmailSettings>(configuration.GetSection("Email"));
        services.Configure<FirebaseSettings>(configuration.GetSection("Firebase"));
        services.Configure<TwilioSettings>(configuration.GetSection("Twilio"));
        services.Configure<AiChatSettings>(configuration.GetSection("AiChat"));
        services.Configure<GoogleSettings>(configuration.GetSection("Google"));
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IPasswordService, PasswordService>();
        services.AddScoped<IEmailSender, SmtpEmailSender>();
        services.AddScoped<IPushNotificationSender, FirebasePushNotificationSender>();
        services.AddScoped<ISmsSender, TwilioSmsSender>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IFileStorageService, LocalFileStorageService>();
        services.AddScoped<IOcrService, TesseractOcrService>();
        services.AddScoped<IContactService, ContactService>();
        services.AddScoped<IGoogleTokenValidator, GoogleTokenValidator>();

        services.AddHttpClient<IAiCompletionClient, GeminiCompletionClient>(client =>
        {
            client.BaseAddress = new Uri("https://generativelanguage.googleapis.com/");
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        services.AddHostedService<ReminderBackgroundService>();

        return services;
    }
}
