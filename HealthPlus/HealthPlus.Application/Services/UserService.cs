using HealthPlus.Application.DTOs.Users;
using HealthPlus.Application.Interfaces;
using HealthPlus.Domain.Interfaces.Repositories;

namespace HealthPlus.Application.Services;

public class UserService : IUserService
{
    private readonly IUnitOfWork _uow;
    private readonly IPasswordService _password;

    public UserService(IUnitOfWork uow, IPasswordService password)
    {
        _uow = uow;
        _password = password;
    }

    public async Task<UserResponse> GetByIdAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await _uow.Users.GetByIdAsync(userId, ct)
            ?? throw new KeyNotFoundException("Không tìm thấy người dùng.");

        var userRoles = await _uow.UserRoles.FindAsync(ur => ur.UserId == userId, ct);
        var roleIds = userRoles.Select(ur => ur.RoleId).ToList();
        var roles = await _uow.Roles.FindAsync(r => roleIds.Contains(r.Id), ct);

        string? hospitalName = null;
        if (user.HospitalId.HasValue)
        {
            var hospital = await _uow.Hospitals.GetByIdAsync(user.HospitalId.Value, ct);
            hospitalName = hospital?.Name;
        }

        return new UserResponse
        {
            Id = user.Id,
            Email = user.Email,
            FullName = user.FullName,
            PhoneNumber = user.PhoneNumber,
            AvatarUrl = user.AvatarUrl,
            IsEmailVerified = user.IsEmailVerified,
            LastLoginAt = user.LastLoginAt,
            CreatedAt = user.CreatedAt,
            Roles = roles.Select(r => r.Name),
            HospitalId = user.HospitalId,
            HospitalName = hospitalName,
            Specialty = user.Specialty,
            ConsultationFee = user.ConsultationFee,
        };
    }

    public async Task<UserResponse> UpdateProfileAsync(Guid userId, UpdateUserRequest request, CancellationToken ct = default)
    {
        var user = await _uow.Users.GetByIdAsync(userId, ct)
            ?? throw new KeyNotFoundException("Không tìm thấy người dùng.");

        user.FullName = request.FullName.Trim();
        user.PhoneNumber = request.PhoneNumber?.Trim();

        var isDoctor = await _uow.UserRoles.AnyAsync(ur => ur.UserId == userId && ur.RoleId == 2, ct);
        if (isDoctor)
        {
            user.HospitalId = request.HospitalId;
            user.Specialty = request.Specialty?.Trim();
            user.ConsultationFee = request.ConsultationFee;
        }

        user.UpdatedAt = DateTime.UtcNow;
        _uow.Users.Update(user);
        await _uow.SaveChangesAsync(ct);

        return await GetByIdAsync(userId, ct);
    }

    public async Task ChangePasswordAsync(Guid userId, ChangePasswordRequest request, CancellationToken ct = default)
    {
        var user = await _uow.Users.GetByIdAsync(userId, ct)
            ?? throw new KeyNotFoundException("Không tìm thấy người dùng.");

        if (!_password.Verify(request.CurrentPassword, user.PasswordHash))
            throw new InvalidOperationException("Mật khẩu hiện tại không đúng.");

        user.PasswordHash = _password.Hash(request.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;
        _uow.Users.Update(user);
        await _uow.SaveChangesAsync(ct);
    }

    public async Task UpdateAvatarAsync(Guid userId, string avatarUrl, CancellationToken ct = default)
    {
        var user = await _uow.Users.GetByIdAsync(userId, ct)
            ?? throw new KeyNotFoundException("Không tìm thấy người dùng.");

        user.AvatarUrl = avatarUrl;
        user.UpdatedAt = DateTime.UtcNow;
        _uow.Users.Update(user);
        await _uow.SaveChangesAsync(ct);
    }
}
