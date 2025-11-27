using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MobilOfis.Web.Models.ViewModels;

public class UserViewModel
{
    public Guid UserId { get; set; }
    
    [Required(ErrorMessage = "Ad gereklidir")]
    [Display(Name = "Ad")]
    public string FirstName { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Soyad gereklidir")]
    [Display(Name = "Soyad")]
    public string LastName { get; set; } = string.Empty;
    
    public string FullName => $"{FirstName} {LastName}";
    
    [Required(ErrorMessage = "Email gereklidir")]
    [EmailAddress]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;
    
    [Phone]
    [Display(Name = "Telefon")]
    public string? PhoneNumber { get; set; }
    
    [Display(Name = "Profil Fotoğrafı")]
    public string? ProfilePictureUrl { get; set; }
    
    [Display(Name = "İş Ünvanı")]
    public string? JobTitle { get; set; }
    
    [Display(Name = "İşe Başlama Tarihi")]
    public DateTime? HireDate { get; set; }
    
    [Display(Name = "Maaş")]
    public decimal? Salary { get; set; }
    
    public Guid? DepartmentId { get; set; }
    
    [Display(Name = "Departman")]
    public string? DepartmentName { get; set; }
    
    public Guid? ManagerId { get; set; }
    
    [Display(Name = "Yönetici")]
    public string? ManagerName { get; set; }
    
    [Display(Name = "Aktif")]
    public bool IsActive { get; set; }
    
    [Display(Name = "Rol")]
    public string? Role { get; set; }
    public string RoleName => string.IsNullOrWhiteSpace(Role) ? "Employee" : Role!;
    public bool IsManager => string.Equals(RoleName, "Manager", StringComparison.OrdinalIgnoreCase);
    
    [Display(Name = "Kayıt Tarihi")]
    public DateTime CreatedDate { get; set; }
    public DateTime CreatedAt => CreatedDate;
    
    [Display(Name = "Son Giriş")]
    public DateTime? LastLoginDate { get; set; }
    
    // Adres Bilgileri
    [Display(Name = "Adres")]
    public string? Address { get; set; }
    
    [Display(Name = "Şehir")]
    public string? City { get; set; }
    
    [Display(Name = "Ülke")]
    public string? Country { get; set; }
    
    [Display(Name = "Posta Kodu")]
    public string? PostalCode { get; set; }
    
    [Display(Name = "Doğum Tarihi")]
    public DateTime? DateOfBirth { get; set; }
    public DateTime? BirthDate
    {
        get => DateOfBirth;
        set => DateOfBirth = value;
    }
    
    [Display(Name = "Acil Durum İletişim")]
    public string? EmergencyContactName { get; set; }
    
    [Display(Name = "Acil Durum Telefon")]
    public string? EmergencyContactPhone { get; set; }

    public int RemainingLeaveDays { get; set; }
    public int UsedLeaveDays { get; set; }
    public int EventsAttended { get; set; }
    public List<LeaveViewModel> RecentLeaves { get; set; } = new();
}

