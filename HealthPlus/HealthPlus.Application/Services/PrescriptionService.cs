using HealthPlus.Application.Common;
using HealthPlus.Application.DTOs.Prescriptions;
using HealthPlus.Application.Interfaces;
using HealthPlus.Domain.Entities;
using HealthPlus.Domain.Enums;
using HealthPlus.Domain.Interfaces.Repositories;

namespace HealthPlus.Application.Services;

public class PrescriptionService : IPrescriptionService
{
    private readonly IUnitOfWork _uow;
    private readonly IFileStorageService _storage;
    private readonly IOcrService _ocr;

    public PrescriptionService(IUnitOfWork uow, IFileStorageService storage, IOcrService ocr)
    {
        _uow = uow;
        _storage = storage;
        _ocr = ocr;
    }

    public async Task<IEnumerable<PrescriptionResponse>> GetAllAsync(Guid userId, CancellationToken ct = default)
    {
        var prescriptions = await _uow.Prescriptions.FindAsync(p => p.UserId == userId, ct);
        var result = new List<PrescriptionResponse>();
        foreach (var p in prescriptions.OrderByDescending(p => p.CreatedAt))
        {
            var items = await _uow.PrescriptionItems.FindAsync(i => i.PrescriptionId == p.Id, ct);
            result.Add(MapResponse(p, items));
        }
        return result;
    }

    public async Task<PrescriptionResponse> GetByIdAsync(Guid id, Guid userId, CancellationToken ct = default)
    {
        var prescription = await GetOwnedAsync(id, userId, ct);
        var items = await _uow.PrescriptionItems.FindAsync(i => i.PrescriptionId == id, ct);
        return MapResponse(prescription, items);
    }

    public async Task<PrescriptionResponse> CreateAsync(Guid userId, CreatePrescriptionRequest req, CancellationToken ct = default)
    {
        var entity = new Prescription
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            MedicalHistoryId = req.MedicalHistoryId,
            Status = PrescriptionStatus.Pending,
        };
        await _uow.Prescriptions.AddAsync(entity, ct);
        await _uow.SaveChangesAsync(ct);
        return MapResponse(entity, []);
    }

    public async Task DeleteAsync(Guid id, Guid userId, CancellationToken ct = default)
    {
        var entity = await GetOwnedAsync(id, userId, ct);
        _uow.Prescriptions.Remove(entity);
        await _uow.SaveChangesAsync(ct);
    }

    public async Task<PrescriptionItemResponse> AddItemAsync(Guid prescriptionId, Guid userId, CreatePrescriptionItemRequest req, CancellationToken ct = default)
    {
        await GetOwnedAsync(prescriptionId, userId, ct);
        var item = new PrescriptionItem
        {
            Id = Guid.NewGuid(),
            PrescriptionId = prescriptionId,
            MedicineName = req.MedicineName.Trim(),
            Dosage = req.Dosage?.Trim(),
            FrequencyPerDay = req.FrequencyPerDay,
            DurationDays = req.DurationDays,
            Timing = req.Timing?.Trim(),
            Instructions = req.Instructions?.Trim(),
            IsConfirmed = false,
        };
        await _uow.PrescriptionItems.AddAsync(item, ct);
        await _uow.SaveChangesAsync(ct);
        return MapItem(item);
    }

    public async Task<PrescriptionItemResponse> UpdateItemAsync(Guid itemId, Guid userId, UpdatePrescriptionItemRequest req, CancellationToken ct = default)
    {
        var item = await _uow.PrescriptionItems.GetByIdAsync(itemId, ct)
            ?? throw new KeyNotFoundException("Không tìm thấy thuốc.");
        await GetOwnedAsync(item.PrescriptionId, userId, ct);
        item.MedicineName = req.MedicineName.Trim();
        item.Dosage = req.Dosage?.Trim();
        item.FrequencyPerDay = req.FrequencyPerDay;
        item.DurationDays = req.DurationDays;
        item.Timing = req.Timing?.Trim();
        item.Instructions = req.Instructions?.Trim();
        item.IsConfirmed = req.IsConfirmed;
        _uow.PrescriptionItems.Update(item);
        await _uow.SaveChangesAsync(ct);
        return MapItem(item);
    }

    public async Task DeleteItemAsync(Guid itemId, Guid userId, CancellationToken ct = default)
    {
        var item = await _uow.PrescriptionItems.GetByIdAsync(itemId, ct)
            ?? throw new KeyNotFoundException("Không tìm thấy thuốc.");
        await GetOwnedAsync(item.PrescriptionId, userId, ct);
        _uow.PrescriptionItems.Remove(item);
        await _uow.SaveChangesAsync(ct);
    }

    public async Task<PrescriptionResponse> UploadImageAsync(Guid id, Guid userId, byte[] imageBytes, string fileName, CancellationToken ct = default)
    {
        var prescription = await GetOwnedAsync(id, userId, ct);

        using (var stream = new MemoryStream(imageBytes))
        {
            var (key, url) = await _storage.SaveAsync(stream, fileName, "prescriptions", ct);
            prescription.ImageStorageKey = key;
            prescription.ImageUrl = url;
        }

        prescription.Status = PrescriptionStatus.Processing;

        try
        {
            var (text, confidence) = await _ocr.ExtractTextAsync(imageBytes, ct);
            prescription.OcrRawText = string.IsNullOrWhiteSpace(text) ? null : text;
            prescription.OcrConfidenceScore = confidence;

            foreach (var parsed in PrescriptionOcrParser.Parse(text))
            {
                await _uow.PrescriptionItems.AddAsync(new PrescriptionItem
                {
                    Id = Guid.NewGuid(),
                    PrescriptionId = prescription.Id,
                    MedicineName = parsed.MedicineName,
                    Dosage = parsed.Dosage,
                    FrequencyPerDay = parsed.FrequencyPerDay,
                    DurationDays = parsed.DurationDays,
                    IsConfirmed = false,
                }, ct);
            }

            prescription.Status = PrescriptionStatus.Completed;
        }
        catch (Exception)
        {
            prescription.Status = PrescriptionStatus.Failed;
        }

        prescription.ProcessedAt = DateTime.UtcNow;
        _uow.Prescriptions.Update(prescription);
        await _uow.SaveChangesAsync(ct);

        var items = await _uow.PrescriptionItems.FindAsync(i => i.PrescriptionId == prescription.Id, ct);
        return MapResponse(prescription, items);
    }

    private async Task<Prescription> GetOwnedAsync(Guid id, Guid userId, CancellationToken ct)
    {
        var entity = await _uow.Prescriptions.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException("Không tìm thấy đơn thuốc.");
        if (entity.UserId != userId)
            throw new UnauthorizedAccessException("Bạn không có quyền truy cập.");
        return entity;
    }

    private static PrescriptionResponse MapResponse(Prescription p, IEnumerable<PrescriptionItem> items) => new()
    {
        Id = p.Id, UserId = p.UserId, MedicalHistoryId = p.MedicalHistoryId,
        ImageUrl = p.ImageUrl, OcrRawText = p.OcrRawText, OcrConfidenceScore = p.OcrConfidenceScore,
        Status = p.Status, ProcessedAt = p.ProcessedAt, CreatedAt = p.CreatedAt,
        Items = items.Select(MapItem).ToList(),
    };

    private static PrescriptionItemResponse MapItem(PrescriptionItem i) => new()
    {
        Id = i.Id, PrescriptionId = i.PrescriptionId, MedicineName = i.MedicineName,
        Dosage = i.Dosage, FrequencyPerDay = i.FrequencyPerDay, DurationDays = i.DurationDays,
        Timing = i.Timing, Instructions = i.Instructions, IsConfirmed = i.IsConfirmed,
    };
}
