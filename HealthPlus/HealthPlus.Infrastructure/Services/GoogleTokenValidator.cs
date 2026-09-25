using Google.Apis.Auth;
using HealthPlus.Application.Interfaces;
using HealthPlus.Infrastructure.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HealthPlus.Infrastructure.Services;

public class GoogleTokenValidator : IGoogleTokenValidator
{
    private readonly GoogleSettings _settings;
    private readonly ILogger<GoogleTokenValidator> _logger;

    public GoogleTokenValidator(IOptions<GoogleSettings> settings, ILogger<GoogleTokenValidator> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<GoogleUserInfo?> ValidateAsync(string idToken, CancellationToken ct = default)
    {
        try
        {
            var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = [_settings.ClientId],
            });
            return new GoogleUserInfo(payload.Email, payload.Name);
        }
        catch (InvalidJwtException ex)
        {
            _logger.LogWarning(ex, "[GoogleAuth] ID token không hợp lệ.");
            return null;
        }
    }
}
