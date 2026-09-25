namespace HealthPlus.Application.Interfaces;

public interface IOcrService
{
    Task<(string Text, decimal? Confidence)> ExtractTextAsync(byte[] imageBytes, CancellationToken ct = default);
}
