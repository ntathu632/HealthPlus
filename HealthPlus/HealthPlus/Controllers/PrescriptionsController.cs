using HealthPlus.Application.Common;
using HealthPlus.Application.DTOs.Prescriptions;
using HealthPlus.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthPlus.Controllers;

[Authorize]
public class PrescriptionsController : BaseApiController
{
    private readonly IPrescriptionService _service;
    public PrescriptionsController(IPrescriptionService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await _service.GetAllAsync(CurrentUserId, ct);
        return Ok(ApiResponse<IEnumerable<PrescriptionResponse>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        try
        {
            var result = await _service.GetByIdAsync(id, CurrentUserId, ct);
            return Ok(ApiResponse<PrescriptionResponse>.Ok(result));
        }
        catch (KeyNotFoundException ex) { return NotFound(ApiResponse<PrescriptionResponse>.Fail(ex.Message)); }
        catch (UnauthorizedAccessException) { return Forbid(); }
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePrescriptionRequest request, CancellationToken ct)
    {
        var result = await _service.CreateAsync(CurrentUserId, request, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id },
            ApiResponse<PrescriptionResponse>.Ok(result, "Tạo đơn thuốc thành công."));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        try
        {
            await _service.DeleteAsync(id, CurrentUserId, ct);
            return Ok(ApiResponse.Ok("Xóa đơn thuốc thành công."));
        }
        catch (KeyNotFoundException ex) { return NotFound(ApiResponse.Fail(ex.Message)); }
        catch (UnauthorizedAccessException) { return Forbid(); }
    }

    // ─── Items ────────────────────────────────────────────────────────────────

    [HttpPost("{prescriptionId:guid}/items")]
    public async Task<IActionResult> AddItem(Guid prescriptionId, [FromBody] CreatePrescriptionItemRequest request, CancellationToken ct)
    {
        try
        {
            var result = await _service.AddItemAsync(prescriptionId, CurrentUserId, request, ct);
            return Ok(ApiResponse<PrescriptionItemResponse>.Ok(result, "Thêm thuốc thành công."));
        }
        catch (KeyNotFoundException ex) { return NotFound(ApiResponse<PrescriptionItemResponse>.Fail(ex.Message)); }
        catch (UnauthorizedAccessException) { return Forbid(); }
    }

    [HttpPut("{prescriptionId:guid}/items/{itemId:guid}")]
    public async Task<IActionResult> UpdateItem(Guid prescriptionId, Guid itemId, [FromBody] UpdatePrescriptionItemRequest request, CancellationToken ct)
    {
        try
        {
            var result = await _service.UpdateItemAsync(itemId, CurrentUserId, request, ct);
            return Ok(ApiResponse<PrescriptionItemResponse>.Ok(result, "Cập nhật thuốc thành công."));
        }
        catch (KeyNotFoundException ex) { return NotFound(ApiResponse<PrescriptionItemResponse>.Fail(ex.Message)); }
        catch (UnauthorizedAccessException) { return Forbid(); }
    }

    [HttpDelete("{prescriptionId:guid}/items/{itemId:guid}")]
    public async Task<IActionResult> DeleteItem(Guid prescriptionId, Guid itemId, CancellationToken ct)
    {
        try
        {
            await _service.DeleteItemAsync(itemId, CurrentUserId, ct);
            return Ok(ApiResponse.Ok("Xóa thuốc thành công."));
        }
        catch (KeyNotFoundException ex) { return NotFound(ApiResponse.Fail(ex.Message)); }
        catch (UnauthorizedAccessException) { return Forbid(); }
    }

    // ─── OCR Image Upload ─────────────────────────────────────────────────────

    [HttpPost("{id:guid}/upload-image")]
    [RequestSizeLimit(5 * 1024 * 1024)] // 5 MB
    public async Task<IActionResult> UploadImage(Guid id, IFormFile image, CancellationToken ct)
    {
        if (image is null || image.Length == 0)
            return BadRequest(ApiResponse.Fail("Vui lòng chọn ảnh."));

        var allowedTypes = new[] { "image/jpeg", "image/png" };
        if (!allowedTypes.Contains(image.ContentType.ToLower()))
            return BadRequest(ApiResponse.Fail("Chỉ hỗ trợ JPG và PNG."));

        try
        {
            using var ms = new MemoryStream();
            await image.CopyToAsync(ms, ct);

            var result = await _service.UploadImageAsync(id, CurrentUserId, ms.ToArray(), image.FileName, ct);
            return Ok(ApiResponse<PrescriptionResponse>.Ok(result, "Tải ảnh và nhận diện OCR thành công."));
        }
        catch (KeyNotFoundException ex) { return NotFound(ApiResponse.Fail(ex.Message)); }
        catch (UnauthorizedAccessException) { return Forbid(); }
    }
}
