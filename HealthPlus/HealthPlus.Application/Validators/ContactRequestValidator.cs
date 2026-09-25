using FluentValidation;
using HealthPlus.Application.DTOs.Contact;

namespace HealthPlus.Application.Validators;

public class ContactRequestValidator : AbstractValidator<ContactRequest>
{
    public ContactRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Vui lòng nhập họ tên.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Vui lòng nhập email.")
            .EmailAddress().WithMessage("Email không hợp lệ.");

        RuleFor(x => x.Message)
            .NotEmpty().WithMessage("Vui lòng nhập nội dung liên hệ.");
    }
}
