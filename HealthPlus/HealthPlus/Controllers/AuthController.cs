using FluentValidation;
using HealthPlus.Application.Common;
using HealthPlus.Application.DTOs.Auth;
using HealthPlus.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthPlus.Controllers;

public class AuthController : BaseApiController
{
    private readonly IAuthService _auth;
    private readonly IValidator<RegisterRequest> _registerValidator;
    private readonly IValidator<LoginRequest> _loginValidator;

    public AuthController(
        IAuthService auth,
        IValidator<RegisterRequest> registerValidator,
        IValidator<LoginRequest> loginValidator)
    {
        _auth = auth;
        _registerValidator = registerValidator;
        _loginValidator = loginValidator;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken ct)
    {
        var validation = await _registerValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return BadRequest(ApiResponse<AuthResponse>.Fail(validation.Errors.Select(e => e.ErrorMessage)));

        try
        {
            var result = await _auth.RegisterAsync(request, ct);
            return Ok(ApiResponse<AuthResponse>.Ok(result, "Đăng ký thành công."));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<AuthResponse>.Fail(ex.Message));
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        var validation = await _loginValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return BadRequest(ApiResponse<AuthResponse>.Fail(validation.Errors.Select(e => e.ErrorMessage)));

        try
        {
            var result = await _auth.LoginAsync(request, CurrentUserIp, ct);
            return Ok(ApiResponse<AuthResponse>.Ok(result, "Đăng nhập thành công."));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ApiResponse<AuthResponse>.Fail(ex.Message));
        }
    }

    [HttpPost("google-login")]
    public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequest request, CancellationToken ct)
    {
        try
        {
            var result = await _auth.GoogleLoginAsync(request.IdToken, CurrentUserIp, ct);
            return Ok(ApiResponse<AuthResponse>.Ok(result, "Đăng nhập thành công."));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ApiResponse<AuthResponse>.Fail(ex.Message));
        }
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request, CancellationToken ct)
    {
        try
        {
            var result = await _auth.RefreshTokenAsync(request.RefreshToken, ct);
            return Ok(ApiResponse<AuthResponse>.Ok(result));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ApiResponse<AuthResponse>.Fail(ex.Message));
        }
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenRequest request, CancellationToken ct)
    {
        await _auth.LogoutAsync(request.RefreshToken, ct);
        return Ok(ApiResponse.Ok("Đăng xuất thành công."));
    }

    [Authorize]
    [HttpPost("revoke-all")]
    public async Task<IActionResult> RevokeAll(CancellationToken ct)
    {
        await _auth.RevokeAllTokensAsync(CurrentUserId, ct);
        return Ok(ApiResponse.Ok("Đã thu hồi tất cả phiên đăng nhập."));
    }
}
