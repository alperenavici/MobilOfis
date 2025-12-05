namespace MobilOfis.Core.IServices;

public interface IFileStorageService
{
    Task<string?> SavePostImageAsync(Stream fileStream, string fileName, long fileSize, CancellationToken cancellationToken = default);
    Task<string?> SaveProfileImageAsync(Stream fileStream, string fileName, long fileSize, CancellationToken cancellationToken = default);
    Task DeleteAsync(string relativePath);
}

