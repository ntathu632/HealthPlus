using System.Linq.Expressions;
using HealthPlus.Application.Services;
using HealthPlus.Domain.Entities;
using HealthPlus.Domain.Enums;
using HealthPlus.Domain.Interfaces.Repositories;
using Moq;
using Xunit;

namespace HealthPlus.Tests.Services;

public class PaymentServiceTests
{
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly Mock<IGenericRepository<Appointment>> _appointments = new();
    private readonly Mock<IGenericRepository<Payment>> _payments = new();
    private readonly Mock<IGenericRepository<User>> _users = new();
    private readonly PaymentService _sut;

    private readonly Guid _patientId = Guid.NewGuid();
    private readonly Guid _doctorId = Guid.NewGuid();
    private readonly Guid _appointmentId = Guid.NewGuid();

    public PaymentServiceTests()
    {
        _uow.SetupGet(u => u.Appointments).Returns(_appointments.Object);
        _uow.SetupGet(u => u.Payments).Returns(_payments.Object);
        _uow.SetupGet(u => u.Users).Returns(_users.Object);
        _sut = new PaymentService(_uow.Object);
    }

    private Appointment MakeAppointment(decimal fee, bool isPaid, Guid? patientId = null) => new()
    {
        Id = _appointmentId,
        DoctorId = _doctorId,
        PatientId = patientId ?? _patientId,
        AppointmentTime = DateTime.UtcNow.AddDays(1),
        Fee = fee,
        IsPaid = isPaid,
    };

    [Fact]
    public async Task SimulatePayAsync_AppointmentNotFound_ThrowsKeyNotFoundException()
    {
        _appointments.Setup(r => r.GetByIdAsync(_appointmentId, It.IsAny<CancellationToken>())).ReturnsAsync((Appointment?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _sut.SimulatePayAsync(_patientId, _appointmentId, CancellationToken.None));
    }

    [Fact]
    public async Task SimulatePayAsync_AppointmentBelongsToAnotherPatient_ThrowsUnauthorizedAccessException()
    {
        var appointment = MakeAppointment(fee: 100_000m, isPaid: false, patientId: Guid.NewGuid());
        _appointments.Setup(r => r.GetByIdAsync(_appointmentId, It.IsAny<CancellationToken>())).ReturnsAsync(appointment);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _sut.SimulatePayAsync(_patientId, _appointmentId, CancellationToken.None));

        _payments.Verify(p => p.AddAsync(It.IsAny<Payment>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SimulatePayAsync_AlreadyPaid_ThrowsInvalidOperationException()
    {
        var appointment = MakeAppointment(fee: 100_000m, isPaid: true);
        _appointments.Setup(r => r.GetByIdAsync(_appointmentId, It.IsAny<CancellationToken>())).ReturnsAsync(appointment);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _sut.SimulatePayAsync(_patientId, _appointmentId, CancellationToken.None));

        _payments.Verify(p => p.AddAsync(It.IsAny<Payment>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SimulatePayAsync_FreeAppointment_ThrowsInvalidOperationException()
    {
        var appointment = MakeAppointment(fee: 0m, isPaid: false);
        _appointments.Setup(r => r.GetByIdAsync(_appointmentId, It.IsAny<CancellationToken>())).ReturnsAsync(appointment);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _sut.SimulatePayAsync(_patientId, _appointmentId, CancellationToken.None));

        _payments.Verify(p => p.AddAsync(It.IsAny<Payment>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SimulatePayAsync_ValidPaidAppointment_CreatesCompletedPaymentAndMarksAppointmentPaid()
    {
        var appointment = MakeAppointment(fee: 150_000m, isPaid: false);
        _appointments.Setup(r => r.GetByIdAsync(_appointmentId, It.IsAny<CancellationToken>())).ReturnsAsync(appointment);
        _users.Setup(r => r.GetByIdAsync(_doctorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { Id = _doctorId, FullName = "Bác sĩ Demo" });

        Payment? created = null;
        _payments.Setup(p => p.AddAsync(It.IsAny<Payment>(), It.IsAny<CancellationToken>()))
            .Callback<Payment, CancellationToken>((p, _) => created = p)
            .Returns(Task.CompletedTask);

        var result = await _sut.SimulatePayAsync(_patientId, _appointmentId, CancellationToken.None);

        Assert.NotNull(created);
        Assert.Equal(PaymentStatus.Completed, created!.Status);
        Assert.Equal("Demo", created.Method);
        Assert.Equal(150_000m, created.Amount);
        Assert.NotNull(created.PaidAt);
        Assert.True(appointment.IsPaid);
        Assert.Equal(PaymentStatus.Completed, result.Status);
        Assert.Equal("Bác sĩ Demo", result.DoctorName);
        _appointments.Verify(r => r.Update(appointment), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetMyPaymentsAsync_ReturnsPaymentsOrderedByMostRecentFirst()
    {
        var olderPayment = new Payment
        {
            Id = Guid.NewGuid(), AppointmentId = _appointmentId, PatientId = _patientId, DoctorId = _doctorId,
            Amount = 100_000m, Status = PaymentStatus.Completed, CreatedAt = DateTime.UtcNow.AddDays(-2),
        };
        var newerPayment = new Payment
        {
            Id = Guid.NewGuid(), AppointmentId = _appointmentId, PatientId = _patientId, DoctorId = _doctorId,
            Amount = 200_000m, Status = PaymentStatus.Completed, CreatedAt = DateTime.UtcNow.AddDays(-1),
        };

        _payments.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Payment, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([olderPayment, newerPayment]);
        _appointments.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Appointment, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([MakeAppointment(fee: 100_000m, isPaid: true)]);
        _users.Setup(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([new User { Id = _doctorId, FullName = "Bác sĩ Demo" }]);

        var result = (await _sut.GetMyPaymentsAsync(_patientId, CancellationToken.None)).ToList();

        Assert.Equal(2, result.Count);
        Assert.Equal(newerPayment.Id, result[0].Id);
        Assert.Equal(olderPayment.Id, result[1].Id);
    }
}
