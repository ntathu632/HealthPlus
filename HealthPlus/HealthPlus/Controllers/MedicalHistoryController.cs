using HealthPlus.Application.Common;
using HealthPlus.Application.DTOs.MedicalHistory;
using HealthPlus.Application.Interfaces;
using HealthPlus.Domain.Entities;
using HealthPlus.Domain.Interfaces.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthPlus.Controllers;

[Authorize]
[Route("api/medical-history")]
[ApiController]
public class MedicalHistoryController : BaseApiController
{
    private readonly IMedicalHistoryService _service;
    private readonly IFileStorageService _storage;
    private readonly IUnitOfWork _uow;

    public MedicalHistoryController(IMedicalHistoryService service, IFileStorageService storage, IUnitOfWork uow)
    {
        _service = service;
        _storage = storage;
        _uow = uow;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] Guid? healthRecordId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var result = await _service.GetAllAsync(CurrentUserId, healthRecordId, page, pageSize, ct);
        return Ok(ApiResponse<PagedResult<MedicalHistoryResponse>>.Ok(result));
    }

    [HttpGet("follow-ups")]
    public async Task<IActionResult> GetUpcomingFollowUps([FromQuery] int days = 30, CancellationToken ct = default)
    {
        var result = await _service.GetUpcomingFollowUpsAsync(CurrentUserId, days, ct);
        return Ok(ApiResponse<IEnumerable<MedicalHistoryResponse>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        try
        {
            var result = await _service.GetByIdAsync(id, CurrentUserId, ct);
            return Ok(ApiResponse<MedicalHistoryResponse>.Ok(result));
        }
        catch (KeyNotFoundException ex) { return NotFound(ApiResponse<MedicalHistoryResponse>.Fail(ex.Message)); }
        catch (UnauthorizedAccessException) { return Forbid(); }
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateMedicalHistoryRequest request, CancellationToken ct)
    {
        try
        {
            var result = await _service.CreateAsync(CurrentUserId, request, ct);
            return CreatedAtAction(nameof(GetById), new { id = result.Id },
                ApiResponse<MedicalHistoryResponse>.Ok(result, "Thêm lịch sử khám thành công."));
        }
        catch (KeyNotFoundException ex) { return NotFound(ApiResponse<MedicalHistoryResponse>.Fail(ex.Message)); }
        catch (UnauthorizedAccessException) { return Forbid(); }
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateMedicalHistoryRequest request, CancellationToken ct)
    {
        try
        {
            var result = await _service.UpdateAsync(id, CurrentUserId, request, ct);
            return Ok(ApiResponse<MedicalHistoryResponse>.Ok(result, "Cập nhật thành công."));
        }
        catch (KeyNotFoundException ex) { return NotFound(ApiResponse<MedicalHistoryResponse>.Fail(ex.Message)); }
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

    // ─── Document Upload ──────────────────────────────────────────────────────

    [HttpPost("{id:guid}/documents")]
    [RequestSizeLimit(10 * 1024 * 1024)] // 10 MB
    public async Task<IActionResult> UploadDocument(Guid id, IFormFile file, [FromForm] string? documentType, CancellationToken ct)
    {
        if (file is null || file.Length == 0)
            return BadRequest(ApiResponse.Fail("Vui lòng chọn file."));

        var allowedTypes = new[] { "image/jpeg", "image/png", "application/pdf" };
        if (!allowedTypes.Contains(file.ContentType.ToLower()))
            return BadRequest(ApiResponse.Fail("Chỉ hỗ trợ JPG, PNG, PDF."));

        try
        {
            var history = await _uow.MedicalHistories.GetByIdAsync(id, ct)
                ?? throw new KeyNotFoundException("Không tìm thấy lịch sử khám.");
            if (history.UserId != CurrentUserId)
                throw new UnauthorizedAccessException();

            await using var stream = file.OpenReadStream();
            var (key, url) = await _storage.SaveAsync(stream, file.FileName, "medical-documents", ct);

            var doc = new MedicalDocument
            {
                Id = Guid.NewGuid(),
                MedicalHistoryId = id,
                FileName = file.FileName,
                StorageKey = key,
                FileUrl = url,
                FileType = file.ContentType,
                FileSizeBytes = file.Length,
                DocumentType = documentType,
                UploadedAt = DateTime.UtcNow,
            };
            await _uow.MedicalDocuments.AddAsync(doc, ct);
            await _uow.SaveChangesAsync(ct);

            return Ok(ApiResponse<object>.Ok(new { doc.Id, doc.FileName, doc.FileUrl, doc.FileType, doc.FileSizeBytes }, "Upload thành công."));
        }
        catch (KeyNotFoundException ex) { return NotFound(ApiResponse.Fail(ex.Message)); }
        catch (UnauthorizedAccessException) { return Forbid(); }
    }
}
