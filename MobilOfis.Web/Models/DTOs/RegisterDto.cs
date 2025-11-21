using System.ComponentModel.DataAnnotations;

namespace MobilOfis.Web.Models.DTOs;

public class RegisterDto
{
    [Required(ErrorMessage = "Ad alanı zorunludur.")]
    public string FirstName { get; set; }

    [Required(ErrorMessage = "Soyad alanı zorunludur.")]
    public string LastName { get; set; }

    [Required(ErrorMessage = "Email alanı zorunludur.")]
    [EmailAddress(ErrorMessage = "Geçerli bir email adresi giriniz.")]
    public string Email { get; set; }

    [Required(ErrorMessage = "Şifre alanı zorunludur.")]
    [MinLength(6, ErrorMessage = "Şifre en az 6 karakter olmalıdır.")]
    public string Password { get; set; }

    [Required(ErrorMessage = "Telefon numarası zorunludur.")]
    public string PhoneNumber { get; set; }
}

