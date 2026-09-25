namespace HealthPlus.Application.Interfaces;

public record GoogleUserInfo(string Email, string Name);

// Trừu tượng hoá việc verify ID token Google (Google.Apis.Auth) — để AuthService
// không phụ thuộc trực tiếp vào thư viện của Google.
public interface IGoogleTokenValidator
{
    Task<GoogleUserInfo?> ValidateAsync(string idToken, CancellationToken ct = default);
}
