using FluentValidation;
using HealthPlus.Application.DTOs.Users;

namespace HealthPlus.Application.Validators;

public class ChangePasswordRequestValidator : AbstractValidator<ChangePasswordRequest>
{
    public ChangePasswordRequestValidator()
    {
        RuleFor(x => x.CurrentPassword)
            .NotEmpty().WithMessage("Vui lòng nhập mật khẩu hiện tại.");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("Mật khẩu mới không được để trống.")
            .MinimumLength(8).WithMessage("Mật khẩu mới phải có ít nhất 8 ký tự.")
            .Matches("[A-Z]").WithMessage("Mật khẩu mới phải có ít nhất 1 chữ hoa.")
            .Matches("[0-9]").WithMessage("Mật khẩu mới phải có ít nhất 1 chữ số.")
            .NotEqual(x => x.CurrentPassword).WithMessage("Mật khẩu mới phải khác mật khẩu hiện tại.");
    }
}
