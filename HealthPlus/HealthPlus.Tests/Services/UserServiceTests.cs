using System.Linq.Expressions;
using HealthPlus.Application.DTOs.Users;
using HealthPlus.Application.Interfaces;
using HealthPlus.Application.Services;
using HealthPlus.Domain.Entities;
using HealthPlus.Domain.Interfaces.Repositories;
using Moq;
using Xunit;

namespace HealthPlus.Tests.Services;

public class UserServiceTests
{
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly Mock<IGenericRepository<User>> _users = new();
    private readonly Mock<IGenericRepository<UserRole>> _userRoles = new();
    private readonly Mock<IGenericRepository<Role>> _roles = new();
    private readonly Mock<IPasswordService> _password = new();
    private readonly UserService _sut;

    public UserServiceTests()
    {
        _uow.SetupGet(u => u.Users).Returns(_users.Object);
        _uow.SetupGet(u => u.UserRoles).Returns(_userRoles.Object);
        _uow.SetupGet(u => u.Roles).Returns(_roles.Object);
        _userRoles.Setup(r => r.FindAsync(It.IsAny<Expression<Func<UserRole, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        _roles.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Role, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        _sut = new UserService(_uow.Object, _password.Object);
    }

    private static User MakeUser(Guid id) => new()
    {
        Id = id,
        Email = "user@example.com",
        FullName = "Nguyen Van A",
        PhoneNumber = "0912345678",
        PasswordHash = "old-hash",
    };

    [Fact]
    public async Task GetByIdAsync_UnknownUser_ThrowsKeyNotFoundException()
    {
        var userId = Guid.NewGuid();
        _users.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.GetByIdAsync(userId));
    }

    [Fact]
    public async Task GetByIdAsync_ExistingUser_MapsFieldsCorrectly()
    {
        var userId = Guid.NewGuid();
        var user = MakeUser(userId);
        _users.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(user);

        var result = await _sut.GetByIdAsync(userId);

        Assert.Equal(user.Email, result.Email);
        Assert.Equal(user.FullName, result.FullName);
        Assert.Equal(user.PhoneNumber, result.PhoneNumber);
    }

    [Fact]
    public async Task UpdateProfileAsync_UpdatesFullNameAndPhoneNumber()
    {
        var userId = Guid.NewGuid();
        var user = MakeUser(userId);
        _users.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(user);

        var request = new UpdateUserRequest { FullName = "  Nguyen Van B  ", PhoneNumber = " 0987654321 " };
        var result = await _sut.UpdateProfileAsync(userId, request);

        Assert.Equal("Nguyen Van B", result.FullName);
        Assert.Equal("0987654321", result.PhoneNumber);
        _users.Verify(r => r.Update(It.Is<User>(u => u.FullName == "Nguyen Van B")), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateProfileAsync_UnknownUser_ThrowsKeyNotFoundException()
    {
        var userId = Guid.NewGuid();
        _users.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _sut.UpdateProfileAsync(userId, new UpdateUserRequest { FullName = "X" }));
    }

    [Fact]
    public async Task ChangePasswordAsync_WrongCurrentPassword_ThrowsInvalidOperationException()
    {
        var userId = Guid.NewGuid();
        var user = MakeUser(userId);
        _users.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _password.Setup(p => p.Verify("wrong", "old-hash")).Returns(false);

        var request = new ChangePasswordRequest { CurrentPassword = "wrong", NewPassword = "NewPass1" };

        await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.ChangePasswordAsync(userId, request));
        _users.Verify(r => r.Update(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task ChangePasswordAsync_CorrectCurrentPassword_HashesAndSavesNewPassword()
    {
        var userId = Guid.NewGuid();
        var user = MakeUser(userId);
        _users.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _password.Setup(p => p.Verify("old-pass", "old-hash")).Returns(true);
        _password.Setup(p => p.Hash("NewPass1")).Returns("new-hash");

        var request = new ChangePasswordRequest { CurrentPassword = "old-pass", NewPassword = "NewPass1" };
        await _sut.ChangePasswordAsync(userId, request);

        Assert.Equal("new-hash", user.PasswordHash);
        _users.Verify(r => r.Update(It.Is<User>(u => u.PasswordHash == "new-hash")), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAvatarAsync_SetsAvatarUrl()
    {
        var userId = Guid.NewGuid();
        var user = MakeUser(userId);
        _users.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(user);

        await _sut.UpdateAvatarAsync(userId, "/uploads/avatars/new.png");

        Assert.Equal("/uploads/avatars/new.png", user.AvatarUrl);
        _users.Verify(r => r.Update(It.Is<User>(u => u.AvatarUrl == "/uploads/avatars/new.png")), Times.Once);
    }
}
