using HealthPlus.Application.Common;
using HealthPlus.Application.DTOs.Vaccines;

namespace HealthPlus.Application.Interfaces;

public interface IVaccineService
{
    Task<PagedResult<VaccineResponse>> GetAllAsync(Guid userId, Guid? healthRecordId, int page, int pageSize, CancellationToken ct = default);
    Task<VaccineResponse> GetByIdAsync(Guid id, Guid userId, CancellationToken ct = default);
    Task<VaccineResponse> CreateAsync(Guid userId, CreateVaccineRequest request, CancellationToken ct = default);
    Task<VaccineResponse> UpdateAsync(Guid id, Guid userId, UpdateVaccineRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid id, Guid userId, CancellationToken ct = default);
    Task<IEnumerable<VaccineResponse>> GetOverdueAsync(Guid userId, CancellationToken ct = default);
    Task<IEnumerable<VaccineScheduleTemplateResponse>> GetTemplatesAsync(string? vaccineName, CancellationToken ct = default);
}
