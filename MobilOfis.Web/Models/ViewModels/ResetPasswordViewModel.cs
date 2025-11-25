using System.ComponentModel.DataAnnotations;

namespace MobilOfis.Web.Models.ViewModels;

public class ResetPasswordViewModel
{
    [Required(ErrorMessage = "Email adresi gereklidir")]
    [EmailAddress]
    public string Email { get; set; }
    
    [Required(ErrorMessage = "Reset token gereklidir")]
    public string ResetToken { get; set; }
    
    [Required(ErrorMessage = "Yeni şifre gereklidir")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Şifre en az 6 karakter olmalıdır")]
    [DataType(DataType.Password)]
    [Display(Name = "Yeni Şifre")]
    public string NewPassword { get; set; }
    
    [Required(ErrorMessage = "Şifre tekrarı gereklidir")]
    [Compare("NewPassword", ErrorMessage = "Şifreler eşleşmiyor")]
    [DataType(DataType.Password)]
    [Display(Name = "Şifre Tekrar")]
    public string ConfirmPassword { get; set; }
}

