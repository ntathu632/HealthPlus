using HealthPlus.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Tesseract;

namespace HealthPlus.Infrastructure.Services;

public class TesseractOcrService : IOcrService
{
    private readonly string _tessDataPath;
    private readonly ILogger<TesseractOcrService> _logger;

    public TesseractOcrService(ILogger<TesseractOcrService> logger)
    {
        _tessDataPath = Path.Combine(AppContext.BaseDirectory, "tessdata");
        _logger = logger;
    }

    public Task<(string Text, decimal? Confidence)> ExtractTextAsync(byte[] imageBytes, CancellationToken ct = default)
    {
        if (!Directory.Exists(_tessDataPath))
        {
            _logger.LogWarning("[OCR] Không tìm thấy thư mục tessdata tại {Path}, bỏ qua nhận diện.", _tessDataPath);
            return Task.FromResult<(string, decimal?)>((string.Empty, null));
        }

        return Task.Run(() =>
        {
            try
            {
                using var engine = new TesseractEngine(_tessDataPath, "eng+vie", EngineMode.Default);
                using var img = Pix.LoadFromMemory(imageBytes);
                using var page = engine.Process(img);

                var text = page.GetText()?.Trim() ?? string.Empty;
                var confidence = (decimal)page.GetMeanConfidence();
                return (text, (decimal?)confidence);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[OCR] Nhận diện ảnh thất bại.");
                return (string.Empty, (decimal?)null);
            }
        }, ct);
    }
}
