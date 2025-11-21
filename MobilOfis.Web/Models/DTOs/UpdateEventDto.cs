namespace MobilOfis.Web.Models.DTOs;

public class UpdateEventDto
{
    public string Title { get; set; }
    public string Description { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string? Location { get; set; }
}

