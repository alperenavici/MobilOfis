using System.ComponentModel.DataAnnotations;

namespace MobilOfis.Web.Models.DTOs;

public class ForgotPasswordDto
{
    [Required(ErrorMessage = "Email alanı zorunludur.")]
    [EmailAddress(ErrorMessage = "Geçerli bir email adresi giriniz.")]
    public string Email { get; set; } = string.Empty;
}

