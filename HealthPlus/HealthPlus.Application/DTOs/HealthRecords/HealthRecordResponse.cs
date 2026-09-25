using HealthPlus.Domain.Enums;

namespace HealthPlus.Application.DTOs.HealthRecords;

public class HealthRecordResponse
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string ProfileName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public DateOnly DateOfBirth { get; set; }
    public Gender Gender { get; set; }
    public BloodType? BloodType { get; set; }
    public string? Allergies { get; set; }
    public string? ChronicDiseases { get; set; }
    public string? InsuranceNumber { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public HealthMetricResponse? LatestMetric { get; set; }
}
