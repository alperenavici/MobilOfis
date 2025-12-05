using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MobilOfis.Core.IServices;
using MobilOfis.Core.Options;

namespace MobilOfis.Service.FileStorage;

public class FileStorageService : IFileStorageService
{
    private readonly IWebHostEnvironment _environment;
    private readonly FileStorageOptions _options;
    private readonly ILogger<FileStorageService> _logger;

    public FileStorageService(
        IWebHostEnvironment environment,
        IOptions<FileStorageOptions> options,
        ILogger<FileStorageService> logger)
    {
        _environment = environment;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<string?> SavePostImageAsync(Stream fileStream, string fileName, long fileSize, CancellationToken cancellationToken = default)
    {
        if (fileStream == null || fileSize <= 0)
        {
            return null;
        }

        if (fileSize > _options.MaxFileSizeBytes)
        {
            throw new InvalidOperationException($"Dosya boyutu {_options.MaxFileSizeBytes / (1024 * 1024)} MB sınırını aşıyor.");
        }

        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(extension) || !_options.AllowedImageExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Desteklenmeyen dosya uzantısı.");
        }

        var uploadsRoot = EnsurePostsDirectoryExists();
        var uniqueFileName = $"{Guid.NewGuid():N}{extension}";
        var destinationPath = Path.Combine(uploadsRoot, uniqueFileName);

        await using (var destinationStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None, 81920, true))
        {
            await fileStream.CopyToAsync(destinationStream, cancellationToken);
        }

        var relativePath = Path.Combine(_options.PostsFolder, uniqueFileName)
            .Replace("\\", "/");

        _logger.LogInformation("Post image saved to {RelativePath}", relativePath);

        return $"/{relativePath.TrimStart('/')}";
    }

    public async Task<string?> SaveProfileImageAsync(Stream fileStream, string fileName, long fileSize, CancellationToken cancellationToken = default)
    {
        if (fileStream == null || fileSize <= 0)
        {
            return null;
        }

        if (fileSize > _options.MaxFileSizeBytes)
        {
            throw new InvalidOperationException($"Dosya boyutu {_options.MaxFileSizeBytes / (1024 * 1024)} MB sınırını aşıyor.");
        }

        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(extension) || !_options.AllowedImageExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Desteklenmeyen dosya uzantısı.");
        }

        var uploadsRoot = EnsureProfilesDirectoryExists();
        var uniqueFileName = $"{Guid.NewGuid():N}{extension}";
        var destinationPath = Path.Combine(uploadsRoot, uniqueFileName);

        await using (var destinationStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None, 81920, true))
        {
            await fileStream.CopyToAsync(destinationStream, cancellationToken);
        }

        var relativePath = Path.Combine(_options.ProfilesFolder, uniqueFileName)
            .Replace("\\", "/");

        _logger.LogInformation("Profile image saved to {RelativePath}", relativePath);

        return $"/{relativePath.TrimStart('/')}";
    }

    public Task DeleteAsync(string relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
        {
            return Task.CompletedTask;
        }

        try
        {
            var normalized = relativePath.TrimStart('/', '\\');
            var fullPath = Path.Combine(GetWebRootPath(), normalized.Replace("/", Path.DirectorySeparatorChar.ToString()));

            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
                _logger.LogInformation("Deleted stored file {RelativePath}", relativePath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Dosya silinirken hata oluştu: {RelativePath}", relativePath);
        }

        return Task.CompletedTask;
    }

    private string EnsurePostsDirectoryExists()
    {
        var postsPath = Path.Combine(GetWebRootPath(), _options.PostsFolder.Replace("/", Path.DirectorySeparatorChar.ToString()));

        if (!Directory.Exists(postsPath))
        {
            Directory.CreateDirectory(postsPath);
        }

        return postsPath;
    }

    private string EnsureProfilesDirectoryExists()
    {
        var profilesPath = Path.Combine(GetWebRootPath(), _options.ProfilesFolder.Replace("/", Path.DirectorySeparatorChar.ToString()));

        if (!Directory.Exists(profilesPath))
        {
            Directory.CreateDirectory(profilesPath);
        }

        return profilesPath;
    }

    private string GetWebRootPath()
    {
        if (!string.IsNullOrWhiteSpace(_environment.WebRootPath))
        {
            return _environment.WebRootPath!;
        }

        var fallback = Path.Combine(_environment.ContentRootPath, "wwwroot");
        if (!Directory.Exists(fallback))
        {
            Directory.CreateDirectory(fallback);
        }

        return fallback;
    }
}

