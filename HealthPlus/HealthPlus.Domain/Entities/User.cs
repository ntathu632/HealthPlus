using HealthPlus.Domain.Common;

namespace HealthPlus.Domain.Entities;

public class User : AuditableEntity
{
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? AvatarUrl { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsEmailVerified { get; set; } = false;
    public DateTime? LastLoginAt { get; set; }

    // Chỉ có ý nghĩa với tài khoản Bác sĩ (role Doctor) — dùng cho tính năng tư vấn trực tuyến trả phí.
    public Guid? HospitalId { get; set; }
    public string? Specialty { get; set; }
    public decimal? ConsultationFee { get; set; }

    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    public ICollection<HealthRecord> HealthRecords { get; set; } = new List<HealthRecord>();
    public UserNotificationSetting? NotificationSetting { get; set; }
    public Hospital? Hospital { get; set; }
}
