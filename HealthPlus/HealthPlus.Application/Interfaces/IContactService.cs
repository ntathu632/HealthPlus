using HealthPlus.Application.DTOs.Contact;

namespace HealthPlus.Application.Interfaces;

public interface IContactService
{
    Task SendMessageAsync(ContactRequest request, CancellationToken ct = default);
}
