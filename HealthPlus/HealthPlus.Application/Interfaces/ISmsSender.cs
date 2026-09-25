namespace HealthPlus.Application.Interfaces;

public interface ISmsSender
{
    Task SendAsync(string phoneNumber, string message, CancellationToken ct = default);
}
