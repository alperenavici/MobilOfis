using System.ComponentModel.DataAnnotations;

namespace MobilOfis.Web.Models.ViewModels;

public class LoginViewModel
{
    [Required(ErrorMessage = "Email adresi gereklidir")]
    [EmailAddress(ErrorMessage = "Geçerli bir email adresi giriniz")]
    [Display(Name = "Email Adresi")]
    public string Email { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Şifre gereklidir")]
    [DataType(DataType.Password)]
    [Display(Name = "Şifre")]
    public string Password { get; set; } = string.Empty;
    
    [Display(Name = "Beni Hatırla")]
    public bool RememberMe { get; set; }
    
    public string? ReturnUrl { get; set; }
}

