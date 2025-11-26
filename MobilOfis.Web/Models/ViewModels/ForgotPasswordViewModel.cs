using System.ComponentModel.DataAnnotations;

namespace MobilOfis.Web.Models.ViewModels;

public class ForgotPasswordViewModel
{
    [Required(ErrorMessage = "Email adresi gereklidir")]
    [EmailAddress(ErrorMessage = "Geçerli bir email adresi giriniz")]
    [Display(Name = "Email Adresi")]
    public string Email { get; set; } = string.Empty;
}

