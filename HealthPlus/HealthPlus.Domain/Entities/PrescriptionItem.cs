using HealthPlus.Domain.Common;

namespace HealthPlus.Domain.Entities;

public class PrescriptionItem : BaseEntity
{
    public Guid PrescriptionId { get; set; }
    public string MedicineName { get; set; } = string.Empty;
    public string? Dosage { get; set; }
    public int? FrequencyPerDay { get; set; }
    public int? DurationDays { get; set; }
    public string? Timing { get; set; }
    public string? Instructions { get; set; }
    public bool IsConfirmed { get; set; } = false;

    public Prescription Prescription { get; set; } = null!;
}
