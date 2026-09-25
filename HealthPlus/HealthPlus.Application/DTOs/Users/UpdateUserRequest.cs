namespace HealthPlus.Application.DTOs.Users;

public class UpdateUserRequest
{
    public string FullName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }

    // Chỉ có ý nghĩa với tài khoản vai trò Bác sĩ — bỏ trống với các vai trò khác.
    public Guid? HospitalId { get; set; }
    public string? Specialty { get; set; }
    public decimal? ConsultationFee { get; set; }
}
