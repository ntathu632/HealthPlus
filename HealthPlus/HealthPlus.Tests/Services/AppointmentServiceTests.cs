using System.Linq.Expressions;
using HealthPlus.Application.DTOs.Appointments;
using HealthPlus.Application.Services;
using HealthPlus.Domain.Entities;
using HealthPlus.Domain.Enums;
using HealthPlus.Domain.Interfaces.Repositories;
using Moq;
using Xunit;

namespace HealthPlus.Tests.Services;

public class AppointmentServiceTests
{
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly Mock<IGenericRepository<Appointment>> _appointments = new();
    private readonly Mock<IGenericRepository<User>> _users = new();
    private readonly Mock<IGenericRepository<UserRole>> _userRoles = new();
    private readonly AppointmentService _sut;

    private readonly Guid _patientId = Guid.NewGuid();
    private readonly Guid _doctorId = Guid.NewGuid();

    public AppointmentServiceTests()
    {
        _uow.SetupGet(u => u.Appointments).Returns(_appointments.Object);
        _uow.SetupGet(u => u.Users).Returns(_users.Object);
        _uow.SetupGet(u => u.UserRoles).Returns(_userRoles.Object);
        _sut = new AppointmentService(_uow.Object);

        _users.Setup(r => r.GetByIdAsync(_doctorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { Id = _doctorId, FullName = "Bác sĩ Demo", IsActive = true });
        _users.Setup(r => r.GetByIdAsync(_patientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { Id = _patientId, FullName = "Bệnh nhân Demo" });
        _userRoles.Setup(r => r.AnyAsync(It.IsAny<Expression<Func<UserRole, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
    }

    [Fact]
    public async Task CreateAsync_PastAppointmentTime_ThrowsInvalidOperationException()
    {
        var request = new CreateAppointmentRequest { DoctorId = _doctorId, AppointmentTime = DateTime.UtcNow.AddDays(-1) };

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _sut.CreateAsync(_patientId, request, CancellationToken.None));

        _appointments.Verify(r => r.AddAsync(It.IsAny<Appointment>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_DoctorAlreadyBookedAtSameTime_ThrowsInvalidOperationException()
    {
        _appointments.Setup(r => r.AnyAsync(It.IsAny<Expression<Func<Appointment, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        var request = new CreateAppointmentRequest { DoctorId = _doctorId, AppointmentTime = DateTime.UtcNow.AddDays(1) };

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _sut.CreateAsync(_patientId, request, CancellationToken.None));

        _appointments.Verify(r => r.AddAsync(It.IsAny<Appointment>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_FreeSlot_CreatesAppointmentWithVideoRoomUrl()
    {
        _appointments.Setup(r => r.AnyAsync(It.IsAny<Expression<Func<Appointment, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        var request = new CreateAppointmentRequest { DoctorId = _doctorId, AppointmentTime = DateTime.UtcNow.AddDays(1) };

        var result = await _sut.CreateAsync(_patientId, request, CancellationToken.None);

        Assert.StartsWith("https://meet.jit.si/healthplus-", result.VideoRoomUrl);
        _appointments.Verify(r => r.AddAsync(It.IsAny<Appointment>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
