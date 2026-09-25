using HealthPlus.Application.Common;
using HealthPlus.Application.DTOs.Reminders;
using HealthPlus.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthPlus.Controllers;

[Authorize]
public class RemindersController : BaseApiController
{
    private readonly IReminderService _service;
    public RemindersController(IReminderService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool? isEnabled, CancellationToken ct)
    {
        var result = await _service.GetAllAsync(CurrentUserId, isEnabled, ct);
        return Ok(ApiResponse<IEnumerable<ReminderResponse>>.Ok(result));
    }

    [HttpGet("upcoming")]
    public async Task<IActionResult> GetUpcoming([FromQuery] int hours = 24, CancellationToken ct = default)
    {
        var result = await _service.GetUpcomingAsync(CurrentUserId, hours, ct);
        return Ok(ApiResponse<IEnumerable<ReminderResponse>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        try
        {
            var result = await _service.GetByIdAsync(id, CurrentUserId, ct);
            return Ok(ApiResponse<ReminderResponse>.Ok(result));
        }
        catch (KeyNotFoundException ex) { return NotFound(ApiResponse<ReminderResponse>.Fail(ex.Message)); }
        catch (UnauthorizedAccessException) { return Forbid(); }
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateReminderRequest request, CancellationToken ct)
    {
        var result = await _service.CreateAsync(CurrentUserId, request, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id },
            ApiResponse<ReminderResponse>.Ok(result, "Tạo nhắc nhở thành công."));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateReminderRequest request, CancellationToken ct)
    {
        try
        {
            var result = await _service.UpdateAsync(id, CurrentUserId, request, ct);
            return Ok(ApiResponse<ReminderResponse>.Ok(result, "Cập nhật nhắc nhở thành công."));
        }
        catch (KeyNotFoundException ex) { return NotFound(ApiResponse<ReminderResponse>.Fail(ex.Message)); }
        catch (UnauthorizedAccessException) { return Forbid(); }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        try
        {
            await _service.DeleteAsync(id, CurrentUserId, ct);
            return Ok(ApiResponse.Ok("Xóa nhắc nhở thành công."));
        }
        catch (KeyNotFoundException ex) { return NotFound(ApiResponse.Fail(ex.Message)); }
        catch (UnauthorizedAccessException) { return Forbid(); }
    }

    // ─── Notification Settings ────────────────────────────────────────────────

    [HttpGet("/api/notifications/settings")]
    public async Task<IActionResult> GetSettings(CancellationToken ct)
    {
        var result = await _service.GetNotificationSettingAsync(CurrentUserId, ct);
        return Ok(ApiResponse<NotificationSettingResponse>.Ok(result));
    }

    [HttpPut("/api/notifications/settings")]
    public async Task<IActionResult> UpdateSettings([FromBody] UpdateNotificationSettingRequest request, CancellationToken ct)
    {
        var result = await _service.UpdateNotificationSettingAsync(CurrentUserId, request, ct);
        return Ok(ApiResponse<NotificationSettingResponse>.Ok(result, "Cập nhật cài đặt thông báo thành công."));
    }
}
