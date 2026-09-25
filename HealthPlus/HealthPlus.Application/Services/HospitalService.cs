using HealthPlus.Application.DTOs.Hospitals;
using HealthPlus.Application.Interfaces;
using HealthPlus.Domain.Interfaces.Repositories;

namespace HealthPlus.Application.Services;

public class HospitalService : IHospitalService
{
    private readonly IUnitOfWork _uow;
    public HospitalService(IUnitOfWork uow) => _uow = uow;

    public async Task<IEnumerable<HospitalResponse>> GetAllAsync(CancellationToken ct = default)
    {
        var hospitals = await _uow.Hospitals.GetAllAsync(ct);
        return hospitals.OrderBy(h => h.Name).Select(h => new HospitalResponse
        {
            Id = h.Id,
            Name = h.Name,
            Address = h.Address,
        });
    }
}
