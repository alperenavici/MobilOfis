using MobilOfis.Entity;

namespace MobilOfis.Core.IServices;

public interface IPostService
{
    Task<Post> CreatePostAsync(
        Guid userId,
        string content,
        Stream? imageStream,
        string? imageFileName,
        long? imageLength,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<Post>> GetLatestPostsAsync(int page, int pageSize);
    Task<Post?> GetPostByIdAsync(Guid postId);
    Task LikePostAsync(Guid postId, Guid userId);
    Task UnlikePostAsync(Guid postId, Guid userId);
    Task AddCommentAsync(Guid postId, Guid userId, string content);
    Task<IEnumerable<PostComment>> GetCommentsAsync(Guid postId);
    Task<bool> IsPostLikedAsync(Guid postId, Guid userId);
    Task<int> GetLikeCountAsync(Guid postId);
    Task<int> GetCommentCountAsync(Guid postId);
}

