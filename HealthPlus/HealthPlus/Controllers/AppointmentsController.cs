using HealthPlus.Application.Common;
using HealthPlus.Application.DTOs.Appointments;
using HealthPlus.Application.Interfaces;
using HealthPlus.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthPlus.Controllers;

[Authorize]
public class AppointmentsController : BaseApiController
{
    private readonly IAppointmentService _service;
    public AppointmentsController(IAppointmentService service) => _service = service;

    [HttpGet("doctors")]
    public async Task<IActionResult> GetActiveDoctors(CancellationToken ct)
    {
        var result = await _service.GetActiveDoctorsAsync(ct);
        return Ok(ApiResponse<IEnumerable<DoctorListItemResponse>>.Ok(result));
    }

    [HttpGet]
    public async Task<IActionResult> GetMyAppointments([FromQuery] AppointmentStatus? status, CancellationToken ct)
    {
        var result = await _service.GetMyAppointmentsAsPatientAsync(CurrentUserId, status, ct);
        return Ok(ApiResponse<IEnumerable<AppointmentResponse>>.Ok(result));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAppointmentRequest request, CancellationToken ct)
    {
        try
        {
            var result = await _service.CreateAsync(CurrentUserId, request, ct);
            return Ok(ApiResponse<AppointmentResponse>.Ok(result, "Đặt lịch hẹn thành công."));
        }
        catch (KeyNotFoundException ex) { return NotFound(ApiResponse.Fail(ex.Message)); }
        catch (InvalidOperationException ex) { return Conflict(ApiResponse.Fail(ex.Message)); }
    }

    [HttpPut("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken ct)
    {
        try
        {
            await _service.CancelAsync(CurrentUserId, id, ct);
            return Ok(ApiResponse.Ok("Đã huỷ lịch hẹn."));
        }
        catch (KeyNotFoundException ex) { return NotFound(ApiResponse.Fail(ex.Message)); }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, ApiResponse.Fail(ex.Message)); }
        catch (InvalidOperationException ex) { return Conflict(ApiResponse.Fail(ex.Message)); }
    }

    [Authorize(Roles = "Doctor")]
    [HttpGet("doctor")]
    public async Task<IActionResult> GetMyAppointmentsAsDoctor([FromQuery] AppointmentStatus? status, CancellationToken ct)
    {
        var result = await _service.GetMyAppointmentsAsDoctorAsync(CurrentUserId, status, ct);
        return Ok(ApiResponse<IEnumerable<AppointmentResponse>>.Ok(result));
    }

    [Authorize(Roles = "Doctor")]
    [HttpPut("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateAppointmentStatusRequest request, CancellationToken ct)
    {
        try
        {
            var result = await _service.UpdateStatusAsync(CurrentUserId, id, request, ct);
            return Ok(ApiResponse<AppointmentResponse>.Ok(result, "Cập nhật lịch hẹn thành công."));
        }
        catch (KeyNotFoundException ex) { return NotFound(ApiResponse.Fail(ex.Message)); }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, ApiResponse.Fail(ex.Message)); }
    }
}
