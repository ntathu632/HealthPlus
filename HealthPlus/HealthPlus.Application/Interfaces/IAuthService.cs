using HealthPlus.Application.DTOs.Auth;

namespace HealthPlus.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default);
    Task<AuthResponse> LoginAsync(LoginRequest request, string? ipAddress = null, CancellationToken ct = default);
    Task<AuthResponse> GoogleLoginAsync(string idToken, string? ipAddress = null, CancellationToken ct = default);
    Task<AuthResponse> RefreshTokenAsync(string refreshToken, CancellationToken ct = default);
    Task LogoutAsync(string refreshToken, CancellationToken ct = default);
    Task RevokeAllTokensAsync(Guid userId, CancellationToken ct = default);
}
