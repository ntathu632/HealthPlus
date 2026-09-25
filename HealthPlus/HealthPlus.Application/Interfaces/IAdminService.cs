using HealthPlus.Application.Common;
using HealthPlus.Application.DTOs.Admin;
using HealthPlus.Application.DTOs.Appointments;
using HealthPlus.Domain.Enums;

namespace HealthPlus.Application.Interfaces;

public interface IAdminService
{
    Task<PagedResult<AdminUserResponse>> GetUsersAsync(int page, int pageSize, string? search, int? roleId, CancellationToken ct = default);
    Task<AdminUserResponse> GetUserByIdAsync(Guid userId, CancellationToken ct = default);
    Task<AdminUserResponse> UpdateUserRoleAsync(Guid userId, UpdateUserRoleRequest request, CancellationToken ct = default);
    Task<AdminUserResponse> UpdateUserStatusAsync(Guid userId, UpdateUserStatusRequest request, CancellationToken ct = default);
    Task<AdminUserResponse> CreateDoctorAsync(CreateDoctorRequest request, CancellationToken ct = default);
    Task ResetPasswordAsync(Guid userId, ResetPasswordRequest request, CancellationToken ct = default);

    Task<IEnumerable<RoleResponse>> GetRolesAsync(CancellationToken ct = default);
    Task<IEnumerable<PermissionResponse>> GetPermissionsAsync(CancellationToken ct = default);
    Task<IEnumerable<int>> GetRolePermissionIdsAsync(int roleId, CancellationToken ct = default);
    Task UpdateRolePermissionsAsync(int roleId, UpdateRolePermissionsRequest request, CancellationToken ct = default);

    Task<IEnumerable<DoctorPatientResponse>> GetDoctorPatientsAsync(Guid? doctorId, CancellationToken ct = default);
    Task<DoctorPatientResponse> AssignPatientAsync(AssignPatientRequest request, Guid actorId, CancellationToken ct = default);
    Task UnassignPatientAsync(Guid assignmentId, CancellationToken ct = default);

    Task<PagedResult<AuditLogResponse>> GetAuditLogsAsync(int page, int pageSize, Guid? userId, string? entity, CancellationToken ct = default);

    Task<IEnumerable<SystemSettingResponse>> GetSystemSettingsAsync(CancellationToken ct = default);
    Task<SystemSettingResponse> UpdateSystemSettingAsync(string key, UpdateSystemSettingRequest request, CancellationToken ct = default);

    Task<AdminDashboardResponse> GetDashboardStatsAsync(CancellationToken ct = default);

    Task<PagedResult<AppointmentResponse>> GetAllAppointmentsAsync(int page, int pageSize, AppointmentStatus? status, CancellationToken ct = default);
}
