using FluentValidation;
using HealthPlus.Application.Common;
using HealthPlus.Application.DTOs.Users;
using HealthPlus.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthPlus.Controllers;

[Authorize]
public class UsersController : BaseApiController
{
    private readonly IUserService _users;
    private readonly IFileStorageService _storage;
    private readonly IValidator<ChangePasswordRequest> _changePasswordValidator;

    public UsersController(IUserService users, IFileStorageService storage, IValidator<ChangePasswordRequest> changePasswordValidator)
    {
        _users = users;
        _storage = storage;
        _changePasswordValidator = changePasswordValidator;
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMe(CancellationToken ct)
    {
        try
        {
            var result = await _users.GetByIdAsync(CurrentUserId, ct);
            return Ok(ApiResponse<UserResponse>.Ok(result));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<UserResponse>.Fail(ex.Message));
        }
    }

    [HttpPut("me")]
    public async Task<IActionResult> UpdateMe([FromBody] UpdateUserRequest request, CancellationToken ct)
    {
        try
        {
            var result = await _users.UpdateProfileAsync(CurrentUserId, request, ct);
            return Ok(ApiResponse<UserResponse>.Ok(result, "Cập nhật thông tin thành công."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<UserResponse>.Fail(ex.Message));
        }
    }

    [HttpPut("me/password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request, CancellationToken ct)
    {
        var validation = await _changePasswordValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return BadRequest(ApiResponse.Fail(validation.Errors.Select(e => e.ErrorMessage)));

        try
        {
            await _users.ChangePasswordAsync(CurrentUserId, request, ct);
            return Ok(ApiResponse.Ok("Đổi mật khẩu thành công."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.Fail(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.Fail(ex.Message));
        }
    }

    [HttpPost("me/avatar")]
    [RequestSizeLimit(2 * 1024 * 1024)] // 2 MB
    public async Task<IActionResult> UploadAvatar(IFormFile avatar, CancellationToken ct)
    {
        if (avatar is null || avatar.Length == 0)
            return BadRequest(ApiResponse.Fail("Vui lòng chọn ảnh."));

        var allowedTypes = new[] { "image/jpeg", "image/png" };
        if (!allowedTypes.Contains(avatar.ContentType.ToLower()))
            return BadRequest(ApiResponse.Fail("Chỉ hỗ trợ JPG và PNG."));

        try
        {
            await using var stream = avatar.OpenReadStream();
            var (_, url) = await _storage.SaveAsync(stream, avatar.FileName, "avatars", ct);
            await _users.UpdateAvatarAsync(CurrentUserId, url, ct);

            return Ok(ApiResponse<object>.Ok(new { AvatarUrl = url }, "Cập nhật ảnh đại diện thành công."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse.Fail(ex.Message));
        }
    }
}
