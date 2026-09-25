namespace HealthPlus.Application.DTOs.HealthRecords;

public class HealthMetricResponse
{
    public Guid Id { get; set; }
    public Guid HealthRecordId { get; set; }
    public DateTime MeasuredAt { get; set; }
    public decimal? HeightCm { get; set; }
    public decimal? WeightKg { get; set; }
    public decimal? Bmi { get; set; }
    public int? SystolicBp { get; set; }
    public int? DiastolicBp { get; set; }
    public int? HeartRate { get; set; }
    public decimal? BloodSugar { get; set; }
    public decimal? Temperature { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}
