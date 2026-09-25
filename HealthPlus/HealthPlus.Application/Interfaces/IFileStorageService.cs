namespace HealthPlus.Application.Interfaces;

public interface IFileStorageService
{
    Task<(string Key, string Url)> SaveAsync(Stream stream, string fileName, string folder, CancellationToken ct = default);
    Task DeleteAsync(string key, CancellationToken ct = default);
    string GetUrl(string key);
}
