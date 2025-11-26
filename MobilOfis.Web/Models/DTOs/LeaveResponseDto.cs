using MobilOfis.Entity.Enums;

namespace MobilOfis.Web.Models.DTOs;

public class LeaveResponseDto
{
    public Guid LeavesId { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime RequestDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string LeavesType { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public string? RejectionReason { get; set; }
    public Guid? ManagerApprovalId { get; set; }
    public string? ManagerApprovalName { get; set; }
    public DateTime? ManagerApprovalDate { get; set; }
    public Guid? HRApprovalId { get; set; }
    public DateTime? HRApprovalDate { get; set; }
}

