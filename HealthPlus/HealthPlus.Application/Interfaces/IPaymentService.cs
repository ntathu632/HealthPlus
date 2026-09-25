using HealthPlus.Application.DTOs.Payments;

namespace HealthPlus.Application.Interfaces;

public interface IPaymentService
{
    Task<PaymentResponse> SimulatePayAsync(Guid patientId, Guid appointmentId, CancellationToken ct = default);
    Task<IEnumerable<PaymentResponse>> GetMyPaymentsAsync(Guid patientId, CancellationToken ct = default);
    Task<IEnumerable<PaymentResponse>> GetMyEarningsAsync(Guid doctorId, CancellationToken ct = default);
}
