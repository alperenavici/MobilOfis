namespace MobilOfis.Web.Models.DTOs;

public class CreateLeaveDto
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string LeavesType { get; set; }
    public string? Reason { get; set; }
}

