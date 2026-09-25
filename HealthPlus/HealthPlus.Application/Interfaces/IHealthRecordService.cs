using HealthPlus.Application.Common;
using HealthPlus.Application.DTOs.HealthRecords;

namespace HealthPlus.Application.Interfaces;

public interface IHealthRecordService
{
    Task<IEnumerable<HealthRecordResponse>> GetAllByUserAsync(Guid userId, CancellationToken ct = default);
    Task<HealthRecordResponse> GetByIdAsync(Guid id, Guid userId, CancellationToken ct = default);
    Task<HealthRecordResponse> CreateAsync(Guid userId, CreateHealthRecordRequest request, CancellationToken ct = default);
    Task<HealthRecordResponse> UpdateAsync(Guid id, Guid userId, UpdateHealthRecordRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid id, Guid userId, CancellationToken ct = default);

    Task<PagedResult<HealthMetricResponse>> GetMetricsPagedAsync(Guid recordId, Guid userId, int page, int pageSize, CancellationToken ct = default);
    Task<HealthMetricResponse> AddMetricAsync(Guid recordId, Guid userId, CreateHealthMetricRequest request, CancellationToken ct = default);
    Task DeleteMetricAsync(Guid metricId, Guid userId, CancellationToken ct = default);
}
