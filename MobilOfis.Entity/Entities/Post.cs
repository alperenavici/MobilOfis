namespace MobilOfis.Entity;

public class Post
{
    public Guid PostId { get; set; }
    public Guid UserId { get; set; }
    public string Content { get; set; } = null!;
    public string? ImagePath { get; set; }
    public DateTime CreatedAt { get; set; }

    public virtual User? User { get; set; }
    public virtual ICollection<PostLike> Likes { get; set; } = new List<PostLike>();
    public virtual ICollection<PostComment> Comments { get; set; } = new List<PostComment>();
}

