using System.Net.Http.Json;
using System.Text.Json;
using HealthPlus.Application.Interfaces;
using HealthPlus.Infrastructure.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HealthPlus.Infrastructure.Services;

// Gọi Google Gemini API thật (generateContent, gói miễn phí). Nếu chưa cấu hình ApiKey (appsettings:AiChat),
// trả về câu trả lời mặc định thay vì lỗi — giống cách TwilioSmsSender/SmtpEmailSender xử lý khi chưa cấu hình.
public class GeminiCompletionClient : IAiCompletionClient
{
    private readonly HttpClient _http;
    private readonly AiChatSettings _settings;
    private readonly ILogger<GeminiCompletionClient> _logger;

    public GeminiCompletionClient(HttpClient http, IOptions<AiChatSettings> settings, ILogger<GeminiCompletionClient> logger)
    {
        _http = http;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<string> GetCompletionAsync(string systemPrompt, IEnumerable<(string Role, string Content)> history, CancellationToken ct = default)
    {
        if (!_settings.IsConfigured)
        {
            _logger.LogWarning("[AiChat] Chưa cấu hình API key (appsettings:AiChat:ApiKey).");
            return "Tính năng trò chuyện AI hiện chưa được cấu hình API key. Vui lòng liên hệ quản trị viên hệ thống.";
        }

        try
        {
            var payload = new
            {
                system_instruction = new { parts = new[] { new { text = systemPrompt } } },
                contents = history.Select(h => new
                {
                    role = h.Role == "assistant" ? "model" : "user",
                    parts = new[] { new { text = h.Content } },
                }).ToArray(),
                generationConfig = new { maxOutputTokens = _settings.MaxTokens },
            };

            using var request = new HttpRequestMessage(HttpMethod.Post, $"v1beta/models/{_settings.Model}:generateContent")
            {
                Content = JsonContent.Create(payload),
            };
            request.Headers.Add("x-goog-api-key", _settings.ApiKey);

            using var response = await _http.SendAsync(request, ct);
            var body = await response.Content.ReadAsStringAsync(ct);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("[AiChat] Gemini API trả lỗi {Status}: {Body}", response.StatusCode, body);
                return "Xin lỗi, trợ lý AI đang gặp sự cố. Vui lòng thử lại sau.";
            }

            using var doc = JsonDocument.Parse(body);
            var text = doc.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();
            return string.IsNullOrWhiteSpace(text) ? "Xin lỗi, tôi chưa thể trả lời câu hỏi này." : text;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[AiChat] Gọi Gemini API thất bại.");
            return "Xin lỗi, trợ lý AI đang gặp sự cố. Vui lòng thử lại sau.";
        }
    }
}
