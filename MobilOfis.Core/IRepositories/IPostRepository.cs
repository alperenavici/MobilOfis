using MobilOfis.Entity;

namespace MobilOfis.Core.IRepositories;

public interface IPostRepository : IGenericRepository<Post>
{
    Task<IEnumerable<Post>> GetLatestPostsAsync(int page, int pageSize);
    Task<Post?> GetByIdWithUserAsync(Guid postId);
    Task AddLikeAsync(PostLike like);
    Task RemoveLikeAsync(Guid postId, Guid userId);
    Task AddCommentAsync(PostComment comment);
    Task<IEnumerable<PostComment>> GetCommentsByPostIdAsync(Guid postId);
    Task<bool> IsPostLikedByUserAsync(Guid postId, Guid userId);
    Task<int> GetLikeCountAsync(Guid postId);
    Task<int> GetCommentCountAsync(Guid postId);
}

