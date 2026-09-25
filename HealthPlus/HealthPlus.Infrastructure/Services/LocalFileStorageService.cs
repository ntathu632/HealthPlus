using HealthPlus.Application.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

namespace HealthPlus.Infrastructure.Services;

public class LocalFileStorageService : IFileStorageService
{
    private readonly string _uploadRoot;
    private readonly string _baseUrl;
    private readonly ILogger<LocalFileStorageService> _logger;

    public LocalFileStorageService(IWebHostEnvironment env, ILogger<LocalFileStorageService> logger)
    {
        _uploadRoot = Path.Combine(env.WebRootPath ?? env.ContentRootPath, "uploads");
        _baseUrl = "/uploads";
        _logger = logger;
        Directory.CreateDirectory(_uploadRoot);
    }

    public async Task<(string Key, string Url)> SaveAsync(Stream stream, string fileName, string folder, CancellationToken ct = default)
    {
        var ext = Path.GetExtension(fileName);
        var key = $"{folder}/{Guid.NewGuid()}{ext}";
        var fullPath = Path.Combine(_uploadRoot, key.Replace('/', Path.DirectorySeparatorChar));

        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);

        await using var fileStream = File.Create(fullPath);
        await stream.CopyToAsync(fileStream, ct);

        _logger.LogInformation("File saved: {Key}", key);
        return (key, GetUrl(key));
    }

    public Task DeleteAsync(string key, CancellationToken ct = default)
    {
        var fullPath = Path.Combine(_uploadRoot, key.Replace('/', Path.DirectorySeparatorChar));
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
            _logger.LogInformation("File deleted: {Key}", key);
        }
        return Task.CompletedTask;
    }

    public string GetUrl(string key) => $"{_baseUrl}/{key}";
}
