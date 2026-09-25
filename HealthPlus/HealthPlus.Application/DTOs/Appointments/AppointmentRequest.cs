using HealthPlus.Domain.Enums;

namespace HealthPlus.Application.DTOs.Appointments;

public class CreateAppointmentRequest
{
    public Guid DoctorId { get; set; }
    public DateTime AppointmentTime { get; set; }
    public string? Reason { get; set; }
}

public class UpdateAppointmentStatusRequest
{
    public AppointmentStatus Status { get; set; }
    public string? Notes { get; set; }
}
