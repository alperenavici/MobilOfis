namespace MobilOfis.Web.Models.DTOs;

public class RejectLeaveDto
{
    public Guid LeaveId { get; set; }
    public string RejectionReason { get; set; } = string.Empty;
}

