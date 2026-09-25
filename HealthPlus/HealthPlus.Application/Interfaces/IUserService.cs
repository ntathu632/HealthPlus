using HealthPlus.Application.DTOs.Users;

namespace HealthPlus.Application.Interfaces;

public interface IUserService
{
    Task<UserResponse> GetByIdAsync(Guid userId, CancellationToken ct = default);
    Task<UserResponse> UpdateProfileAsync(Guid userId, UpdateUserRequest request, CancellationToken ct = default);
    Task ChangePasswordAsync(Guid userId, ChangePasswordRequest request, CancellationToken ct = default);
    Task UpdateAvatarAsync(Guid userId, string avatarUrl, CancellationToken ct = default);
}
