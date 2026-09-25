namespace HealthPlus.Infrastructure.Settings;

public class AiChatSettings
{
    public string Provider { get; set; } = "Gemini";
    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = "gemini-flash-latest";
    public int MaxTokens { get; set; } = 1024;

    public bool IsConfigured => !string.IsNullOrWhiteSpace(ApiKey);
}
