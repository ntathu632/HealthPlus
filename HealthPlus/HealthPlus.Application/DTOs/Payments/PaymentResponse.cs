using HealthPlus.Domain.Enums;

namespace HealthPlus.Application.DTOs.Payments;

public class PaymentResponse
{
    public Guid Id { get; set; }
    public Guid AppointmentId { get; set; }
    public string DoctorName { get; set; } = string.Empty;
    public string PatientName { get; set; } = string.Empty;
    public DateTime AppointmentTime { get; set; }
    public decimal Amount { get; set; }
    public PaymentStatus Status { get; set; }
    public string Method { get; set; } = string.Empty;
    public DateTime? PaidAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
