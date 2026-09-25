using FluentValidation;
using HealthPlus.Application.Common;
using HealthPlus.Application.DTOs.Contact;
using HealthPlus.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HealthPlus.Controllers;

public class ContactController : BaseApiController
{
    private readonly IContactService _contactService;
    private readonly IValidator<ContactRequest> _validator;

    public ContactController(IContactService contactService, IValidator<ContactRequest> validator)
    {
        _contactService = contactService;
        _validator = validator;
    }

    [HttpPost]
    public async Task<IActionResult> Send([FromBody] ContactRequest request, CancellationToken ct)
    {
        var validation = await _validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return BadRequest(ApiResponse<object>.Fail(validation.Errors.Select(e => e.ErrorMessage)));

        await _contactService.SendMessageAsync(request, ct);
        return Ok(ApiResponse.Ok("Đã gửi liên hệ thành công."));
    }
}
