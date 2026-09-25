using HealthPlus.Domain.Common;

namespace HealthPlus.Domain.Entities;

public class MedicalHistory : AuditableEntity
{
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

    public User User { get; set; } = null!;
    public HealthRecord HealthRecord { get; set; } = null!;
    public ICollection<MedicalDocument> Documents { get; set; } = new List<MedicalDocument>();
    public ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();
}
