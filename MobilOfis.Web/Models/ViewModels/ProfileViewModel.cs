using System;
using System.ComponentModel.DataAnnotations;

namespace MobilOfis.Web.Models.ViewModels;

public class ProfileViewModel
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
    
    [Display(Name = "Profil Fotoğrafı URL")]
    public string? ProfilePictureUrl { get; set; }
    public string? ProfilePhotoUrl
    {
        get => ProfilePictureUrl;
        set => ProfilePictureUrl = value;
    }
    
    [Display(Name = "İş Ünvanı")]
    public string? JobTitle { get; set; }
    
    [Display(Name = "Departman")]
    public string? DepartmentName { get; set; }
    
    [Display(Name = "Yönetici")]
    public string? ManagerName { get; set; }
    
    [Display(Name = "Rol")]
    public string? Role { get; set; }
    public string RoleName => Role ?? "Employee";
    
    [Display(Name = "İşe Başlama Tarihi")]
    public DateTime? HireDate { get; set; }
    public DateTime? JoinDate
    {
        get => HireDate;
        set => HireDate = value;
    }
    
    [Display(Name = "Son Giriş")]
    public DateTime? LastLoginDate { get; set; }
    
    public DateTime? BirthDate { get; set; }
    public string? Address { get; set; }
    
    // Şifre Değiştirme
    [DataType(DataType.Password)]
    [Display(Name = "Mevcut Şifre")]
    public string? CurrentPassword { get; set; }
    
    [DataType(DataType.Password)]
    [Display(Name = "Yeni Şifre")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Şifre en az 6 karakter olmalıdır")]
    public string? NewPassword { get; set; }
    
    [DataType(DataType.Password)]
    [Compare("NewPassword", ErrorMessage = "Şifreler eşleşmiyor")]
    [Display(Name = "Yeni Şifre Tekrar")]
    public string? ConfirmNewPassword { get; set; }
    
    // Bildirim Tercihleri
    [Display(Name = "Email Bildirimleri")]
    public bool EmailNotifications { get; set; } = true;
    
    [Display(Name = "Push Bildirimleri")]
    public bool PushNotifications { get; set; } = true;
    
    public bool TwoFactorEnabled { get; set; }
    public bool LeaveNotifications { get; set; } = true;
    public bool EventNotifications { get; set; } = true;
    public bool SalaryNotifications { get; set; } = true;
}

