using HealthPlus.Application.DTOs.HealthRecords;
using HealthPlus.Application.DTOs.MedicalHistory;
using HealthPlus.Application.DTOs.Prescriptions;
using HealthPlus.Application.DTOs.Vaccines;

namespace HealthPlus.Application.DTOs.Doctor;

public class PatientSummaryResponse
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? AvatarUrl { get; set; }
    public DateTime AssignedAt { get; set; }
}

public class PatientProfileResponse
{
    public PatientSummaryResponse Patient { get; set; } = null!;
    public IEnumerable<HealthRecordResponse> HealthRecords { get; set; } = [];
    public IEnumerable<MedicalHistoryResponse> MedicalHistories { get; set; } = [];
    public IEnumerable<VaccineResponse> Vaccines { get; set; } = [];
    public IEnumerable<PrescriptionResponse> Prescriptions { get; set; } = [];
}
