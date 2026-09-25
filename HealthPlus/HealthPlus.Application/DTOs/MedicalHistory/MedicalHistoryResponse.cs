namespace HealthPlus.Application.DTOs.MedicalHistory;

public class MedicalHistoryResponse
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid HealthRecordId { get; set; }
    public DateOnly VisitDate { get; set; }
    public string? DoctorName { get; set; }
    public string? Hospital { get; set; }
    public string? Specialty { get; set; }
    public string? Diagnosis { get; set; }
    public string? Treatment { get; set; }
    public DateOnly? FollowUpDate { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
