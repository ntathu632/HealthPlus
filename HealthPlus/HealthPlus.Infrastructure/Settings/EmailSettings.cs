namespace HealthPlus.Infrastructure.Settings;

public class EmailSettings
{
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 587;
    public bool UseSsl { get; set; } = true;
    public string SenderEmail { get; set; } = string.Empty;
    public string SenderName { get; set; } = "HealthPlus";
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;

    public bool IsConfigured => !string.IsNullOrWhiteSpace(Host) && !string.IsNullOrWhiteSpace(SenderEmail);
}
