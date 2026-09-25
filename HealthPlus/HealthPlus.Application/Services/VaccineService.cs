using HealthPlus.Application.Common;
using HealthPlus.Application.DTOs.Vaccines;
using HealthPlus.Application.Interfaces;
using HealthPlus.Domain.Entities;
using HealthPlus.Domain.Enums;
using HealthPlus.Domain.Interfaces.Repositories;

namespace HealthPlus.Application.Services;

public class VaccineService : IVaccineService
{
    private readonly IUnitOfWork _uow;
    public VaccineService(IUnitOfWork uow) => _uow = uow;

    public async Task<PagedResult<VaccineResponse>> GetAllAsync(Guid userId, Guid? healthRecordId, int page, int pageSize, CancellationToken ct = default)
    {
        var vaccines = await _uow.Vaccines.FindAsync(
            v => v.UserId == userId && (healthRecordId == null || v.HealthRecordId == healthRecordId), ct);
        var ordered = vaccines.OrderByDescending(v => v.InjectionDate).Select(Map);
        return PagedResult<VaccineResponse>.Create(ordered, page, pageSize);
    }

    public async Task<VaccineResponse> GetByIdAsync(Guid id, Guid userId, CancellationToken ct = default)
        => Map(await GetOwnedAsync(id, userId, ct));

    public async Task<VaccineResponse> CreateAsync(Guid userId, CreateVaccineRequest req, CancellationToken ct = default)
    {
        var healthRecord = await _uow.HealthRecords.GetByIdAsync(req.HealthRecordId, ct)
            ?? throw new KeyNotFoundException("Không tìm thấy hồ sơ sức khỏe.");
        if (healthRecord.UserId != userId)
            throw new UnauthorizedAccessException("Bạn không có quyền truy cập hồ sơ này.");

        // Tự tính NextDueDate từ template nếu chưa có
        var nextDueDate = req.NextDueDate;
        if (nextDueDate == null)
        {
            var templates = await _uow.VaccineScheduleTemplates.FindAsync(
                t => t.VaccineName == req.VaccineName && t.DoseNumber == req.DoseNumber + 1, ct);
            var nextTemplate = templates.FirstOrDefault();
            if (nextTemplate?.IntervalDays != null)
                nextDueDate = req.InjectionDate.AddDays(nextTemplate.IntervalDays.Value);
        }

        var entity = new Vaccine
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            HealthRecordId = req.HealthRecordId,
            VaccineName = req.VaccineName.Trim(),
            Manufacturer = req.Manufacturer?.Trim(),
            LotNumber = req.LotNumber?.Trim(),
            DoseNumber = req.DoseNumber,
            InjectionDate = req.InjectionDate,
            NextDueDate = nextDueDate,
            Location = req.Location?.Trim(),
            AdministeredBy = req.AdministeredBy?.Trim(),
            SideEffects = req.SideEffects?.Trim(),
            Status = req.Status,
            Notes = req.Notes?.Trim(),
        };
        await _uow.Vaccines.AddAsync(entity, ct);
        await _uow.SaveChangesAsync(ct);
        return Map(entity);
    }

    public async Task<VaccineResponse> UpdateAsync(Guid id, Guid userId, UpdateVaccineRequest req, CancellationToken ct = default)
    {
        var entity = await GetOwnedAsync(id, userId, ct);
        entity.VaccineName = req.VaccineName.Trim();
        entity.Manufacturer = req.Manufacturer?.Trim();
        entity.LotNumber = req.LotNumber?.Trim();
        entity.DoseNumber = req.DoseNumber;
        entity.InjectionDate = req.InjectionDate;
        entity.NextDueDate = req.NextDueDate;
        entity.Location = req.Location?.Trim();
        entity.AdministeredBy = req.AdministeredBy?.Trim();
        entity.SideEffects = req.SideEffects?.Trim();
        entity.Status = req.Status;
        entity.Notes = req.Notes?.Trim();
        entity.UpdatedAt = DateTime.UtcNow;
        _uow.Vaccines.Update(entity);
        await _uow.SaveChangesAsync(ct);
        return Map(entity);
    }

    public async Task DeleteAsync(Guid id, Guid userId, CancellationToken ct = default)
    {
        var entity = await GetOwnedAsync(id, userId, ct);
        _uow.Vaccines.Remove(entity);
        await _uow.SaveChangesAsync(ct);
    }

    public async Task<IEnumerable<VaccineResponse>> GetOverdueAsync(Guid userId, CancellationToken ct = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var vaccines = await _uow.Vaccines.FindAsync(
            v => v.UserId == userId && v.NextDueDate != null && v.NextDueDate < today
                 && v.Status != VaccineStatus.Completed, ct);
        return vaccines.OrderBy(v => v.NextDueDate).Select(Map);
    }

    public async Task<IEnumerable<VaccineScheduleTemplateResponse>> GetTemplatesAsync(string? vaccineName, CancellationToken ct = default)
    {
        var templates = string.IsNullOrWhiteSpace(vaccineName)
            ? await _uow.VaccineScheduleTemplates.GetAllAsync(ct)
            : await _uow.VaccineScheduleTemplates.FindAsync(t => t.VaccineName.Contains(vaccineName), ct);

        return templates.OrderBy(t => t.VaccineName).ThenBy(t => t.DoseNumber).Select(MapTemplate);
    }

    private async Task<Vaccine> GetOwnedAsync(Guid id, Guid userId, CancellationToken ct)
    {
        var entity = await _uow.Vaccines.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException("Không tìm thấy thông tin tiêm chủng.");
        if (entity.UserId != userId)
            throw new UnauthorizedAccessException("Bạn không có quyền truy cập.");
        return entity;
    }

    private static VaccineResponse Map(Vaccine v) => new()
    {
        Id = v.Id, UserId = v.UserId, HealthRecordId = v.HealthRecordId,
        VaccineName = v.VaccineName, Manufacturer = v.Manufacturer, LotNumber = v.LotNumber,
        DoseNumber = v.DoseNumber, InjectionDate = v.InjectionDate, NextDueDate = v.NextDueDate,
        Location = v.Location, AdministeredBy = v.AdministeredBy, SideEffects = v.SideEffects,
        Status = v.Status, Notes = v.Notes, CreatedAt = v.CreatedAt,
    };

    private static VaccineScheduleTemplateResponse MapTemplate(VaccineScheduleTemplate t) => new()
    {
        Id = t.Id, VaccineName = t.VaccineName, DoseNumber = t.DoseNumber,
        IntervalDays = t.IntervalDays, RecommendedAge = t.RecommendedAge, Notes = t.Notes,
    };
}
