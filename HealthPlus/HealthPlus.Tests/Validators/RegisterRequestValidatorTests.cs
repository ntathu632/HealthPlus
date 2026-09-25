using FluentValidation.TestHelper;
using HealthPlus.Application.DTOs.Auth;
using HealthPlus.Application.Validators;
using Xunit;

namespace HealthPlus.Tests.Validators;

public class RegisterRequestValidatorTests
{
    private readonly RegisterRequestValidator _validator = new();

    private static RegisterRequest ValidRequest() => new()
    {
        Email = "user@example.com",
        Password = "Password1",
        FullName = "Nguyen Van A",
        PhoneNumber = "0912345678",
    };

    [Fact]
    public void ValidRequest_PassesValidation()
    {
        var result = _validator.TestValidate(ValidRequest());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-an-email")]
    public void InvalidEmail_FailsValidation(string email)
    {
        var request = ValidRequest();
        request.Email = email;

        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Theory]
    [InlineData("short1A")]      // < 8 chars
    [InlineData("nouppercase1")] // no uppercase
    [InlineData("NoDigitsHere")] // no digit
    public void WeakPassword_FailsValidation(string password)
    {
        var request = ValidRequest();
        request.Password = password;

        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void EmptyFullName_FailsValidation()
    {
        var request = ValidRequest();
        request.FullName = "";

        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.FullName);
    }

    [Theory]
    [InlineData("123")]
    [InlineData("abcdefghij")]
    public void InvalidPhoneNumber_FailsValidation(string phone)
    {
        var request = ValidRequest();
        request.PhoneNumber = phone;

        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.PhoneNumber);
    }

    [Fact]
    public void EmptyPhoneNumber_IsAllowed()
    {
        var request = ValidRequest();
        request.PhoneNumber = null;

        var result = _validator.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.PhoneNumber);
    }
}
