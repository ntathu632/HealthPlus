using HealthPlus.Application.Common;
using HealthPlus.Application.DTOs.HealthRecords;
using HealthPlus.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthPlus.Controllers;

[Authorize]
public class HealthRecordsController : BaseApiController
{
    private readonly IHealthRecordService _service;
    public HealthRecordsController(IHealthRecordService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await _service.GetAllByUserAsync(CurrentUserId, ct);
        return Ok(ApiResponse<IEnumerable<HealthRecordResponse>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        try
        {
            var result = await _service.GetByIdAsync(id, CurrentUserId, ct);
            return Ok(ApiResponse<HealthRecordResponse>.Ok(result));
        }
        catch (KeyNotFoundException ex) { return NotFound(ApiResponse<HealthRecordResponse>.Fail(ex.Message)); }
        catch (UnauthorizedAccessException) { return Forbid(); }
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateHealthRecordRequest request, CancellationToken ct)
    {
        var result = await _service.CreateAsync(CurrentUserId, request, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id },
            ApiResponse<HealthRecordResponse>.Ok(result, "Tạo hồ sơ thành công."));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateHealthRecordRequest request, CancellationToken ct)
    {
        try
        {
            var result = await _service.UpdateAsync(id, CurrentUserId, request, ct);
            return Ok(ApiResponse<HealthRecordResponse>.Ok(result, "Cập nhật hồ sơ thành công."));
        }
        catch (KeyNotFoundException ex) { return NotFound(ApiResponse<HealthRecordResponse>.Fail(ex.Message)); }
        catch (UnauthorizedAccessException) { return Forbid(); }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        try
        {
            await _service.DeleteAsync(id, CurrentUserId, ct);
            return Ok(ApiResponse.Ok("Xóa hồ sơ thành công."));
        }
        catch (KeyNotFoundException ex) { return NotFound(ApiResponse.Fail(ex.Message)); }
        catch (UnauthorizedAccessException) { return Forbid(); }
    }

    // ─── Metrics ─────────────────────────────────────────────────────────────

    [HttpGet("{recordId:guid}/metrics")]
    public async Task<IActionResult> GetMetrics(Guid recordId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        try
        {
            var result = await _service.GetMetricsPagedAsync(recordId, CurrentUserId, page, pageSize, ct);
            return Ok(ApiResponse<PagedResult<HealthMetricResponse>>.Ok(result));
        }
        catch (KeyNotFoundException ex) { return NotFound(ApiResponse<PagedResult<HealthMetricResponse>>.Fail(ex.Message)); }
        catch (UnauthorizedAccessException) { return Forbid(); }
    }

    [HttpPost("{recordId:guid}/metrics")]
    public async Task<IActionResult> AddMetric(Guid recordId, [FromBody] CreateHealthMetricRequest request, CancellationToken ct)
    {
        try
        {
            var result = await _service.AddMetricAsync(recordId, CurrentUserId, request, ct);
            return Ok(ApiResponse<HealthMetricResponse>.Ok(result, "Thêm chỉ số thành công."));
        }
        catch (KeyNotFoundException ex) { return NotFound(ApiResponse<HealthMetricResponse>.Fail(ex.Message)); }
        catch (UnauthorizedAccessException) { return Forbid(); }
    }

    [HttpDelete("{recordId:guid}/metrics/{metricId:guid}")]
    public async Task<IActionResult> DeleteMetric(Guid recordId, Guid metricId, CancellationToken ct)
    {
        try
        {
            await _service.DeleteMetricAsync(metricId, CurrentUserId, ct);
            return Ok(ApiResponse.Ok("Xóa chỉ số thành công."));
        }
        catch (KeyNotFoundException ex) { return NotFound(ApiResponse.Fail(ex.Message)); }
        catch (UnauthorizedAccessException) { return Forbid(); }
    }
}
