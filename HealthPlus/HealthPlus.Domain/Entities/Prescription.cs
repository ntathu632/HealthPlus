using HealthPlus.Domain.Common;
using HealthPlus.Domain.Enums;

namespace HealthPlus.Domain.Entities;

public class Prescription : BaseEntity
{
    public Guid? MedicalHistoryId { get; set; }
    public Guid UserId { get; set; }
    public string? ImageStorageKey { get; set; }
    public string? ImageUrl { get; set; }
    public string? OcrRawText { get; set; }
    public decimal? OcrConfidenceScore { get; set; }
    public PrescriptionStatus Status { get; set; } = PrescriptionStatus.Pending;
    public DateTime? ProcessedAt { get; set; }

    public User User { get; set; } = null!;
    public MedicalHistory? MedicalHistory { get; set; }
    public ICollection<PrescriptionItem> Items { get; set; } = new List<PrescriptionItem>();
}
