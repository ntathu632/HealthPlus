using HealthPlus.Application.Common;
using HealthPlus.Application.DTOs.Vaccines;
using HealthPlus.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthPlus.Controllers;

[Authorize]
public class VaccinesController : BaseApiController
{
    private readonly IVaccineService _service;
    public VaccinesController(IVaccineService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] Guid? healthRecordId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var result = await _service.GetAllAsync(CurrentUserId, healthRecordId, page, pageSize, ct);
        return Ok(ApiResponse<PagedResult<VaccineResponse>>.Ok(result));
    }

    [HttpGet("overdue")]
    public async Task<IActionResult> GetOverdue(CancellationToken ct)
    {
        var result = await _service.GetOverdueAsync(CurrentUserId, ct);
        return Ok(ApiResponse<IEnumerable<VaccineResponse>>.Ok(result));
    }

    [HttpGet("templates")]
    public async Task<IActionResult> GetTemplates([FromQuery] string? vaccineName, CancellationToken ct)
    {
        var result = await _service.GetTemplatesAsync(vaccineName, ct);
        return Ok(ApiResponse<IEnumerable<VaccineScheduleTemplateResponse>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        try
        {
            var result = await _service.GetByIdAsync(id, CurrentUserId, ct);
            return Ok(ApiResponse<VaccineResponse>.Ok(result));
        }
        catch (KeyNotFoundException ex) { return NotFound(ApiResponse<VaccineResponse>.Fail(ex.Message)); }
        catch (UnauthorizedAccessException) { return Forbid(); }
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateVaccineRequest request, CancellationToken ct)
    {
        try
        {
            var result = await _service.CreateAsync(CurrentUserId, request, ct);
            return CreatedAtAction(nameof(GetById), new { id = result.Id },
                ApiResponse<VaccineResponse>.Ok(result, "Thêm thông tin tiêm chủng thành công."));
        }
        catch (KeyNotFoundException ex) { return NotFound(ApiResponse<VaccineResponse>.Fail(ex.Message)); }
        catch (UnauthorizedAccessException) { return Forbid(); }
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateVaccineRequest request, CancellationToken ct)
    {
        try
        {
            var result = await _service.UpdateAsync(id, CurrentUserId, request, ct);
            return Ok(ApiResponse<VaccineResponse>.Ok(result, "Cập nhật thành công."));
        }
        catch (KeyNotFoundException ex) { return NotFound(ApiResponse<VaccineResponse>.Fail(ex.Message)); }
        catch (UnauthorizedAccessException) { return Forbid(); }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        try
        {
            await _service.DeleteAsync(id, CurrentUserId, ct);
            return Ok(ApiResponse.Ok("Xóa thành công."));
        }
        catch (KeyNotFoundException ex) { return NotFound(ApiResponse.Fail(ex.Message)); }
        catch (UnauthorizedAccessException) { return Forbid(); }
    }
}
