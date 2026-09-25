using HealthPlus.Domain.Enums;

namespace HealthPlus.Application.DTOs.Appointments;

public class AppointmentResponse
{
    public Guid Id { get; set; }
    public Guid DoctorId { get; set; }
    public string DoctorName { get; set; } = string.Empty;
    public Guid PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public DateTime AppointmentTime { get; set; }
    public string? Reason { get; set; }
    public AppointmentStatus Status { get; set; }
    public string? Notes { get; set; }
    public decimal Fee { get; set; }
    public bool IsPaid { get; set; }
    public DateTime CreatedAt { get; set; }
    public string VideoRoomUrl { get; set; } = string.Empty;
}

public class DoctorListItemResponse
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? AvatarUrl { get; set; }
    public string? Specialty { get; set; }
    public string? HospitalName { get; set; }
    public decimal ConsultationFee { get; set; }
}
