using HealthPlus.Application.DTOs.Appointments;
using HealthPlus.Domain.Enums;

namespace HealthPlus.Application.Interfaces;

public interface IAppointmentService
{
    Task<IEnumerable<DoctorListItemResponse>> GetActiveDoctorsAsync(CancellationToken ct = default);
    Task<AppointmentResponse> CreateAsync(Guid patientId, CreateAppointmentRequest request, CancellationToken ct = default);
    Task<IEnumerable<AppointmentResponse>> GetMyAppointmentsAsPatientAsync(Guid patientId, AppointmentStatus? status, CancellationToken ct = default);
    Task<IEnumerable<AppointmentResponse>> GetMyAppointmentsAsDoctorAsync(Guid doctorId, AppointmentStatus? status, CancellationToken ct = default);
    Task CancelAsync(Guid patientId, Guid appointmentId, CancellationToken ct = default);
    Task<AppointmentResponse> UpdateStatusAsync(Guid doctorId, Guid appointmentId, UpdateAppointmentStatusRequest request, CancellationToken ct = default);
}
