using System.ComponentModel.DataAnnotations;

namespace MobilOfis.Web.Models.DTOs;

public class VerifyEmailDto
{
    [Required(ErrorMessage = "Kullanıcı ID zorunludur.")]
    public Guid UserId { get; set; }

    [Required(ErrorMessage = "Doğrulama token zorunludur.")]
    public string VerificationToken { get; set; } = string.Empty;
}

