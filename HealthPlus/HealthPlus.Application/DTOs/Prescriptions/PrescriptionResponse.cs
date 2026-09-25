using HealthPlus.Domain.Enums;

namespace HealthPlus.Application.DTOs.Prescriptions;

public class PrescriptionResponse
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid? MedicalHistoryId { get; set; }
    public string? ImageUrl { get; set; }
    public string? OcrRawText { get; set; }
    public decimal? OcrConfidenceScore { get; set; }
    public PrescriptionStatus Status { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<PrescriptionItemResponse> Items { get; set; } = [];
}

public class PrescriptionItemResponse
{
    public Guid Id { get; set; }
    public Guid PrescriptionId { get; set; }
    public string MedicineName { get; set; } = string.Empty;
    public string? Dosage { get; set; }
    public int? FrequencyPerDay { get; set; }
    public int? DurationDays { get; set; }
    public string? Timing { get; set; }
    public string? Instructions { get; set; }
    public bool IsConfirmed { get; set; }
}
