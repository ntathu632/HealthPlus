using HealthPlus.Domain.Common;
using HealthPlus.Domain.Enums;

namespace HealthPlus.Domain.Entities;

public class Appointment : BaseEntity
{
    public Guid DoctorId { get; set; }
    public Guid PatientId { get; set; }
    public DateTime AppointmentTime { get; set; }
    public string? Reason { get; set; }
    public AppointmentStatus Status { get; set; } = AppointmentStatus.Pending;
    public string? Notes { get; set; }

    // Snapshot phí tư vấn của bác sĩ tại thời điểm đặt lịch (0 = miễn phí).
    public decimal Fee { get; set; }
    public bool IsPaid { get; set; }

    public User Doctor { get; set; } = null!;
    public User Patient { get; set; } = null!;
}
