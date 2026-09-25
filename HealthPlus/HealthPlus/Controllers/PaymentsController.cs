using HealthPlus.Application.Common;
using HealthPlus.Application.DTOs.Payments;
using HealthPlus.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthPlus.Controllers;

[Authorize]
public class PaymentsController : BaseApiController
{
    private readonly IPaymentService _service;
    public PaymentsController(IPaymentService service) => _service = service;

    [HttpGet("my")]
    public async Task<IActionResult> GetMyPayments(CancellationToken ct)
    {
        var result = await _service.GetMyPaymentsAsync(CurrentUserId, ct);
        return Ok(ApiResponse<IEnumerable<PaymentResponse>>.Ok(result));
    }

    [Authorize(Roles = "Doctor")]
    [HttpGet("doctor-earnings")]
    public async Task<IActionResult> GetMyEarnings(CancellationToken ct)
    {
        var result = await _service.GetMyEarningsAsync(CurrentUserId, ct);
        return Ok(ApiResponse<IEnumerable<PaymentResponse>>.Ok(result));
    }

    [HttpPost("{appointmentId:guid}/pay")]
    public async Task<IActionResult> Pay(Guid appointmentId, CancellationToken ct)
    {
        try
        {
            var result = await _service.SimulatePayAsync(CurrentUserId, appointmentId, ct);
            return Ok(ApiResponse<PaymentResponse>.Ok(result, "Thanh toán thành công (mô phỏng)."));
        }
        catch (KeyNotFoundException ex) { return NotFound(ApiResponse.Fail(ex.Message)); }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, ApiResponse.Fail(ex.Message)); }
        catch (InvalidOperationException ex) { return Conflict(ApiResponse.Fail(ex.Message)); }
    }
}
