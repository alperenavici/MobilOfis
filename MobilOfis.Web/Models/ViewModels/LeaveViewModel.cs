namespace MobilOfis.Web.Models.ViewModels;

public class LeaveViewModel
{
    public Guid LeavesId { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime RequestDate { get; set; }
    public string Status { get; set; }
    public string LeavesType { get; set; }
    public string? Reason { get; set; }
    public string? RejectionReason { get; set; }
    public Guid? ManagerApprovalId { get; set; }
    public string? ManagerApprovalName { get; set; }
    public DateTime? ManagerApprovalDate { get; set; }
    public Guid? HRApprovalId { get; set; }
    public DateTime? HRApprovalDate { get; set; }
    
    public string StatusDisplay => GetStatusDisplay();
    public string StatusBadgeColor => GetStatusBadgeColor();
    public string LeavesTypeDisplay => GetLeavesTypeDisplay();
    public int DayCount => (EndDate - StartDate).Days + 1;
    
    private string GetStatusDisplay()
    {
        return Status switch
        {
            "Pending" => "Bekliyor",
            "ManagerApproved" => "Manager Onaylı",
            "Approved" => "Onaylandı",
            "Rejected" => "Reddedildi",
            _ => Status
        };
    }
    
    private string GetStatusBadgeColor()
    {
        return Status switch
        {
            "Pending" => "warning",
            "ManagerApproved" => "info",
            "Approved" => "success",
            "Rejected" => "danger",
            _ => "secondary"
        };
    }
    
    private string GetLeavesTypeDisplay()
    {
        return LeavesType switch
        {
            "YillikUcretliIzin" => "Yıllık Ücretli İzin",
            "EvlilikIzni" => "Evlilik İzni",
            "BabalikIzni" => "Babalık İzni",
            "OlumIzni" => "Ölüm İzni",
            "HastalikIzni" => "Hastalık İzni",
            "UcretsizIzin" => "Ücretsiz İzin",
            _ => LeavesType
        };
    }
}

