using FluentValidation.TestHelper;
using HealthPlus.Application.DTOs.Auth;
using HealthPlus.Application.Validators;
using Xunit;

namespace HealthPlus.Tests.Validators;

public class LoginRequestValidatorTests
{
    private readonly LoginRequestValidator _validator = new();

    [Fact]
    public void ValidRequest_PassesValidation()
    {
        var result = _validator.TestValidate(new LoginRequest { Email = "user@example.com", Password = "anything" });
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void EmptyEmail_FailsValidation()
    {
        var result = _validator.TestValidate(new LoginRequest { Email = "", Password = "anything" });
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void NonEmailUsername_PassesValidation()
    {
        // Login không bắt buộc đúng định dạng email — hỗ trợ tài khoản demo dùng tên đăng nhập
        // đơn giản (vd. "admin"/"bacsi"/"benhnhan"), chỉ RegisterRequestValidator mới bắt buộc email chuẩn.
        var result = _validator.TestValidate(new LoginRequest { Email = "not-an-email", Password = "anything" });
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void EmptyPassword_FailsValidation()
    {
        var result = _validator.TestValidate(new LoginRequest { Email = "user@example.com", Password = "" });
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }
}
