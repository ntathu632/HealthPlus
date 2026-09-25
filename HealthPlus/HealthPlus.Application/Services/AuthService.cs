using HealthPlus.Application.DTOs.Auth;
using HealthPlus.Application.Interfaces;
using HealthPlus.Domain.Entities;
using HealthPlus.Domain.Interfaces.Repositories;

namespace HealthPlus.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _uow;
    private readonly IJwtService _jwt;
    private readonly IPasswordService _password;
    private readonly IGoogleTokenValidator _googleValidator;

    public AuthService(IUnitOfWork uow, IJwtService jwt, IPasswordService password, IGoogleTokenValidator googleValidator)
    {
        _uow = uow;
        _jwt = jwt;
        _password = password;
        _googleValidator = googleValidator;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        if (await _uow.Users.AnyAsync(u => u.Email == request.Email.ToLower(), ct))
            throw new InvalidOperationException("Email đã được sử dụng.");

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email.ToLower().Trim(),
            PasswordHash = _password.Hash(request.Password),
            FullName = request.FullName.Trim(),
            PhoneNumber = request.PhoneNumber?.Trim(),
            IsActive = true,
            IsEmailVerified = false,
        };

        await _uow.Users.AddAsync(user, ct);

        // Gán role "User" mặc định (Id = 3)
        var userRole = new UserRole { UserId = user.Id, RoleId = 3 };
        await _uow.UserRoles.AddAsync(userRole, ct);

        // Tạo notification setting mặc định
        var notifSetting = new UserNotificationSetting { UserId = user.Id };
        await _uow.UserNotificationSettings.AddAsync(notifSetting, ct);

        await _uow.SaveChangesAsync(ct);

        return await BuildAuthResponseAsync(user, ["User"], null, ct);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, string? ipAddress = null, CancellationToken ct = default)
    {
        var user = await _uow.Users.FirstOrDefaultAsync(
            u => u.Email == request.Email.ToLower() && u.IsActive, ct)
            ?? throw new UnauthorizedAccessException("Email hoặc mật khẩu không đúng.");

        if (!_password.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Email hoặc mật khẩu không đúng.");

        var roles = await GetUserRolesAsync(user.Id, ct);
        var response = await BuildAuthResponseAsync(user, roles, ipAddress, ct);

        // Cập nhật LastLoginAt
        user.LastLoginAt = DateTime.UtcNow;
        _uow.Users.Update(user);
        await _uow.SaveChangesAsync(ct);

        return response;
    }

    public async Task<AuthResponse> GoogleLoginAsync(string idToken, string? ipAddress = null, CancellationToken ct = default)
    {
        var googleUser = await _googleValidator.ValidateAsync(idToken, ct)
            ?? throw new UnauthorizedAccessException("Google token không hợp lệ.");

        var email = googleUser.Email.ToLower().Trim();
        var user = await _uow.Users.FirstOrDefaultAsync(u => u.Email == email, ct);

        if (user is null)
        {
            user = new User
            {
                Id = Guid.NewGuid(),
                Email = email,
                // Tài khoản đăng nhập qua Google không có mật khẩu thật — lưu hash của 1 chuỗi
                // ngẫu nhiên để cột PasswordHash (NOT NULL) không bao giờ khớp mật khẩu ai đó tự nhập.
                PasswordHash = _password.Hash(Guid.NewGuid().ToString()),
                FullName = string.IsNullOrWhiteSpace(googleUser.Name) ? email : googleUser.Name,
                IsActive = true,
                IsEmailVerified = true,
            };

            await _uow.Users.AddAsync(user, ct);

            var userRole = new UserRole { UserId = user.Id, RoleId = 3 };
            await _uow.UserRoles.AddAsync(userRole, ct);

            var notifSetting = new UserNotificationSetting { UserId = user.Id };
            await _uow.UserNotificationSettings.AddAsync(notifSetting, ct);

            await _uow.SaveChangesAsync(ct);
        }
        else if (!user.IsActive)
        {
            throw new UnauthorizedAccessException("Tài khoản đã bị khóa.");
        }

        var roles = await GetUserRolesAsync(user.Id, ct);
        var response = await BuildAuthResponseAsync(user, roles, ipAddress, ct);

        user.LastLoginAt = DateTime.UtcNow;
        _uow.Users.Update(user);
        await _uow.SaveChangesAsync(ct);

        return response;
    }

    public async Task<AuthResponse> RefreshTokenAsync(string refreshToken, CancellationToken ct = default)
    {
        var token = await _uow.RefreshTokens.FirstOrDefaultAsync(
            rt => rt.Token == refreshToken && !rt.IsRevoked && rt.ExpiresAt > DateTime.UtcNow, ct)
            ?? throw new UnauthorizedAccessException("Refresh token không hợp lệ hoặc đã hết hạn.");

        var user = await _uow.Users.GetByIdAsync(token.UserId, ct)
            ?? throw new UnauthorizedAccessException("Người dùng không tồn tại.");

        if (!user.IsActive)
            throw new UnauthorizedAccessException("Tài khoản đã bị khóa.");

        // Thu hồi token cũ
        token.IsRevoked = true;
        token.RevokedAt = DateTime.UtcNow;
        _uow.RefreshTokens.Update(token);

        var roles = await GetUserRolesAsync(user.Id, ct);
        var response = await BuildAuthResponseAsync(user, roles, token.IpAddress, ct);

        await _uow.SaveChangesAsync(ct);
        return response;
    }

    public async Task LogoutAsync(string refreshToken, CancellationToken ct = default)
    {
        var token = await _uow.RefreshTokens.FirstOrDefaultAsync(
            rt => rt.Token == refreshToken && !rt.IsRevoked, ct);

        if (token is null) return;

        token.IsRevoked = true;
        token.RevokedAt = DateTime.UtcNow;
        _uow.RefreshTokens.Update(token);
        await _uow.SaveChangesAsync(ct);
    }

    public async Task RevokeAllTokensAsync(Guid userId, CancellationToken ct = default)
    {
        var tokens = await _uow.RefreshTokens.FindAsync(
            rt => rt.UserId == userId && !rt.IsRevoked, ct);

        foreach (var token in tokens)
        {
            token.IsRevoked = true;
            token.RevokedAt = DateTime.UtcNow;
            _uow.RefreshTokens.Update(token);
        }
        await _uow.SaveChangesAsync(ct);
    }

    // ─── Private helpers ─────────────────────────────────────────────────────

    private async Task<AuthResponse> BuildAuthResponseAsync(
        User user, IEnumerable<string> roles, string? ipAddress, CancellationToken ct)
    {
        var accessToken = _jwt.GenerateAccessToken(user, roles);
        var refreshTokenValue = _jwt.GenerateRefreshToken();
        var expiresAt = _jwt.GetAccessTokenExpiry();

        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = refreshTokenValue,
            IpAddress = ipAddress,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
        };
        await _uow.RefreshTokens.AddAsync(refreshToken, ct);

        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshTokenValue,
            AccessTokenExpiresAt = expiresAt,
            User = new UserInfo
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                AvatarUrl = user.AvatarUrl,
                Roles = roles,
            }
        };
    }

    private async Task<IEnumerable<string>> GetUserRolesAsync(Guid userId, CancellationToken ct)
    {
        var userRoles = await _uow.UserRoles.FindAsync(ur => ur.UserId == userId, ct);
        var roleIds = userRoles.Select(ur => ur.RoleId).ToList();
        var roles = await _uow.Roles.FindAsync(r => roleIds.Contains(r.Id), ct);
        return roles.Select(r => r.Name);
    }
}
