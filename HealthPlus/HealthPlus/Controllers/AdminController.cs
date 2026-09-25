using FluentValidation;
using HealthPlus.Application.Common;
using HealthPlus.Application.DTOs.Admin;
using HealthPlus.Application.DTOs.Appointments;
using HealthPlus.Application.Interfaces;
using HealthPlus.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthPlus.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : BaseApiController
{
    private readonly IAdminService _admin;
    private readonly IValidator<CreateDoctorRequest> _createDoctorValidator;
    private readonly IValidator<ResetPasswordRequest> _resetPasswordValidator;

    public AdminController(
        IAdminService admin,
        IValidator<CreateDoctorRequest> createDoctorValidator,
        IValidator<ResetPasswordRequest> resetPasswordValidator)
    {
        _admin = admin;
        _createDoctorValidator = createDoctorValidator;
        _resetPasswordValidator = resetPasswordValidator;
    }

    // ─── Dashboard ────────────────────────────────────────────────────────────

    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard(CancellationToken ct)
    {
        var result = await _admin.GetDashboardStatsAsync(ct);
        return Ok(ApiResponse<AdminDashboardResponse>.Ok(result));
    }

    // ─── Users ────────────────────────────────────────────────────────────────

    [HttpGet("users")]
    public async Task<IActionResult> GetUsers([FromQuery] int page = 1, [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null, [FromQuery] int? roleId = null, CancellationToken ct = default)
    {
        var result = await _admin.GetUsersAsync(page, pageSize, search, roleId, ct);
        return Ok(ApiResponse<PagedResult<AdminUserResponse>>.Ok(result));
    }

    [HttpGet("users/{id:guid}")]
    public async Task<IActionResult> GetUserById(Guid id, CancellationToken ct)
    {
        try
        {
            var result = await _admin.GetUserByIdAsync(id, ct);
            return Ok(ApiResponse<AdminUserResponse>.Ok(result));
        }
        catch (KeyNotFoundException ex) { return NotFound(ApiResponse.Fail(ex.Message)); }
    }

    [HttpPut("users/{id:guid}/role")]
    public async Task<IActionResult> UpdateUserRole(Guid id, [FromBody] UpdateUserRoleRequest request, CancellationToken ct)
    {
        try
        {
            var result = await _admin.UpdateUserRoleAsync(id, request, ct);
            return Ok(ApiResponse<AdminUserResponse>.Ok(result, "Cập nhật role thành công."));
        }
        catch (KeyNotFoundException ex) { return NotFound(ApiResponse.Fail(ex.Message)); }
    }

    [HttpPut("users/{id:guid}/status")]
    public async Task<IActionResult> UpdateUserStatus(Guid id, [FromBody] UpdateUserStatusRequest request, CancellationToken ct)
    {
        try
        {
            var result = await _admin.UpdateUserStatusAsync(id, request, ct);
            return Ok(ApiResponse<AdminUserResponse>.Ok(result, "Cập nhật trạng thái thành công."));
        }
        catch (KeyNotFoundException ex) { return NotFound(ApiResponse.Fail(ex.Message)); }
    }

    [HttpPut("users/{id:guid}/reset-password")]
    public async Task<IActionResult> ResetPassword(Guid id, [FromBody] ResetPasswordRequest request, CancellationToken ct)
    {
        var validation = await _resetPasswordValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return BadRequest(ApiResponse.Fail(validation.Errors.Select(e => e.ErrorMessage)));

        try
        {
            await _admin.ResetPasswordAsync(id, request, ct);
            return Ok(ApiResponse.Ok("Đặt lại mật khẩu thành công."));
        }
        catch (KeyNotFoundException ex) { return NotFound(ApiResponse.Fail(ex.Message)); }
    }

    [HttpPost("doctors")]
    public async Task<IActionResult> CreateDoctor([FromBody] CreateDoctorRequest request, CancellationToken ct)
    {
        var validation = await _createDoctorValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return BadRequest(ApiResponse.Fail(validation.Errors.Select(e => e.ErrorMessage)));

        try
        {
            var result = await _admin.CreateDoctorAsync(request, ct);
            return Ok(ApiResponse<AdminUserResponse>.Ok(result, "Tạo tài khoản thành công."));
        }
        catch (InvalidOperationException ex) { return Conflict(ApiResponse.Fail(ex.Message)); }
    }

    // ─── Roles & Permissions ──────────────────────────────────────────────────

    [HttpGet("roles")]
    public async Task<IActionResult> GetRoles(CancellationToken ct)
    {
        var result = await _admin.GetRolesAsync(ct);
        return Ok(ApiResponse<IEnumerable<RoleResponse>>.Ok(result));
    }

    [HttpGet("permissions")]
    public async Task<IActionResult> GetPermissions(CancellationToken ct)
    {
        var result = await _admin.GetPermissionsAsync(ct);
        return Ok(ApiResponse<IEnumerable<PermissionResponse>>.Ok(result));
    }

    [HttpGet("roles/{roleId:int}/permissions")]
    public async Task<IActionResult> GetRolePermissions(int roleId, CancellationToken ct)
    {
        var result = await _admin.GetRolePermissionIdsAsync(roleId, ct);
        return Ok(ApiResponse<IEnumerable<int>>.Ok(result));
    }

    [HttpPut("roles/{roleId:int}/permissions")]
    public async Task<IActionResult> UpdateRolePermissions(int roleId, [FromBody] UpdateRolePermissionsRequest request, CancellationToken ct)
    {
        try
        {
            await _admin.UpdateRolePermissionsAsync(roleId, request, ct);
            return Ok(ApiResponse.Ok("Cập nhật quyền thành công."));
        }
        catch (KeyNotFoundException ex) { return NotFound(ApiResponse.Fail(ex.Message)); }
    }

    // ─── Doctor-Patient assignment ────────────────────────────────────────────

    [HttpGet("doctor-patients")]
    public async Task<IActionResult> GetDoctorPatients([FromQuery] Guid? doctorId, CancellationToken ct)
    {
        var result = await _admin.GetDoctorPatientsAsync(doctorId, ct);
        return Ok(ApiResponse<IEnumerable<DoctorPatientResponse>>.Ok(result));
    }

    [HttpPost("doctor-patients")]
    public async Task<IActionResult> AssignPatient([FromBody] AssignPatientRequest request, CancellationToken ct)
    {
        try
        {
            var result = await _admin.AssignPatientAsync(request, CurrentUserId, ct);
            return Ok(ApiResponse<DoctorPatientResponse>.Ok(result, "Gán bệnh nhân thành công."));
        }
        catch (KeyNotFoundException ex) { return NotFound(ApiResponse.Fail(ex.Message)); }
        catch (InvalidOperationException ex) { return Conflict(ApiResponse.Fail(ex.Message)); }
    }

    [HttpDelete("doctor-patients/{id:guid}")]
    public async Task<IActionResult> UnassignPatient(Guid id, CancellationToken ct)
    {
        try
        {
            await _admin.UnassignPatientAsync(id, ct);
            return Ok(ApiResponse.Ok("Huỷ gán bệnh nhân thành công."));
        }
        catch (KeyNotFoundException ex) { return NotFound(ApiResponse.Fail(ex.Message)); }
    }

    // ─── Audit Logs ───────────────────────────────────────────────────────────

    [HttpGet("audit-logs")]
    public async Task<IActionResult> GetAuditLogs([FromQuery] int page = 1, [FromQuery] int pageSize = 20,
        [FromQuery] Guid? userId = null, [FromQuery] string? entity = null, CancellationToken ct = default)
    {
        var result = await _admin.GetAuditLogsAsync(page, pageSize, userId, entity, ct);
        return Ok(ApiResponse<PagedResult<AuditLogResponse>>.Ok(result));
    }

    // ─── Lịch hẹn (toàn hệ thống) ─────────────────────────────────────────────

    [HttpGet("appointments")]
    public async Task<IActionResult> GetAllAppointments([FromQuery] int page = 1, [FromQuery] int pageSize = 20,
        [FromQuery] AppointmentStatus? status = null, CancellationToken ct = default)
    {
        var result = await _admin.GetAllAppointmentsAsync(page, pageSize, status, ct);
        return Ok(ApiResponse<PagedResult<AppointmentResponse>>.Ok(result));
    }

    // ─── System Settings ──────────────────────────────────────────────────────

    [HttpGet("settings")]
    public async Task<IActionResult> GetSettings(CancellationToken ct)
    {
        var result = await _admin.GetSystemSettingsAsync(ct);
        return Ok(ApiResponse<IEnumerable<SystemSettingResponse>>.Ok(result));
    }

    [HttpPut("settings/{key}")]
    public async Task<IActionResult> UpdateSetting(string key, [FromBody] UpdateSystemSettingRequest request, CancellationToken ct)
    {
        try
        {
            var result = await _admin.UpdateSystemSettingAsync(key, request, ct);
            return Ok(ApiResponse<SystemSettingResponse>.Ok(result, "Cập nhật cấu hình thành công."));
        }
        catch (KeyNotFoundException ex) { return NotFound(ApiResponse.Fail(ex.Message)); }
    }
}
