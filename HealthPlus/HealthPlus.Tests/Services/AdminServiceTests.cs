using System.Linq.Expressions;
using HealthPlus.Application.DTOs.Admin;
using HealthPlus.Application.Interfaces;
using HealthPlus.Application.Services;
using HealthPlus.Domain.Entities;
using HealthPlus.Domain.Enums;
using HealthPlus.Domain.Interfaces.Repositories;
using Moq;
using Xunit;

namespace HealthPlus.Tests.Services;

public class AdminServiceTests
{
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly Mock<IGenericRepository<User>> _users = new();
    private readonly Mock<IGenericRepository<UserRole>> _userRoles = new();
    private readonly Mock<IGenericRepository<Role>> _roles = new();
    private readonly Mock<IGenericRepository<UserNotificationSetting>> _notificationSettings = new();
    private readonly Mock<IGenericRepository<Appointment>> _appointments = new();
    private readonly Mock<IPasswordService> _password = new();
    private readonly AdminService _sut;

    public AdminServiceTests()
    {
        _uow.SetupGet(u => u.Users).Returns(_users.Object);
        _uow.SetupGet(u => u.UserRoles).Returns(_userRoles.Object);
        _uow.SetupGet(u => u.Roles).Returns(_roles.Object);
        _uow.SetupGet(u => u.UserNotificationSettings).Returns(_notificationSettings.Object);
        _uow.SetupGet(u => u.Appointments).Returns(_appointments.Object);

        _sut = new AdminService(_uow.Object, _password.Object);
    }

    private static CreateDoctorRequest MakeCreateRequest(int roleId) => new()
    {
        Email = "new.user@example.com",
        Password = "Password1",
        FullName = "Nguyen Van C",
        RoleId = roleId,
    };

    [Theory]
    [InlineData(1)]
    [InlineData(4)]
    [InlineData(99)]
    public async Task CreateDoctorAsync_RoleIdNotDoctorOrPatient_ThrowsInvalidOperationException(int roleId)
    {
        var request = MakeCreateRequest(roleId);

        await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.CreateDoctorAsync(request));

        _users.Verify(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateDoctorAsync_EmailAlreadyUsed_ThrowsInvalidOperationException()
    {
        var request = MakeCreateRequest(2);
        _users.Setup(r => r.AnyAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

        await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.CreateDoctorAsync(request));

        _users.Verify(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateDoctorAsync_RoleIdThree_CreatesPatientAccountWithNotificationSettings()
    {
        var request = MakeCreateRequest(3);
        _users.Setup(r => r.AnyAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _password.Setup(p => p.Hash(request.Password)).Returns("hashed");
        _roles.Setup(r => r.GetByIdAsync(3, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Role { Id = 3, Name = "User" });

        var result = await _sut.CreateDoctorAsync(request);

        Assert.Equal("User", result.Roles.Single().Name);
        _users.Verify(r => r.AddAsync(It.Is<User>(u => u.Email == "new.user@example.com" && u.PasswordHash == "hashed"), It.IsAny<CancellationToken>()), Times.Once);
        _userRoles.Verify(r => r.AddAsync(It.Is<UserRole>(ur => ur.RoleId == 3), It.IsAny<CancellationToken>()), Times.Once);
        _notificationSettings.Verify(r => r.AddAsync(It.IsAny<UserNotificationSetting>(), It.IsAny<CancellationToken>()), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ResetPasswordAsync_UnknownUser_ThrowsKeyNotFoundException()
    {
        var userId = Guid.NewGuid();
        _users.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _sut.ResetPasswordAsync(userId, new ResetPasswordRequest { NewPassword = "NewPass1" }));
    }

    [Fact]
    public async Task ResetPasswordAsync_ExistingUser_HashesAndSavesNewPassword()
    {
        var userId = Guid.NewGuid();
        var user = new User { Id = userId, Email = "x@x.com", FullName = "X", PasswordHash = "old-hash" };
        _users.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _password.Setup(p => p.Hash("NewPass1")).Returns("new-hash");

        await _sut.ResetPasswordAsync(userId, new ResetPasswordRequest { NewPassword = "NewPass1" });

        Assert.Equal("new-hash", user.PasswordHash);
        _users.Verify(r => r.Update(It.Is<User>(u => u.PasswordHash == "new-hash")), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAllAppointmentsAsync_NoStatusFilter_MapsDoctorAndPatientNames()
    {
        var doctorId = Guid.NewGuid();
        var patientId = Guid.NewGuid();
        var appointment = new Appointment
        {
            Id = Guid.NewGuid(), DoctorId = doctorId, PatientId = patientId,
            AppointmentTime = DateTime.UtcNow.AddDays(1), Status = AppointmentStatus.Pending,
        };

        _appointments
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<Appointment, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([appointment]);
        _users
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([
                new User { Id = doctorId, Email = "doc@x.com", FullName = "BS. Test" },
                new User { Id = patientId, Email = "pat@x.com", FullName = "Patient Test" },
            ]);

        var result = await _sut.GetAllAppointmentsAsync(1, 20, null, CancellationToken.None);

        var item = Assert.Single(result.Items);
        Assert.Equal("BS. Test", item.DoctorName);
        Assert.Equal("Patient Test", item.PatientName);
    }

    [Fact]
    public async Task GetAllAppointmentsAsync_MissingUser_MapsNameAsDeletedPlaceholder()
    {
        var doctorId = Guid.NewGuid();
        var patientId = Guid.NewGuid();
        var appointment = new Appointment
        {
            Id = Guid.NewGuid(), DoctorId = doctorId, PatientId = patientId,
            AppointmentTime = DateTime.UtcNow.AddDays(1), Status = AppointmentStatus.Confirmed,
        };

        _appointments
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<Appointment, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([appointment]);
        _users
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var result = await _sut.GetAllAppointmentsAsync(1, 20, null, CancellationToken.None);

        var item = Assert.Single(result.Items);
        Assert.Equal("(đã xoá)", item.DoctorName);
        Assert.Equal("(đã xoá)", item.PatientName);
    }
}
