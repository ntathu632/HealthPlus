namespace HealthPlus.Application.DTOs.Prescriptions;

public class CreatePrescriptionRequest
{
    public Guid? MedicalHistoryId { get; set; }
    public string? Notes { get; set; }
}

public class CreatePrescriptionItemRequest
{
    public string MedicineName { get; set; } = string.Empty;
    public string? Dosage { get; set; }
    public int? FrequencyPerDay { get; set; }
    public int? DurationDays { get; set; }
    public string? Timing { get; set; }
    public string? Instructions { get; set; }
}

public class UpdatePrescriptionItemRequest
{
    public string MedicineName { get; set; } = string.Empty;
    public string? Dosage { get; set; }
    public int? FrequencyPerDay { get; set; }
    public int? DurationDays { get; set; }
    public string? Timing { get; set; }
    public string? Instructions { get; set; }
    public bool IsConfirmed { get; set; }
}
