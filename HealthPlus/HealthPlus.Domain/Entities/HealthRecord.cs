using HealthPlus.Domain.Common;
using HealthPlus.Domain.Enums;

namespace HealthPlus.Domain.Entities;

public class HealthRecord : AuditableEntity
{
    public Guid UserId { get; set; }
    public string ProfileName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public DateOnly DateOfBirth { get; set; }
    public Gender Gender { get; set; }
    public BloodType? BloodType { get; set; }
    public string? Allergies { get; set; }
    public string? ChronicDiseases { get; set; }
    public string? InsuranceNumber { get; set; }

    public User User { get; set; } = null!;
    public ICollection<HealthMetric> HealthMetrics { get; set; } = new List<HealthMetric>();
    public ICollection<MedicalHistory> MedicalHistories { get; set; } = new List<MedicalHistory>();
    public ICollection<Vaccine> Vaccines { get; set; } = new List<Vaccine>();
}
