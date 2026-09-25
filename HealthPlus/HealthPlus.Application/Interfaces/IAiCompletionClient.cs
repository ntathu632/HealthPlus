namespace HealthPlus.Application.Interfaces;

// Trừu tượng hoá lời gọi tới nhà cung cấp AI thật (Gemini API) — để AiChatService không phụ thuộc
// trực tiếp vào chi tiết HTTP/JSON của một hãng cụ thể.
public interface IAiCompletionClient
{
    Task<string> GetCompletionAsync(string systemPrompt, IEnumerable<(string Role, string Content)> history, CancellationToken ct = default);
}
