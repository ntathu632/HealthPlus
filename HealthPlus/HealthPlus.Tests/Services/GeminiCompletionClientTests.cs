using HealthPlus.Infrastructure.Services;
using HealthPlus.Infrastructure.Settings;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace HealthPlus.Tests.Services;

public class GeminiCompletionClientTests
{
    [Fact]
    public async Task GetCompletionAsync_ApiKeyNotConfigured_ReturnsFriendlyFallbackWithoutCallingHttp()
    {
        var settings = new AiChatSettings { ApiKey = "" };
        var sut = new GeminiCompletionClient(
            new HttpClient(),
            Options.Create(settings),
            NullLogger<GeminiCompletionClient>.Instance);

        var reply = await sut.GetCompletionAsync("system prompt", [("user", "Xin chào")]);

        Assert.Contains("chưa được cấu hình", reply);
    }
}
