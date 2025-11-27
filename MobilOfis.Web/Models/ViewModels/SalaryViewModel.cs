using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace MobilOfis.Web.Models.ViewModels;

public class SalaryViewModel
{
    public Guid UserId { get; set; }
    public Guid? SalaryId { get; set; }
    
    [Display(Name = "Çalışan")]
    public string UserName { get; set; } = string.Empty;
    
    [Display(Name = "Departman")]
    public string? DepartmentName { get; set; }
    
    [Display(Name = "Pozisyon")]
    public string? JobTitle { get; set; }
    
    [Display(Name = "Brüt Maaş")]
    [DataType(DataType.Currency)]
    public decimal GrossSalary { get; set; }
    
    [Display(Name = "Net Maaş")]
    [DataType(DataType.Currency)]
    public decimal NetSalary { get; set; }
    
    [Display(Name = "Prim")]
    public decimal? Bonus { get; set; }
    
    [Display(Name = "Kesintiler")]
    [DataType(DataType.Currency)]
    public decimal? Deductions { get; set; }
    
    [Display(Name = "Geçerlilik Tarihi")]
    public DateTime? EffectiveDate { get; set; }
    
    [Display(Name = "Son Güncelleme")]
    public DateTime? LastUpdated { get; set; }
    
    public List<SalaryPaymentViewModel> PaymentHistory { get; set; } = new();
    public List<TeamMemberSalaryViewModel> TeamMembers { get; set; } = new();
    public bool CanManageTeam => TeamMembers?.Any() == true;
}

public class SalaryPaymentViewModel
{
    public Guid SalaryId { get; set; }
    public DateTime Period { get; set; }
    public decimal GrossSalary { get; set; }
    public decimal NetSalary { get; set; }
    public decimal? Bonus { get; set; }
    public DateTime PaymentDate { get; set; }
}

public class TeamMemberSalaryViewModel
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string? DepartmentName { get; set; }
    public string? JobTitle { get; set; }
    public decimal Salary { get; set; }
    public DateTime? LastUpdated { get; set; }
}

