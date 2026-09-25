using HealthPlus.Application.Common;
using HealthPlus.Application.DTOs.MedicalHistory;
using HealthPlus.Application.Interfaces;
using HealthPlus.Domain.Entities;
using HealthPlus.Domain.Interfaces.Repositories;

namespace HealthPlus.Application.Services;

public class MedicalHistoryService : IMedicalHistoryService
{
    private readonly IUnitOfWork _uow;
    public MedicalHistoryService(IUnitOfWork uow) => _uow = uow;

    public async Task<PagedResult<MedicalHistoryResponse>> GetAllAsync(Guid userId, Guid? healthRecordId, int page, int pageSize, CancellationToken ct = default)
    {
        var records = await _uow.MedicalHistories.FindAsync(
            mh => mh.UserId == userId && (healthRecordId == null || mh.HealthRecordId == healthRecordId), ct);
        var ordered = records.OrderByDescending(mh => mh.VisitDate).Select(Map);
        return PagedResult<MedicalHistoryResponse>.Create(ordered, page, pageSize);
    }

    public async Task<MedicalHistoryResponse> GetByIdAsync(Guid id, Guid userId, CancellationToken ct = default)
    {
        var record = await GetOwnedAsync(id, userId, ct);
        return Map(record);
    }

    public async Task<MedicalHistoryResponse> CreateAsync(Guid userId, CreateMedicalHistoryRequest req, CancellationToken ct = default)
    {
        // Verify health record belongs to user
        var healthRecord = await _uow.HealthRecords.GetByIdAsync(req.HealthRecordId, ct)
            ?? throw new KeyNotFoundException("Không tìm thấy hồ sơ sức khỏe.");
        if (healthRecord.UserId != userId)
            throw new UnauthorizedAccessException("Bạn không có quyền truy cập hồ sơ này.");

        var entity = new Domain.Entities.MedicalHistory
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            HealthRecordId = req.HealthRecordId,
            VisitDate = req.VisitDate,
            DoctorName = req.DoctorName?.Trim(),
            Hospital = req.Hospital?.Trim(),
            Specialty = req.Specialty?.Trim(),
            Diagnosis = req.Diagnosis?.Trim(),
            Treatment = req.Treatment?.Trim(),
            FollowUpDate = req.FollowUpDate,
            Notes = req.Notes?.Trim(),
        };
        await _uow.MedicalHistories.AddAsync(entity, ct);
        await _uow.SaveChangesAsync(ct);
        return Map(entity);
    }

    public async Task<MedicalHistoryResponse> UpdateAsync(Guid id, Guid userId, UpdateMedicalHistoryRequest req, CancellationToken ct = default)
    {
        var entity = await GetOwnedAsync(id, userId, ct);
        entity.VisitDate = req.VisitDate;
        entity.DoctorName = req.DoctorName?.Trim();
        entity.Hospital = req.Hospital?.Trim();
        entity.Specialty = req.Specialty?.Trim();
        entity.Diagnosis = req.Diagnosis?.Trim();
        entity.Treatment = req.Treatment?.Trim();
        entity.FollowUpDate = req.FollowUpDate;
        entity.Notes = req.Notes?.Trim();
        entity.UpdatedAt = DateTime.UtcNow;
        _uow.MedicalHistories.Update(entity);
        await _uow.SaveChangesAsync(ct);
        return Map(entity);
    }

    public async Task DeleteAsync(Guid id, Guid userId, CancellationToken ct = default)
    {
        var entity = await GetOwnedAsync(id, userId, ct);
        _uow.MedicalHistories.Remove(entity);
        await _uow.SaveChangesAsync(ct);
    }

    public async Task<IEnumerable<MedicalHistoryResponse>> GetUpcomingFollowUpsAsync(Guid userId, int days = 30, CancellationToken ct = default)
    {
        var cutoff = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(days));
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var records = await _uow.MedicalHistories.FindAsync(
            mh => mh.UserId == userId && mh.FollowUpDate != null
                  && mh.FollowUpDate >= today && mh.FollowUpDate <= cutoff, ct);
        return records.OrderBy(mh => mh.FollowUpDate).Select(Map);
    }

    private async Task<Domain.Entities.MedicalHistory> GetOwnedAsync(Guid id, Guid userId, CancellationToken ct)
    {
        var entity = await _uow.MedicalHistories.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException("Không tìm thấy lịch sử khám.");
        if (entity.UserId != userId)
            throw new UnauthorizedAccessException("Bạn không có quyền truy cập.");
        return entity;
    }

    private static MedicalHistoryResponse Map(Domain.Entities.MedicalHistory e) => new()
    {
        Id = e.Id, UserId = e.UserId, HealthRecordId = e.HealthRecordId,
        VisitDate = e.VisitDate, DoctorName = e.DoctorName, Hospital = e.Hospital,
        Specialty = e.Specialty, Diagnosis = e.Diagnosis, Treatment = e.Treatment,
        FollowUpDate = e.FollowUpDate, Notes = e.Notes,
        CreatedAt = e.CreatedAt, UpdatedAt = e.UpdatedAt,
    };
}
