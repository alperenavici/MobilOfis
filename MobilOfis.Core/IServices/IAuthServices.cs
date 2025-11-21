using MobilOfis.Entity;

namespace MobilOfis.Core.IServices;

public interface IAuthServices
{
    // Kullanıcı Kaydı
    Task<User> RegisterAsync(string firstName, string lastName, string email, string password, string phoneNumber);
    
    // Kullanıcı Girişi
    Task<User> LoginAsync(string email, string password);
    
    // Token ile Kullanıcı Doğrulama
    Task<User> ValidateTokenAsync(string token);
    
    // Şifre Değiştirme
    Task<bool> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword);
    
    // Şifre Sıfırlama Talebi
    Task<string> ForgotPasswordAsync(string email);
    
    // Şifre Sıfırlama (Token ile)
    Task<bool> ResetPasswordAsync(string email, string resetToken, string newPassword);
    
    // Email Doğrulama
    Task<bool> VerifyEmailAsync(Guid userId, string verificationToken);
    
    // Kullanıcı Bilgilerini Getir
    Task<User> GetUserByIdAsync(Guid userId);
    
    // Email ile Kullanıcı Getir
    Task<User> GetUserByEmailAsync(string email);
    
    // Kullanıcı Profili Güncelleme
    Task<User> UpdateUserProfileAsync(Guid userId, string firstName, string lastName, string phoneNumber, string profilePictureUrl);
    
    // Kullanıcı Rolü Güncelleme (Admin için)
    Task<bool> UpdateUserRoleAsync(Guid userId, string role);
    
    // Kullanıcı Aktiflik Durumu Güncelleme (Admin için)
    Task<bool> UpdateUserStatusAsync(Guid userId, bool isActive);
    
    // Kullanıcı Departman ve Yönetici Atama
    Task<bool> AssignDepartmentAndManagerAsync(Guid userId, Guid? departmentId, Guid? managerId);
    
    // Son Giriş Tarihini Güncelle
    Task UpdateLastLoginDateAsync(Guid userId);
    
    // Token Oluşturma (JWT)
    string GenerateJwtToken(User user);
    
    // Refresh Token Oluşturma
    string GenerateRefreshToken();
    
    // Şifre Hash'leme
    string HashPassword(string password);
    
    // Şifre Doğrulama
    bool VerifyPassword(string password, string hashedPassword);
}

