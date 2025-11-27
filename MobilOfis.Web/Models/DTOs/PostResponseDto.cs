namespace MobilOfis.Web.Models.DTOs;

public class PostResponseDto
{
    public Guid PostId { get; set; }
    public string Content { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public string? AuthorRole { get; set; }
    public string? AuthorAvatar { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedAtDisplay { get; set; } = string.Empty;
    public bool IsOwner { get; set; }
    public int LikeCount { get; set; }
    public int CommentCount { get; set; }
    public bool IsLiked { get; set; }
}

