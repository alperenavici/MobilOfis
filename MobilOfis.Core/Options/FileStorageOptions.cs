namespace MobilOfis.Core.Options;

public class FileStorageOptions
{
    public string PostsFolder { get; set; } = "uploads/posts";
    public long MaxFileSizeBytes { get; set; } = 5 * 1024 * 1024; // 5 MB
    public string[] AllowedImageExtensions { get; set; } = [".jpg", ".jpeg", ".png", ".gif"];
}

