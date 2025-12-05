namespace MobilOfis.Web.Models.DTOs;

public class CreateLeaveDto
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string LeavesType { get; set; } = string.Empty;
    public string? Reason { get; set; }
}

