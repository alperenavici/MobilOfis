namespace MobilOfis.Web.Models.DTOs;

public class EventResponseDto
{
    public Guid EventId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string? Location { get; set; }
    public Guid CreatedByUserId { get; set; }
    public string CreatedByName { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public List<ParticipantDto>? Participants { get; set; }
}

public class ParticipantDto
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
}

