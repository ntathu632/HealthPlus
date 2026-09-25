using HealthPlus.Application.Common;
using HealthPlus.Application.DTOs.MedicalHistory;

namespace HealthPlus.Application.Interfaces;

public interface IMedicalHistoryService
{
    Task<PagedResult<MedicalHistoryResponse>> GetAllAsync(Guid userId, Guid? healthRecordId, int page, int pageSize, CancellationToken ct = default);
    Task<MedicalHistoryResponse> GetByIdAsync(Guid id, Guid userId, CancellationToken ct = default);
    Task<MedicalHistoryResponse> CreateAsync(Guid userId, CreateMedicalHistoryRequest request, CancellationToken ct = default);
    Task<MedicalHistoryResponse> UpdateAsync(Guid id, Guid userId, UpdateMedicalHistoryRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid id, Guid userId, CancellationToken ct = default);
    Task<IEnumerable<MedicalHistoryResponse>> GetUpcomingFollowUpsAsync(Guid userId, int days = 30, CancellationToken ct = default);
}
