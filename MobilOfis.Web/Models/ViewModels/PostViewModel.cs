namespace MobilOfis.Web.Models.ViewModels;

public class PostViewModel
{
    public Guid PostId { get; set; }
    public string Content { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public string? AuthorRole { get; set; }
    public string? AuthorAvatar { get; set; }
    public bool IsOwner { get; set; }

    public string CreatedAtDisplay => CreatedAt.ToLocalTime().ToString("dd MMM yyyy HH:mm");
}

