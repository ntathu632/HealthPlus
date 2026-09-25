namespace HealthPlus.Domain.Entities;

public class VaccineScheduleTemplate
{
    public int Id { get; set; }
    public string VaccineName { get; set; } = string.Empty;
    public int DoseNumber { get; set; }
    public int? IntervalDays { get; set; }
    public string? RecommendedAge { get; set; }
    public string? Notes { get; set; }
}
