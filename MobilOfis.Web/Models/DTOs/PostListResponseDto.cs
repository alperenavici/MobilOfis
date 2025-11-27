namespace MobilOfis.Web.Models.DTOs;

public class PostListResponseDto
{
    public IEnumerable<PostResponseDto> Posts { get; set; } = new List<PostResponseDto>();
    public bool HasMore { get; set; }
}

