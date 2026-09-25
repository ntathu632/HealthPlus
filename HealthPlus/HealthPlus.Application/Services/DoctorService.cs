using HealthPlus.Application.DTOs.Doctor;
using HealthPlus.Application.DTOs.MedicalHistory;
using HealthPlus.Application.DTOs.Prescriptions;
using HealthPlus.Application.DTOs.Vaccines;
using HealthPlus.Application.Interfaces;
using HealthPlus.Domain.Interfaces.Repositories;

namespace HealthPlus.Application.Services;

public class DoctorService : IDoctorService
{
    private readonly IUnitOfWork _uow;
    private readonly IHealthRecordService _healthRecords;
    private readonly IMedicalHistoryService _medicalHistories;
    private readonly IVaccineService _vaccines;
    private readonly IPrescriptionService _prescriptions;

    public DoctorService(
        IUnitOfWork uow,
        IHealthRecordService healthRecords,
        IMedicalHistoryService medicalHistories,
        IVaccineService vaccines,
        IPrescriptionService prescriptions)
    {
        _uow = uow;
        _healthRecords = healthRecords;
        _medicalHistories = medicalHistories;
        _vaccines = vaccines;
        _prescriptions = prescriptions;
    }

    public async Task<IEnumerable<PatientSummaryResponse>> GetMyPatientsAsync(Guid doctorId, CancellationToken ct = default)
    {
        var assignments = await _uow.DoctorPatients.FindAsync(dp => dp.DoctorId == doctorId && dp.IsActive, ct);
        var ordered = assignments.OrderByDescending(a => a.CreatedAt).ToList();

        var patientIds = ordered.Select(a => a.PatientId).ToList();
        var patients = (await _uow.Users.FindAsync(u => patientIds.Contains(u.Id), ct)).ToDictionary(u => u.Id);

        return ordered
            .Where(a => patients.ContainsKey(a.PatientId))
            .Select(a =>
            {
                var p = patients[a.PatientId];
                return new PatientSummaryResponse
                {
                    Id = p.Id,
                    Email = p.Email,
                    FullName = p.FullName,
                    PhoneNumber = p.PhoneNumber,
                    AvatarUrl = p.AvatarUrl,
                    AssignedAt = a.CreatedAt,
                };
            });
    }

    public async Task<PatientProfileResponse> GetPatientProfileAsync(Guid doctorId, Guid patientId, CancellationToken ct = default)
    {
        var assignment = await EnsureAssignedAsync(doctorId, patientId, ct);
        var patient = await _uow.Users.GetByIdAsync(patientId, ct)
            ?? throw new KeyNotFoundException("Không tìm thấy bệnh nhân.");

        var healthRecords = await _healthRecords.GetAllByUserAsync(patientId, ct);
        var medicalHistories = await _medicalHistories.GetAllAsync(patientId, null, 1, 100, ct);
        var vaccines = await _vaccines.GetAllAsync(patientId, null, 1, 100, ct);
        var prescriptions = await _prescriptions.GetAllAsync(patientId, ct);

        return new PatientProfileResponse
        {
            Patient = new PatientSummaryResponse
            {
                Id = patient.Id,
                Email = patient.Email,
                FullName = patient.FullName,
                PhoneNumber = patient.PhoneNumber,
                AvatarUrl = patient.AvatarUrl,
                AssignedAt = assignment.CreatedAt,
            },
            HealthRecords = healthRecords,
            MedicalHistories = medicalHistories.Items,
            Vaccines = vaccines.Items,
            Prescriptions = prescriptions,
        };
    }

    public async Task<MedicalHistoryResponse> AddMedicalHistoryAsync(Guid doctorId, Guid patientId, CreateMedicalHistoryRequest request, CancellationToken ct = default)
    {
        await EnsureAssignedAsync(doctorId, patientId, ct);
        return await _medicalHistories.CreateAsync(patientId, request, ct);
    }

    public async Task<MedicalHistoryResponse> UpdateMedicalHistoryAsync(Guid doctorId, Guid patientId, Guid medicalHistoryId, UpdateMedicalHistoryRequest request, CancellationToken ct = default)
    {
        await EnsureAssignedAsync(doctorId, patientId, ct);
        return await _medicalHistories.UpdateAsync(medicalHistoryId, patientId, request, ct);
    }

    public async Task DeleteMedicalHistoryAsync(Guid doctorId, Guid patientId, Guid medicalHistoryId, CancellationToken ct = default)
    {
        await EnsureAssignedAsync(doctorId, patientId, ct);
        await _medicalHistories.DeleteAsync(medicalHistoryId, patientId, ct);
    }

    public async Task<VaccineResponse> AddVaccineAsync(Guid doctorId, Guid patientId, CreateVaccineRequest request, CancellationToken ct = default)
    {
        await EnsureAssignedAsync(doctorId, patientId, ct);
        return await _vaccines.CreateAsync(patientId, request, ct);
    }

    public async Task<VaccineResponse> UpdateVaccineAsync(Guid doctorId, Guid patientId, Guid vaccineId, UpdateVaccineRequest request, CancellationToken ct = default)
    {
        await EnsureAssignedAsync(doctorId, patientId, ct);
        return await _vaccines.UpdateAsync(vaccineId, patientId, request, ct);
    }

    public async Task DeleteVaccineAsync(Guid doctorId, Guid patientId, Guid vaccineId, CancellationToken ct = default)
    {
        await EnsureAssignedAsync(doctorId, patientId, ct);
        await _vaccines.DeleteAsync(vaccineId, patientId, ct);
    }

    public async Task<PrescriptionResponse> CreatePrescriptionAsync(Guid doctorId, Guid patientId, CreatePrescriptionRequest request, CancellationToken ct = default)
    {
        await EnsureAssignedAsync(doctorId, patientId, ct);
        return await _prescriptions.CreateAsync(patientId, request, ct);
    }

    public async Task DeletePrescriptionAsync(Guid doctorId, Guid patientId, Guid prescriptionId, CancellationToken ct = default)
    {
        await EnsureAssignedAsync(doctorId, patientId, ct);
        await _prescriptions.DeleteAsync(prescriptionId, patientId, ct);
    }

    public async Task<PrescriptionItemResponse> AddPrescriptionItemAsync(Guid doctorId, Guid patientId, Guid prescriptionId, CreatePrescriptionItemRequest request, CancellationToken ct = default)
    {
        await EnsureAssignedAsync(doctorId, patientId, ct);
        return await _prescriptions.AddItemAsync(prescriptionId, patientId, request, ct);
    }

    public async Task<PrescriptionItemResponse> UpdatePrescriptionItemAsync(Guid doctorId, Guid patientId, Guid itemId, UpdatePrescriptionItemRequest request, CancellationToken ct = default)
    {
        await EnsureAssignedAsync(doctorId, patientId, ct);
        return await _prescriptions.UpdateItemAsync(itemId, patientId, request, ct);
    }

    public async Task DeletePrescriptionItemAsync(Guid doctorId, Guid patientId, Guid itemId, CancellationToken ct = default)
    {
        await EnsureAssignedAsync(doctorId, patientId, ct);
        await _prescriptions.DeleteItemAsync(itemId, patientId, ct);
    }

    private async Task<Domain.Entities.DoctorPatient> EnsureAssignedAsync(Guid doctorId, Guid patientId, CancellationToken ct)
    {
        var assignment = await _uow.DoctorPatients.FirstOrDefaultAsync(
            dp => dp.DoctorId == doctorId && dp.PatientId == patientId && dp.IsActive, ct);

        return assignment ?? throw new UnauthorizedAccessException("Bệnh nhân này chưa được gán cho bạn.");
    }
}
