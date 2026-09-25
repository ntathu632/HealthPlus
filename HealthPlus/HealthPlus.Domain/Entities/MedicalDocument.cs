using HealthPlus.Domain.Common;

namespace HealthPlus.Domain.Entities;

public class MedicalDocument : BaseEntity
{
    public Guid MedicalHistoryId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string StorageKey { get; set; } = string.Empty;
    public string? FileUrl { get; set; }
    public string FileType { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; } = 0;
    public string? DocumentType { get; set; }
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    public MedicalHistory MedicalHistory { get; set; } = null!;
}
