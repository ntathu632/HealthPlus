using HealthPlus.Infrastructure.Services;
using Xunit;

namespace HealthPlus.Tests.Services;

public class TwilioSmsSenderTests
{
    [Theory]
    [InlineData("0912345678", "+84912345678")]
    [InlineData("0987654321", "+84987654321")]
    public void ToE164_LocalVietnameseFormat_ConvertsLeadingZeroToCountryCode(string input, string expected)
    {
        Assert.Equal(expected, TwilioSmsSender.ToE164(input));
    }

    [Fact]
    public void ToE164_AlreadyE164_LeftUnchanged()
    {
        Assert.Equal("+84912345678", TwilioSmsSender.ToE164("+84912345678"));
    }

    [Fact]
    public void ToE164_NoLeadingZeroOrPlus_PrependsCountryCode()
    {
        // Defensive fallback — shouldn't normally happen given RegisterRequestValidator's
        // ^[0-9]{10,11}$ rule always produces a leading 0, but don't silently mangle input either way.
        Assert.Equal("+84912345678", TwilioSmsSender.ToE164("912345678"));
    }
}
