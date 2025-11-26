using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MobilOfis.Web.Models.ViewModels;

public class DepartmentViewModel
{
    public Guid DepartmentId { get; set; }
    
    [Required(ErrorMessage = "Departman adı gereklidir")]
    [StringLength(100)]
    [Display(Name = "Departman Adı")]
    public string DepartmentName { get; set; } = string.Empty;
    public string Name => DepartmentName;
    
    public Guid? ManagerId { get; set; }
    
    [Display(Name = "Departman Yöneticisi")]
    public string? ManagerName { get; set; }
    
    public int EmployeeCount { get; set; }
    public List<UserViewModel> Employees { get; set; } = new();
    public string? Description { get; set; }
    public DateTime? CreatedAt { get; set; }
    public int ActiveLeavesCount { get; set; }
    public int UpcomingEventsCount { get; set; }
}

