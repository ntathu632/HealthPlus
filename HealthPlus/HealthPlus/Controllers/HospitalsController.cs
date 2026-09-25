using HealthPlus.Application.Common;
using HealthPlus.Application.DTOs.Hospitals;
using HealthPlus.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthPlus.Controllers;

[Authorize]
public class HospitalsController : BaseApiController
{
    private readonly IHospitalService _service;
    public HospitalsController(IHospitalService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await _service.GetAllAsync(ct);
        return Ok(ApiResponse<IEnumerable<HospitalResponse>>.Ok(result));
    }
}
