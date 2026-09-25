using FluentValidation;
using HealthPlus.Application.DTOs.Auth;

namespace HealthPlus.Application.Validators;

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        // Không bắt buộc đúng định dạng email ở đây (chỉ RegisterRequestValidator mới bắt buộc) —
        // để hỗ trợ đăng nhập bằng tên đăng nhập đơn giản (vd. tài khoản demo "admin"/"bacsi"/"benhnhan").
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email không được để trống.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Mật khẩu không được để trống.");
    }
}
