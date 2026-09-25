using HealthPlus.Domain.Common;
using HealthPlus.Domain.Enums;

namespace HealthPlus.Domain.Entities;

public class Vaccine : AuditableEntity
{
    public Guid UserId { get; set; }
    public Guid HealthRecordId { get; set; }
    public string VaccineName { get; set; } = string.Empty;
    public string? Manufacturer { get; set; }
    public string? LotNumber { get; set; }
    public int DoseNumber { get; set; } = 1;
    public DateOnly InjectionDate { get; set; }
    public DateOnly? NextDueDate { get; set; }
    public string? Location { get; set; }
    public string? AdministeredBy { get; set; }
    public string? SideEffects { get; set; }
    public VaccineStatus Status { get; set; } = VaccineStatus.Completed;
    public string? Notes { get; set; }

    public User User { get; set; } = null!;
    public HealthRecord HealthRecord { get; set; } = null!;
}
