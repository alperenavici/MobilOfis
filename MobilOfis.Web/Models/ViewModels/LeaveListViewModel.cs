namespace MobilOfis.Web.Models.ViewModels;

public class LeaveListViewModel
{
    public List<LeaveViewModel> Leaves { get; set; } = new();
    public LeaveFilterViewModel Filters { get; set; } = new();
    public int RemainingLeaveDays { get; set; }
    public int UsedLeaveDays { get; set; }
    public int TotalLeaveDays { get; set; } = 14; // Default yıllık izin hakkı
}

public class LeaveFilterViewModel
{
    public string? Status { get; set; }
    public string? LeavesType { get; set; }
    public int? Year { get; set; }
}

