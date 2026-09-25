using HealthPlus.Infrastructure.Services;
using Xunit;

namespace HealthPlus.Tests.Services;

public class PasswordServiceTests
{
    private readonly PasswordService _service = new();

    [Fact]
    public void Hash_ThenVerify_SamePassword_ReturnsTrue()
    {
        var hash = _service.Hash("MySecret1");

        Assert.True(_service.Verify("MySecret1", hash));
    }

    [Fact]
    public void Verify_WrongPassword_ReturnsFalse()
    {
        var hash = _service.Hash("MySecret1");

        Assert.False(_service.Verify("WrongPassword", hash));
    }

    [Fact]
    public void Hash_SamePasswordTwice_ProducesDifferentHashes()
    {
        // BCrypt salts each hash randomly — this guards against someone
        // "optimizing" Hash() into something deterministic (e.g. plain SHA256).
        var hash1 = _service.Hash("MySecret1");
        var hash2 = _service.Hash("MySecret1");

        Assert.NotEqual(hash1, hash2);
        Assert.True(_service.Verify("MySecret1", hash1));
        Assert.True(_service.Verify("MySecret1", hash2));
    }
}
