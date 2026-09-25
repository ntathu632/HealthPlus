using HealthPlus.Application.DTOs.Hospitals;

namespace HealthPlus.Application.Interfaces;

public interface IHospitalService
{
    Task<IEnumerable<HospitalResponse>> GetAllAsync(CancellationToken ct = default);
}
