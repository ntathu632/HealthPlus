using FluentValidation.TestHelper;
using HealthPlus.Application.DTOs.Users;
using HealthPlus.Application.Validators;
using Xunit;

namespace HealthPlus.Tests.Validators;

public class ChangePasswordRequestValidatorTests
{
    private readonly ChangePasswordRequestValidator _validator = new();

    [Fact]
    public void ValidRequest_PassesValidation()
    {
        var request = new ChangePasswordRequest { CurrentPassword = "OldPass1", NewPassword = "NewPass1" };

        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void EmptyCurrentPassword_FailsValidation()
    {
        var request = new ChangePasswordRequest { CurrentPassword = "", NewPassword = "NewPass1" };

        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.CurrentPassword);
    }

    [Theory]
    [InlineData("short1A")]      // < 8 chars
    [InlineData("nouppercase1")] // no uppercase
    [InlineData("NoDigitsHere")] // no digit
    public void WeakNewPassword_FailsValidation(string newPassword)
    {
        var request = new ChangePasswordRequest { CurrentPassword = "OldPass1", NewPassword = newPassword };

        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.NewPassword);
    }

    [Fact]
    public void NewPasswordSameAsCurrent_FailsValidation()
    {
        var request = new ChangePasswordRequest { CurrentPassword = "SamePass1", NewPassword = "SamePass1" };

        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.NewPassword);
    }
}
