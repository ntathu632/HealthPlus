namespace HealthPlus.Application.DTOs.Hospitals;

public class HospitalResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
}
