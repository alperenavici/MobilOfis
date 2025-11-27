using System;
using System.Collections.Generic;

namespace MobilOfis.Web.Models.ViewModels;

public class UserListViewModel
{
    public List<UserViewModel> Users { get; set; } = new();
    public UserFilterViewModel Filters { get; set; } = new();
    public List<DepartmentViewModel> Departments { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;

    public int TotalPages => PageSize == 0 ? 0 : (int)Math.Ceiling((double)TotalCount / PageSize);
    public int CurrentPage => PageNumber;
}

public class UserFilterViewModel
{
    public string? SearchTerm { get; set; }
    public Guid? DepartmentId { get; set; }
    public string? Role { get; set; }
    public bool? IsActive { get; set; }
}

