using HealthPlus.Application.DTOs.Prescriptions;

namespace HealthPlus.Application.Interfaces;

public interface IPrescriptionService
{
    Task<IEnumerable<PrescriptionResponse>> GetAllAsync(Guid userId, CancellationToken ct = default);
    Task<PrescriptionResponse> GetByIdAsync(Guid id, Guid userId, CancellationToken ct = default);
    Task<PrescriptionResponse> CreateAsync(Guid userId, CreatePrescriptionRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid id, Guid userId, CancellationToken ct = default);

    Task<PrescriptionItemResponse> AddItemAsync(Guid prescriptionId, Guid userId, CreatePrescriptionItemRequest request, CancellationToken ct = default);
    Task<PrescriptionItemResponse> UpdateItemAsync(Guid itemId, Guid userId, UpdatePrescriptionItemRequest request, CancellationToken ct = default);
    Task DeleteItemAsync(Guid itemId, Guid userId, CancellationToken ct = default);

    Task<PrescriptionResponse> UploadImageAsync(Guid id, Guid userId, byte[] imageBytes, string fileName, CancellationToken ct = default);
}
