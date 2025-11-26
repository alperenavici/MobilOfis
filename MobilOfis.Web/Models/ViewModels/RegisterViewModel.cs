using System.ComponentModel.DataAnnotations;

namespace MobilOfis.Web.Models.ViewModels;

public class RegisterViewModel
{
    [Required(ErrorMessage = "Ad gereklidir")]
    [StringLength(50)]
    [Display(Name = "Ad")]
    public string FirstName { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Soyad gereklidir")]
    [StringLength(50)]
    [Display(Name = "Soyad")]
    public string LastName { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Email gereklidir")]
    [EmailAddress(ErrorMessage = "Geçerli bir email adresi giriniz")]
    [Display(Name = "Email Adresi")]
    public string Email { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Telefon numarası gereklidir")]
    [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz")]
    [Display(Name = "Telefon Numarası")]
    public string PhoneNumber { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Şifre gereklidir")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Şifre en az 6 karakter olmalıdır")]
    [DataType(DataType.Password)]
    [Display(Name = "Şifre")]
    public string Password { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Şifre tekrarı gereklidir")]
    [Compare("Password", ErrorMessage = "Şifreler eşleşmiyor")]
    [DataType(DataType.Password)]
    [Display(Name = "Şifre Tekrar")]
    public string ConfirmPassword { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Kullanım şartlarını kabul etmelisiniz")]
    [Display(Name = "Kullanım şartlarını kabul ediyorum")]
    public bool AcceptTerms { get; set; }
}

