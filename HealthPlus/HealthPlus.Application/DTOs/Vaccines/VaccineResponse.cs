using HealthPlus.Domain.Enums;

namespace HealthPlus.Application.DTOs.Vaccines;

public class VaccineResponse
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid HealthRecordId { get; set; }
    public string VaccineName { get; set; } = string.Empty;
    public string? Manufacturer { get; set; }
    public string? LotNumber { get; set; }
    public int DoseNumber { get; set; }
    public DateOnly InjectionDate { get; set; }
    public DateOnly? NextDueDate { get; set; }
    public string? Location { get; set; }
    public string? AdministeredBy { get; set; }
    public string? SideEffects { get; set; }
    public VaccineStatus Status { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class VaccineScheduleTemplateResponse
{
    public int Id { get; set; }
    public string VaccineName { get; set; } = string.Empty;
    public int DoseNumber { get; set; }
    public int? IntervalDays { get; set; }
    public string? RecommendedAge { get; set; }
    public string? Notes { get; set; }
}
