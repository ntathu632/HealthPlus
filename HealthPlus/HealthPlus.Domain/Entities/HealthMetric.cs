using HealthPlus.Domain.Common;

namespace HealthPlus.Domain.Entities;

public class HealthMetric : BaseEntity
{
    public Guid HealthRecordId { get; set; }
    public DateTime MeasuredAt { get; set; } = DateTime.UtcNow;
    public decimal? HeightCm { get; set; }
    public decimal? WeightKg { get; set; }
    public decimal? Bmi { get; set; }
    public int? SystolicBp { get; set; }
    public int? DiastolicBp { get; set; }
    public int? HeartRate { get; set; }
    public decimal? BloodSugar { get; set; }
    public decimal? Temperature { get; set; }
    public string? Notes { get; set; }

    public HealthRecord HealthRecord { get; set; } = null!;
}
