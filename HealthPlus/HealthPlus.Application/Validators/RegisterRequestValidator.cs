using FluentValidation;
using HealthPlus.Application.DTOs.Auth;

namespace HealthPlus.Application.Validators;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email không được để trống.")
            .EmailAddress().WithMessage("Email không hợp lệ.")
            .MaximumLength(256);

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Mật khẩu không được để trống.")
            .MinimumLength(8).WithMessage("Mật khẩu phải có ít nhất 8 ký tự.")
            .Matches("[A-Z]").WithMessage("Mật khẩu phải có ít nhất 1 chữ hoa.")
            .Matches("[0-9]").WithMessage("Mật khẩu phải có ít nhất 1 chữ số.");

        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Họ tên không được để trống.")
            .MaximumLength(256);

        RuleFor(x => x.PhoneNumber)
            .Matches(@"^[0-9]{10,11}$").WithMessage("Số điện thoại không hợp lệ.")
            .When(x => !string.IsNullOrEmpty(x.PhoneNumber));
    }
}
