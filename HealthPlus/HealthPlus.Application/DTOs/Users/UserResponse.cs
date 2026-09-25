namespace HealthPlus.Application.DTOs.Users;

public class UserResponse
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? AvatarUrl { get; set; }
    public bool IsEmailVerified { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public IEnumerable<string> Roles { get; set; } = [];

    // Chỉ có giá trị với tài khoản vai trò Bác sĩ.
    public Guid? HospitalId { get; set; }
    public string? HospitalName { get; set; }
    public string? Specialty { get; set; }
    public decimal? ConsultationFee { get; set; }
}
