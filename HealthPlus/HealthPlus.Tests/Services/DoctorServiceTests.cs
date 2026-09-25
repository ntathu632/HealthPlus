using System.Linq.Expressions;
using HealthPlus.Application.DTOs.MedicalHistory;
using HealthPlus.Application.DTOs.Vaccines;
using HealthPlus.Application.Interfaces;
using HealthPlus.Application.Services;
using HealthPlus.Domain.Entities;
using HealthPlus.Domain.Interfaces.Repositories;
using Moq;
using Xunit;

namespace HealthPlus.Tests.Services;

public class DoctorServiceTests
{
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly Mock<IGenericRepository<DoctorPatient>> _doctorPatients = new();
    private readonly Mock<IGenericRepository<User>> _users = new();
    private readonly Mock<IHealthRecordService> _healthRecords = new();
    private readonly Mock<IMedicalHistoryService> _medicalHistories = new();
    private readonly Mock<IVaccineService> _vaccines = new();
    private readonly Mock<IPrescriptionService> _prescriptions = new();
    private readonly DoctorService _sut;

    private readonly Guid _doctorId = Guid.NewGuid();
    private readonly Guid _patientId = Guid.NewGuid();

    public DoctorServiceTests()
    {
        _uow.SetupGet(u => u.DoctorPatients).Returns(_doctorPatients.Object);
        _uow.SetupGet(u => u.Users).Returns(_users.Object);

        _sut = new DoctorService(
            _uow.Object, _healthRecords.Object, _medicalHistories.Object, _vaccines.Object, _prescriptions.Object);
    }

    private void SetupAssigned(bool isActive = true)
    {
        _doctorPatients
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<Expression<Func<DoctorPatient, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(isActive
                ? new DoctorPatient { DoctorId = _doctorId, PatientId = _patientId, IsActive = true }
                : null);
    }

    [Fact]
    public async Task AddVaccineAsync_PatientNotAssigned_ThrowsUnauthorizedAccessException()
    {
        SetupAssigned(isActive: false);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _sut.AddVaccineAsync(_doctorId, _patientId, new CreateVaccineRequest(), CancellationToken.None));

        _vaccines.Verify(v => v.CreateAsync(It.IsAny<Guid>(), It.IsAny<CreateVaccineRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task AddVaccineAsync_PatientAssigned_DelegatesToVaccineService()
    {
        SetupAssigned();
        var request = new CreateVaccineRequest { VaccineName = "Cúm" };
        var expected = new VaccineResponse { Id = Guid.NewGuid(), VaccineName = "Cúm" };
        _vaccines.Setup(v => v.CreateAsync(_patientId, request, It.IsAny<CancellationToken>())).ReturnsAsync(expected);

        var result = await _sut.AddVaccineAsync(_doctorId, _patientId, request, CancellationToken.None);

        Assert.Equal(expected.Id, result.Id);
        _vaccines.Verify(v => v.CreateAsync(_patientId, request, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateVaccineAsync_PatientNotAssigned_ThrowsAndDoesNotCallVaccineService()
    {
        SetupAssigned(isActive: false);
        var vaccineId = Guid.NewGuid();

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _sut.UpdateVaccineAsync(_doctorId, _patientId, vaccineId, new UpdateVaccineRequest(), CancellationToken.None));

        _vaccines.Verify(v => v.UpdateAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<UpdateVaccineRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeleteVaccineAsync_PatientAssigned_DelegatesToVaccineService()
    {
        SetupAssigned();
        var vaccineId = Guid.NewGuid();

        await _sut.DeleteVaccineAsync(_doctorId, _patientId, vaccineId, CancellationToken.None);

        _vaccines.Verify(v => v.DeleteAsync(vaccineId, _patientId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateMedicalHistoryAsync_PatientNotAssigned_ThrowsAndDoesNotCallService()
    {
        SetupAssigned(isActive: false);
        var recordId = Guid.NewGuid();

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _sut.UpdateMedicalHistoryAsync(_doctorId, _patientId, recordId, new UpdateMedicalHistoryRequest(), CancellationToken.None));

        _medicalHistories.Verify(
            m => m.UpdateAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<UpdateMedicalHistoryRequest>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task DeleteMedicalHistoryAsync_PatientAssigned_DelegatesToMedicalHistoryService()
    {
        SetupAssigned();
        var recordId = Guid.NewGuid();

        await _sut.DeleteMedicalHistoryAsync(_doctorId, _patientId, recordId, CancellationToken.None);

        _medicalHistories.Verify(m => m.DeleteAsync(recordId, _patientId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeletePrescriptionAsync_PatientNotAssigned_ThrowsAndDoesNotCallService()
    {
        SetupAssigned(isActive: false);
        var prescriptionId = Guid.NewGuid();

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _sut.DeletePrescriptionAsync(_doctorId, _patientId, prescriptionId, CancellationToken.None));

        _prescriptions.Verify(p => p.DeleteAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeletePrescriptionAsync_PatientAssigned_DelegatesToPrescriptionService()
    {
        SetupAssigned();
        var prescriptionId = Guid.NewGuid();

        await _sut.DeletePrescriptionAsync(_doctorId, _patientId, prescriptionId, CancellationToken.None);

        _prescriptions.Verify(p => p.DeleteAsync(prescriptionId, _patientId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetMyPatientsAsync_ReturnsOnlyActiveAssignmentsOrderedByMostRecentlyAssigned()
    {
        var olderPatientId = Guid.NewGuid();
        var newerPatientId = Guid.NewGuid();
        var olderAssignment = new DoctorPatient
        {
            DoctorId = _doctorId, PatientId = olderPatientId, IsActive = true,
            CreatedAt = DateTime.UtcNow.AddDays(-2),
        };
        var newerAssignment = new DoctorPatient
        {
            DoctorId = _doctorId, PatientId = newerPatientId, IsActive = true,
            CreatedAt = DateTime.UtcNow.AddDays(-1),
        };

        _doctorPatients
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<DoctorPatient, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([olderAssignment, newerAssignment]);

        _users
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([
                new User { Id = olderPatientId, Email = "old@x.com", FullName = "Old Patient" },
                new User { Id = newerPatientId, Email = "new@x.com", FullName = "New Patient" },
            ]);

        var result = (await _sut.GetMyPatientsAsync(_doctorId, CancellationToken.None)).ToList();

        Assert.Equal(2, result.Count);
        Assert.Equal(newerPatientId, result[0].Id);
        Assert.Equal(olderPatientId, result[1].Id);
    }
}
