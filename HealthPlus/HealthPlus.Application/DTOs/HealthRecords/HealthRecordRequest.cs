using HealthPlus.Domain.Enums;

namespace HealthPlus.Application.DTOs.HealthRecords;

public class CreateHealthRecordRequest
{
    public string ProfileName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public DateOnly DateOfBirth { get; set; }
    public Gender Gender { get; set; }
    public BloodType? BloodType { get; set; }
    public string? Allergies { get; set; }
    public string? ChronicDiseases { get; set; }
    public string? InsuranceNumber { get; set; }
}

public class UpdateHealthRecordRequest
{
    public string ProfileName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public DateOnly DateOfBirth { get; set; }
    public Gender Gender { get; set; }
    public BloodType? BloodType { get; set; }
    public string? Allergies { get; set; }
    public string? ChronicDiseases { get; set; }
    public string? InsuranceNumber { get; set; }
}
