using HealthPlus.Domain.Common;
using HealthPlus.Domain.Enums;

namespace HealthPlus.Domain.Entities;

// Thanh toán mô phỏng (demo) cho lịch hẹn tư vấn trả phí — chưa nối cổng thanh toán thật.
public class Payment : BaseEntity
{
    public Guid AppointmentId { get; set; }
    public Guid PatientId { get; set; }
    public Guid DoctorId { get; set; }
    public decimal Amount { get; set; }
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public string Method { get; set; } = "Demo";
    public DateTime? PaidAt { get; set; }

    public Appointment Appointment { get; set; } = null!;
    public User Patient { get; set; } = null!;
    public User Doctor { get; set; } = null!;
}
