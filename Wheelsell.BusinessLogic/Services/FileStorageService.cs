using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Wheelsell.BusinessLogic.Settings;

namespace Wheelsell.BusinessLogic.Services;

public interface IFileStorageService
{
    Task<string> SaveFileAsync(IFormFile file, string subFolder);
    void DeleteFile(string relativePath);
}

public class FileStorageService : IFileStorageService
{
    private readonly FileStorageSettings _settings;

    public FileStorageService(IOptions<FileStorageSettings> settings)
    {
        _settings = settings.Value;
    }

    public async Task<string> SaveFileAsync(IFormFile file, string subFolder)
    {
        var folderPath = Path.Combine(_settings.RootPath, subFolder);
        Directory.CreateDirectory(folderPath);

        var extension = Path.GetExtension(file.FileName);
        var fileName = $"{Guid.NewGuid()}{extension}";
        var fullPath = Path.Combine(folderPath, fileName);

        await using var stream = new FileStream(fullPath, FileMode.Create);
        await file.CopyToAsync(stream);

        return $"{_settings.BaseUrl}/{subFolder}/{fileName}".Replace("\\", "/");
    }

    public void DeleteFile(string relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
        {
            return;
        }

        var trimmed = relativePath.TrimStart('/');
        var withoutUploadsPrefix = trimmed.StartsWith("uploads/")
            ? trimmed["uploads/".Length..]
            : trimmed;

        var fullPath = Path.Combine(_settings.RootPath, withoutUploadsPrefix);
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }
    }
}
