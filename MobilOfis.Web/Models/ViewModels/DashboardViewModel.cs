namespace MobilOfis.Web.Models.ViewModels;

public class DashboardViewModel
{
    public UserViewModel CurrentUser { get; set; } = null!;
    
    // Statistikler
    public int PendingLeavesCount { get; set; }
    public int ApprovedLeavesCount { get; set; }
    public int UpcomingEventsCount { get; set; }
    public int UnreadNotificationsCount { get; set; }
    
    // Listeler
    public List<LeaveViewModel> RecentLeaves { get; set; } = new();
    public List<EventViewModel> UpcomingEvents { get; set; } = new();
    public List<NotificationViewModel> RecentNotifications { get; set; } = new();
    
    // Manager için ek
    public List<LeaveViewModel> PendingApprovals { get; set; } = new();
    public List<UserViewModel> TeamMembers { get; set; } = new();
    
    // HR için ek
    public int TotalEmployees { get; set; }
    public int ActiveEmployees { get; set; }
    public Dictionary<string, int> DepartmentDistribution { get; set; } = new();
}

