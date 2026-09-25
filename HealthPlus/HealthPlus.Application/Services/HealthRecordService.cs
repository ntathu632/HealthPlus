using HealthPlus.Application.Common;
using HealthPlus.Application.DTOs.HealthRecords;
using HealthPlus.Application.Interfaces;
using HealthPlus.Domain.Entities;
using HealthPlus.Domain.Interfaces.Repositories;

namespace HealthPlus.Application.Services;

public class HealthRecordService : IHealthRecordService
{
    private readonly IUnitOfWork _uow;

    public HealthRecordService(IUnitOfWork uow) => _uow = uow;

    public async Task<IEnumerable<HealthRecordResponse>> GetAllByUserAsync(Guid userId, CancellationToken ct = default)
    {
        var records = await _uow.HealthRecords.FindAsync(r => r.UserId == userId, ct);
        var result = new List<HealthRecordResponse>();
        foreach (var r in records.OrderByDescending(r => r.CreatedAt))
        {
            var metrics = await _uow.HealthMetrics.FindAsync(m => m.HealthRecordId == r.Id, ct);
            var latest = metrics.OrderByDescending(m => m.MeasuredAt).FirstOrDefault();
            result.Add(MapToResponse(r, latest));
        }
        return result;
    }

    public async Task<HealthRecordResponse> GetByIdAsync(Guid id, Guid userId, CancellationToken ct = default)
    {
        var record = await GetOwnedRecordAsync(id, userId, ct);
        var metrics = await _uow.HealthMetrics.FindAsync(m => m.HealthRecordId == id, ct);
        var latest = metrics.OrderByDescending(m => m.MeasuredAt).FirstOrDefault();
        return MapToResponse(record, latest);
    }

    public async Task<HealthRecordResponse> CreateAsync(Guid userId, CreateHealthRecordRequest req, CancellationToken ct = default)
    {
        var record = new HealthRecord
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            ProfileName = req.ProfileName.Trim(),
            FullName = req.FullName.Trim(),
            DateOfBirth = req.DateOfBirth,
            Gender = req.Gender,
            BloodType = req.BloodType,
            Allergies = req.Allergies?.Trim(),
            ChronicDiseases = req.ChronicDiseases?.Trim(),
            InsuranceNumber = req.InsuranceNumber?.Trim(),
        };
        await _uow.HealthRecords.AddAsync(record, ct);
        await _uow.SaveChangesAsync(ct);
        return MapToResponse(record, null);
    }

    public async Task<HealthRecordResponse> UpdateAsync(Guid id, Guid userId, UpdateHealthRecordRequest req, CancellationToken ct = default)
    {
        var record = await GetOwnedRecordAsync(id, userId, ct);
        record.ProfileName = req.ProfileName.Trim();
        record.FullName = req.FullName.Trim();
        record.DateOfBirth = req.DateOfBirth;
        record.Gender = req.Gender;
        record.BloodType = req.BloodType;
        record.Allergies = req.Allergies?.Trim();
        record.ChronicDiseases = req.ChronicDiseases?.Trim();
        record.InsuranceNumber = req.InsuranceNumber?.Trim();
        record.UpdatedAt = DateTime.UtcNow;
        _uow.HealthRecords.Update(record);
        await _uow.SaveChangesAsync(ct);
        return await GetByIdAsync(id, userId, ct);
    }

    public async Task DeleteAsync(Guid id, Guid userId, CancellationToken ct = default)
    {
        var record = await GetOwnedRecordAsync(id, userId, ct);
        _uow.HealthRecords.Remove(record);
        await _uow.SaveChangesAsync(ct);
    }

    public async Task<PagedResult<HealthMetricResponse>> GetMetricsPagedAsync(Guid recordId, Guid userId, int page, int pageSize, CancellationToken ct = default)
    {
        await GetOwnedRecordAsync(recordId, userId, ct);
        var metrics = await _uow.HealthMetrics.FindAsync(m => m.HealthRecordId == recordId, ct);
        var ordered = metrics.OrderByDescending(m => m.MeasuredAt).Select(MapMetric);
        return PagedResult<HealthMetricResponse>.Create(ordered, page, pageSize);
    }

    public async Task<HealthMetricResponse> AddMetricAsync(Guid recordId, Guid userId, CreateHealthMetricRequest req, CancellationToken ct = default)
    {
        await GetOwnedRecordAsync(recordId, userId, ct);

        decimal? bmi = null;
        if (req.HeightCm > 0 && req.WeightKg > 0)
        {
            var h = (double)req.HeightCm! / 100.0;
            bmi = Math.Round((decimal)((double)req.WeightKg! / (h * h)), 2);
        }

        var metric = new HealthMetric
        {
            Id = Guid.NewGuid(),
            HealthRecordId = recordId,
            MeasuredAt = req.MeasuredAt ?? DateTime.UtcNow,
            HeightCm = req.HeightCm,
            WeightKg = req.WeightKg,
            Bmi = bmi,
            SystolicBp = req.SystolicBp,
            DiastolicBp = req.DiastolicBp,
            HeartRate = req.HeartRate,
            BloodSugar = req.BloodSugar,
            Temperature = req.Temperature,
            Notes = req.Notes?.Trim(),
        };
        await _uow.HealthMetrics.AddAsync(metric, ct);
        await _uow.SaveChangesAsync(ct);
        return MapMetric(metric);
    }

    public async Task DeleteMetricAsync(Guid metricId, Guid userId, CancellationToken ct = default)
    {
        var metric = await _uow.HealthMetrics.GetByIdAsync(metricId, ct)
            ?? throw new KeyNotFoundException("Không tìm thấy chỉ số đo.");
        await GetOwnedRecordAsync(metric.HealthRecordId, userId, ct);
        _uow.HealthMetrics.Remove(metric);
        await _uow.SaveChangesAsync(ct);
    }

    // ─── Private ─────────────────────────────────────────────────────────────

    private async Task<HealthRecord> GetOwnedRecordAsync(Guid id, Guid userId, CancellationToken ct)
    {
        var record = await _uow.HealthRecords.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException("Không tìm thấy hồ sơ sức khỏe.");
        if (record.UserId != userId)
            throw new UnauthorizedAccessException("Bạn không có quyền truy cập hồ sơ này.");
        return record;
    }

    private static HealthRecordResponse MapToResponse(HealthRecord r, HealthMetric? latest) => new()
    {
        Id = r.Id,
        UserId = r.UserId,
        ProfileName = r.ProfileName,
        FullName = r.FullName,
        DateOfBirth = r.DateOfBirth,
        Gender = r.Gender,
        BloodType = r.BloodType,
        Allergies = r.Allergies,
        ChronicDiseases = r.ChronicDiseases,
        InsuranceNumber = r.InsuranceNumber,
        CreatedAt = r.CreatedAt,
        UpdatedAt = r.UpdatedAt,
        LatestMetric = latest is null ? null : MapMetric(latest),
    };

    private static HealthMetricResponse MapMetric(HealthMetric m) => new()
    {
        Id = m.Id,
        HealthRecordId = m.HealthRecordId,
        MeasuredAt = m.MeasuredAt,
        HeightCm = m.HeightCm,
        WeightKg = m.WeightKg,
        Bmi = m.Bmi,
        SystolicBp = m.SystolicBp,
        DiastolicBp = m.DiastolicBp,
        HeartRate = m.HeartRate,
        BloodSugar = m.BloodSugar,
        Temperature = m.Temperature,
        Notes = m.Notes,
        CreatedAt = m.CreatedAt,
    };
}
