using System.ComponentModel.DataAnnotations;

namespace MobilOfis.Web.Models.DTOs;

public class ResetPasswordDto
{
    [Required(ErrorMessage = "Email alanı zorunludur.")]
    [EmailAddress(ErrorMessage = "Geçerli bir email adresi giriniz.")]
    public string Email { get; set; }

    [Required(ErrorMessage = "Reset token zorunludur.")]
    public string ResetToken { get; set; }

    [Required(ErrorMessage = "Yeni şifre zorunludur.")]
    [MinLength(6, ErrorMessage = "Şifre en az 6 karakter olmalıdır.")]
    public string NewPassword { get; set; }
}

