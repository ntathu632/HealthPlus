using HealthPlus.Application.DTOs.Appointments;
using HealthPlus.Application.Interfaces;
using HealthPlus.Domain.Entities;
using HealthPlus.Domain.Enums;
using HealthPlus.Domain.Interfaces.Repositories;

namespace HealthPlus.Application.Services;

public class AppointmentService : IAppointmentService
{
    private readonly IUnitOfWork _uow;
    public AppointmentService(IUnitOfWork uow) => _uow = uow;

    public async Task<IEnumerable<DoctorListItemResponse>> GetActiveDoctorsAsync(CancellationToken ct = default)
    {
        var doctorUserIds = (await _uow.UserRoles.FindAsync(ur => ur.RoleId == 2, ct))
            .Select(ur => ur.UserId).ToHashSet();

        var doctors = (await _uow.Users.FindAsync(u => u.IsActive && doctorUserIds.Contains(u.Id), ct)).ToList();

        var hospitalIds = doctors.Where(d => d.HospitalId.HasValue).Select(d => d.HospitalId!.Value).Distinct().ToList();
        var hospitals = (await _uow.Hospitals.FindAsync(h => hospitalIds.Contains(h.Id), ct)).ToDictionary(h => h.Id);

        return doctors.OrderBy(u => u.FullName).Select(u => new DoctorListItemResponse
        {
            Id = u.Id,
            FullName = u.FullName,
            Email = u.Email,
            PhoneNumber = u.PhoneNumber,
            AvatarUrl = u.AvatarUrl,
            Specialty = u.Specialty,
            HospitalName = u.HospitalId.HasValue && hospitals.TryGetValue(u.HospitalId.Value, out var h) ? h.Name : null,
            ConsultationFee = u.ConsultationFee ?? 0,
        });
    }

    public async Task<AppointmentResponse> CreateAsync(Guid patientId, CreateAppointmentRequest request, CancellationToken ct = default)
    {
        var doctor = await _uow.Users.GetByIdAsync(request.DoctorId, ct)
            ?? throw new KeyNotFoundException("Không tìm thấy bác sĩ.");
        if (!doctor.IsActive)
            throw new InvalidOperationException("Bác sĩ này hiện không hoạt động.");

        var isDoctor = (await _uow.UserRoles.AnyAsync(ur => ur.UserId == request.DoctorId && ur.RoleId == 2, ct));
        if (!isDoctor)
            throw new InvalidOperationException("Người dùng này không phải là bác sĩ.");

        var patient = await _uow.Users.GetByIdAsync(patientId, ct)
            ?? throw new KeyNotFoundException("Không tìm thấy bệnh nhân.");

        if (request.AppointmentTime <= DateTime.UtcNow)
            throw new InvalidOperationException("Không thể đặt lịch hẹn trong quá khứ.");

        var doctorBusy = await _uow.Appointments.AnyAsync(a =>
            a.DoctorId == request.DoctorId &&
            a.AppointmentTime == request.AppointmentTime &&
            a.Status != AppointmentStatus.Cancelled, ct);
        if (doctorBusy)
            throw new InvalidOperationException("Bác sĩ đã có lịch hẹn khác vào thời điểm này, vui lòng chọn giờ khác.");

        var fee = doctor.ConsultationFee ?? 0;
        var appointment = new Appointment
        {
            Id = Guid.NewGuid(),
            DoctorId = request.DoctorId,
            PatientId = patientId,
            AppointmentTime = request.AppointmentTime,
            Reason = request.Reason?.Trim(),
            Status = AppointmentStatus.Pending,
            Fee = fee,
            IsPaid = fee <= 0,
        };
        await _uow.Appointments.AddAsync(appointment, ct);
        await _uow.SaveChangesAsync(ct);

        return Map(appointment, doctor.FullName, patient.FullName);
    }

    public async Task<IEnumerable<AppointmentResponse>> GetMyAppointmentsAsPatientAsync(Guid patientId, AppointmentStatus? status, CancellationToken ct = default)
    {
        var appointments = await _uow.Appointments.FindAsync(
            a => a.PatientId == patientId && (status == null || a.Status == status), ct);
        return await MapWithNamesAsync(appointments.OrderByDescending(a => a.AppointmentTime), ct);
    }

    public async Task<IEnumerable<AppointmentResponse>> GetMyAppointmentsAsDoctorAsync(Guid doctorId, AppointmentStatus? status, CancellationToken ct = default)
    {
        var appointments = await _uow.Appointments.FindAsync(
            a => a.DoctorId == doctorId && (status == null || a.Status == status), ct);
        return await MapWithNamesAsync(appointments.OrderBy(a => a.AppointmentTime), ct);
    }

    public async Task CancelAsync(Guid patientId, Guid appointmentId, CancellationToken ct = default)
    {
        var appointment = await _uow.Appointments.GetByIdAsync(appointmentId, ct)
            ?? throw new KeyNotFoundException("Không tìm thấy lịch hẹn.");
        if (appointment.PatientId != patientId)
            throw new UnauthorizedAccessException("Bạn không có quyền huỷ lịch hẹn này.");
        if (appointment.Status is AppointmentStatus.Completed or AppointmentStatus.Cancelled)
            throw new InvalidOperationException("Lịch hẹn này không thể huỷ.");

        appointment.Status = AppointmentStatus.Cancelled;
        _uow.Appointments.Update(appointment);
        await _uow.SaveChangesAsync(ct);
    }

    public async Task<AppointmentResponse> UpdateStatusAsync(Guid doctorId, Guid appointmentId, UpdateAppointmentStatusRequest request, CancellationToken ct = default)
    {
        var appointment = await _uow.Appointments.GetByIdAsync(appointmentId, ct)
            ?? throw new KeyNotFoundException("Không tìm thấy lịch hẹn.");
        if (appointment.DoctorId != doctorId)
            throw new UnauthorizedAccessException("Bạn không có quyền cập nhật lịch hẹn này.");

        appointment.Status = request.Status;
        appointment.Notes = request.Notes?.Trim();
        _uow.Appointments.Update(appointment);
        await _uow.SaveChangesAsync(ct);

        var doctor = await _uow.Users.GetByIdAsync(appointment.DoctorId, ct);
        var patient = await _uow.Users.GetByIdAsync(appointment.PatientId, ct);
        return Map(appointment, doctor?.FullName ?? "(đã xoá)", patient?.FullName ?? "(đã xoá)");
    }

    private async Task<IEnumerable<AppointmentResponse>> MapWithNamesAsync(IEnumerable<Appointment> appointments, CancellationToken ct)
    {
        var list = appointments.ToList();
        var userIds = list.SelectMany(a => new[] { a.DoctorId, a.PatientId }).Distinct().ToList();
        var users = (await _uow.Users.FindAsync(u => userIds.Contains(u.Id), ct)).ToDictionary(u => u.Id);

        return list.Select(a => Map(
            a,
            users.TryGetValue(a.DoctorId, out var doc) ? doc.FullName : "(đã xoá)",
            users.TryGetValue(a.PatientId, out var pat) ? pat.FullName : "(đã xoá)"));
    }

    private static AppointmentResponse Map(Appointment a, string doctorName, string patientName) => new()
    {
        Id = a.Id,
        DoctorId = a.DoctorId,
        DoctorName = doctorName,
        PatientId = a.PatientId,
        PatientName = patientName,
        AppointmentTime = a.AppointmentTime,
        Reason = a.Reason,
        Status = a.Status,
        Notes = a.Notes,
        Fee = a.Fee,
        IsPaid = a.IsPaid,
        CreatedAt = a.CreatedAt,
        // Phòng Jitsi Meet công khai, miễn phí, không cần đăng ký/API key — tên phòng suy ra
        // trực tiếp từ Id lịch hẹn nên duy nhất và ổn định, không cần lưu thêm cột nào mới.
        VideoRoomUrl = $"https://meet.jit.si/healthplus-{a.Id:N}",
    };
}
