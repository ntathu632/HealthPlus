namespace HealthPlus.Application.DTOs.Admin;

public class UpdateUserRoleRequest
{
    public int RoleId { get; set; }
}

public class UpdateUserStatusRequest
{
    public bool IsActive { get; set; }
}

public class CreateDoctorRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    // 2 = Bác sĩ, 3 = Bệnh nhân (không cho tạo Admin qua đây để tránh lạm quyền ngoài ý muốn)
    public int RoleId { get; set; } = 2;
    // Chỉ áp dụng khi RoleId = 2 (Bác sĩ) — dùng cho tính năng tư vấn trực tuyến trả phí.
    public Guid? HospitalId { get; set; }
    public string? Specialty { get; set; }
    public decimal? ConsultationFee { get; set; }
}

public class ResetPasswordRequest
{
    public string NewPassword { get; set; } = string.Empty;
}

public class UpdateRolePermissionsRequest
{
    public List<int> PermissionIds { get; set; } = [];
}

public class AssignPatientRequest
{
    public Guid DoctorId { get; set; }
    public Guid PatientId { get; set; }
}

public class UpdateSystemSettingRequest
{
    public string? SettingValue { get; set; }
}
