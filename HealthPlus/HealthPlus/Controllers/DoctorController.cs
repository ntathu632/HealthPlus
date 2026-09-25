using HealthPlus.Application.Common;
using HealthPlus.Application.DTOs.Doctor;
using HealthPlus.Application.DTOs.MedicalHistory;
using HealthPlus.Application.DTOs.Prescriptions;
using HealthPlus.Application.DTOs.Vaccines;
using HealthPlus.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthPlus.Controllers;

[Authorize(Roles = "Doctor")]
public class DoctorController : BaseApiController
{
    private readonly IDoctorService _doctor;
    public DoctorController(IDoctorService doctor) => _doctor = doctor;

    [HttpGet("patients")]
    public async Task<IActionResult> GetMyPatients(CancellationToken ct)
    {
        var result = await _doctor.GetMyPatientsAsync(CurrentUserId, ct);
        return Ok(ApiResponse<IEnumerable<PatientSummaryResponse>>.Ok(result));
    }

    [HttpGet("patients/{patientId:guid}")]
    public async Task<IActionResult> GetPatientProfile(Guid patientId, CancellationToken ct)
    {
        try
        {
            var result = await _doctor.GetPatientProfileAsync(CurrentUserId, patientId, ct);
            return Ok(ApiResponse<PatientProfileResponse>.Ok(result));
        }
        catch (KeyNotFoundException ex) { return NotFound(ApiResponse.Fail(ex.Message)); }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, ApiResponse.Fail(ex.Message)); }
    }

    [HttpPost("patients/{patientId:guid}/medical-history")]
    public async Task<IActionResult> AddMedicalHistory(Guid patientId, [FromBody] CreateMedicalHistoryRequest request, CancellationToken ct)
    {
        try
        {
            var result = await _doctor.AddMedicalHistoryAsync(CurrentUserId, patientId, request, ct);
            return Ok(ApiResponse<MedicalHistoryResponse>.Ok(result, "Thêm chẩn đoán thành công."));
        }
        catch (KeyNotFoundException ex) { return NotFound(ApiResponse.Fail(ex.Message)); }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, ApiResponse.Fail(ex.Message)); }
    }

    [HttpPut("patients/{patientId:guid}/medical-history/{id:guid}")]
    public async Task<IActionResult> UpdateMedicalHistory(Guid patientId, Guid id, [FromBody] UpdateMedicalHistoryRequest request, CancellationToken ct)
    {
        try
        {
            var result = await _doctor.UpdateMedicalHistoryAsync(CurrentUserId, patientId, id, request, ct);
            return Ok(ApiResponse<MedicalHistoryResponse>.Ok(result, "Cập nhật chẩn đoán thành công."));
        }
        catch (KeyNotFoundException ex) { return NotFound(ApiResponse.Fail(ex.Message)); }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, ApiResponse.Fail(ex.Message)); }
    }

    [HttpDelete("patients/{patientId:guid}/medical-history/{id:guid}")]
    public async Task<IActionResult> DeleteMedicalHistory(Guid patientId, Guid id, CancellationToken ct)
    {
        try
        {
            await _doctor.DeleteMedicalHistoryAsync(CurrentUserId, patientId, id, ct);
            return Ok(ApiResponse.Ok("Xoá chẩn đoán thành công."));
        }
        catch (KeyNotFoundException ex) { return NotFound(ApiResponse.Fail(ex.Message)); }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, ApiResponse.Fail(ex.Message)); }
    }

    [HttpPost("patients/{patientId:guid}/vaccines")]
    public async Task<IActionResult> AddVaccine(Guid patientId, [FromBody] CreateVaccineRequest request, CancellationToken ct)
    {
        try
        {
            var result = await _doctor.AddVaccineAsync(CurrentUserId, patientId, request, ct);
            return Ok(ApiResponse<VaccineResponse>.Ok(result, "Thêm mũi tiêm thành công."));
        }
        catch (KeyNotFoundException ex) { return NotFound(ApiResponse.Fail(ex.Message)); }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, ApiResponse.Fail(ex.Message)); }
    }

    [HttpPut("patients/{patientId:guid}/vaccines/{id:guid}")]
    public async Task<IActionResult> UpdateVaccine(Guid patientId, Guid id, [FromBody] UpdateVaccineRequest request, CancellationToken ct)
    {
        try
        {
            var result = await _doctor.UpdateVaccineAsync(CurrentUserId, patientId, id, request, ct);
            return Ok(ApiResponse<VaccineResponse>.Ok(result, "Cập nhật mũi tiêm thành công."));
        }
        catch (KeyNotFoundException ex) { return NotFound(ApiResponse.Fail(ex.Message)); }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, ApiResponse.Fail(ex.Message)); }
    }

    [HttpDelete("patients/{patientId:guid}/vaccines/{id:guid}")]
    public async Task<IActionResult> DeleteVaccine(Guid patientId, Guid id, CancellationToken ct)
    {
        try
        {
            await _doctor.DeleteVaccineAsync(CurrentUserId, patientId, id, ct);
            return Ok(ApiResponse.Ok("Xoá mũi tiêm thành công."));
        }
        catch (KeyNotFoundException ex) { return NotFound(ApiResponse.Fail(ex.Message)); }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, ApiResponse.Fail(ex.Message)); }
    }

    [HttpPost("patients/{patientId:guid}/prescriptions")]
    public async Task<IActionResult> CreatePrescription(Guid patientId, [FromBody] CreatePrescriptionRequest request, CancellationToken ct)
    {
        try
        {
            var result = await _doctor.CreatePrescriptionAsync(CurrentUserId, patientId, request, ct);
            return Ok(ApiResponse<PrescriptionResponse>.Ok(result, "Tạo đơn thuốc thành công."));
        }
        catch (KeyNotFoundException ex) { return NotFound(ApiResponse.Fail(ex.Message)); }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, ApiResponse.Fail(ex.Message)); }
    }

    [HttpDelete("patients/{patientId:guid}/prescriptions/{prescriptionId:guid}")]
    public async Task<IActionResult> DeletePrescription(Guid patientId, Guid prescriptionId, CancellationToken ct)
    {
        try
        {
            await _doctor.DeletePrescriptionAsync(CurrentUserId, patientId, prescriptionId, ct);
            return Ok(ApiResponse.Ok("Xoá đơn thuốc thành công."));
        }
        catch (KeyNotFoundException ex) { return NotFound(ApiResponse.Fail(ex.Message)); }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, ApiResponse.Fail(ex.Message)); }
    }

    [HttpPost("patients/{patientId:guid}/prescriptions/{prescriptionId:guid}/items")]
    public async Task<IActionResult> AddPrescriptionItem(Guid patientId, Guid prescriptionId, [FromBody] CreatePrescriptionItemRequest request, CancellationToken ct)
    {
        try
        {
            var result = await _doctor.AddPrescriptionItemAsync(CurrentUserId, patientId, prescriptionId, request, ct);
            return Ok(ApiResponse<PrescriptionItemResponse>.Ok(result, "Thêm thuốc thành công."));
        }
        catch (KeyNotFoundException ex) { return NotFound(ApiResponse.Fail(ex.Message)); }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, ApiResponse.Fail(ex.Message)); }
    }

    [HttpPut("patients/{patientId:guid}/prescriptions/items/{itemId:guid}")]
    public async Task<IActionResult> UpdatePrescriptionItem(Guid patientId, Guid itemId, [FromBody] UpdatePrescriptionItemRequest request, CancellationToken ct)
    {
        try
        {
            var result = await _doctor.UpdatePrescriptionItemAsync(CurrentUserId, patientId, itemId, request, ct);
            return Ok(ApiResponse<PrescriptionItemResponse>.Ok(result, "Cập nhật thuốc thành công."));
        }
        catch (KeyNotFoundException ex) { return NotFound(ApiResponse.Fail(ex.Message)); }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, ApiResponse.Fail(ex.Message)); }
    }

    [HttpDelete("patients/{patientId:guid}/prescriptions/items/{itemId:guid}")]
    public async Task<IActionResult> DeletePrescriptionItem(Guid patientId, Guid itemId, CancellationToken ct)
    {
        try
        {
            await _doctor.DeletePrescriptionItemAsync(CurrentUserId, patientId, itemId, ct);
            return Ok(ApiResponse.Ok("Xoá thuốc thành công."));
        }
        catch (KeyNotFoundException ex) { return NotFound(ApiResponse.Fail(ex.Message)); }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, ApiResponse.Fail(ex.Message)); }
    }
}
