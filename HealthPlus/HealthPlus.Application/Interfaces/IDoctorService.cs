using HealthPlus.Application.DTOs.Doctor;
using HealthPlus.Application.DTOs.MedicalHistory;
using HealthPlus.Application.DTOs.Prescriptions;
using HealthPlus.Application.DTOs.Vaccines;

namespace HealthPlus.Application.Interfaces;

public interface IDoctorService
{
    Task<IEnumerable<PatientSummaryResponse>> GetMyPatientsAsync(Guid doctorId, CancellationToken ct = default);
    Task<PatientProfileResponse> GetPatientProfileAsync(Guid doctorId, Guid patientId, CancellationToken ct = default);

    Task<MedicalHistoryResponse> AddMedicalHistoryAsync(Guid doctorId, Guid patientId, CreateMedicalHistoryRequest request, CancellationToken ct = default);
    Task<MedicalHistoryResponse> UpdateMedicalHistoryAsync(Guid doctorId, Guid patientId, Guid medicalHistoryId, UpdateMedicalHistoryRequest request, CancellationToken ct = default);
    Task DeleteMedicalHistoryAsync(Guid doctorId, Guid patientId, Guid medicalHistoryId, CancellationToken ct = default);

    Task<VaccineResponse> AddVaccineAsync(Guid doctorId, Guid patientId, CreateVaccineRequest request, CancellationToken ct = default);
    Task<VaccineResponse> UpdateVaccineAsync(Guid doctorId, Guid patientId, Guid vaccineId, UpdateVaccineRequest request, CancellationToken ct = default);
    Task DeleteVaccineAsync(Guid doctorId, Guid patientId, Guid vaccineId, CancellationToken ct = default);

    Task<PrescriptionResponse> CreatePrescriptionAsync(Guid doctorId, Guid patientId, CreatePrescriptionRequest request, CancellationToken ct = default);
    Task DeletePrescriptionAsync(Guid doctorId, Guid patientId, Guid prescriptionId, CancellationToken ct = default);
    Task<PrescriptionItemResponse> AddPrescriptionItemAsync(Guid doctorId, Guid patientId, Guid prescriptionId, CreatePrescriptionItemRequest request, CancellationToken ct = default);
    Task<PrescriptionItemResponse> UpdatePrescriptionItemAsync(Guid doctorId, Guid patientId, Guid itemId, UpdatePrescriptionItemRequest request, CancellationToken ct = default);
    Task DeletePrescriptionItemAsync(Guid doctorId, Guid patientId, Guid itemId, CancellationToken ct = default);
}
