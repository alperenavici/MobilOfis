using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MobilOfis.Core.IRepositories;
using MobilOfis.Core.IServices;
using MobilOfis.Entity;

namespace MobilOfis.Service.AuthService;

public class AuthService : IAuthServices
{
    private readonly IGenericRepository<User> _userRepository;
    private readonly IConfiguration _configuration;

    public AuthService(IGenericRepository<User> userRepository, IConfiguration configuration)
    {
        _userRepository = userRepository;
        _configuration = configuration;
    }

    // Kullanıcı Kaydı
    public async Task<User> RegisterAsync(string firstName, string lastName, string email, string password, string phoneNumber)
    {
        // Email kontrolü
        var existingUser = await GetUserByEmailAsync(email);
        if (existingUser != null)
        {
            throw new Exception("Bu email adresi zaten kullanılıyor.");
        }

        // Yeni kullanıcı oluştur
        var user = new User
        {
            UserId = Guid.NewGuid(),
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            PhoneNumber = phoneNumber,
            PasswordHash = HashPassword(password),
            IsActive = true,
            Role = "Employee", // Varsayılan rol
            CreatedDate = DateTime.UtcNow
        };

        await _userRepository.AddAsync(user);
        await _userRepository.SaveChangesAsync();

        return user;
    }

    // Kullanıcı Girişi
    public async Task<User> LoginAsync(string email, string password)
    {
        var user = await GetUserByEmailAsync(email);
        
        if (user == null)
        {
            throw new Exception("Kullanıcı bulunamadı.");
        }

        if (!user.IsActive)
        {
            throw new Exception("Hesabınız aktif değil.");
        }

        if (!VerifyPassword(password, user.PasswordHash))
        {
            throw new Exception("Şifre hatalı.");
        }

        // Son giriş tarihini güncelle
        await UpdateLastLoginDateAsync(user.UserId);

        return user;
    }

    // Token ile Kullanıcı Doğrulama
    public async Task<User> ValidateTokenAsync(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? "YourSuperSecretKeyHere123456789012");
            
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _configuration["Jwt:Issuer"] ?? "MobilOfis",
                ValidateAudience = true,
                ValidAudience = _configuration["Jwt:Audience"] ?? "MobilOfisUsers",
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            var jwtToken = (JwtSecurityToken)validatedToken;
            var userId = Guid.Parse(jwtToken.Claims.First(x => x.Type == "userId").Value);

            return await GetUserByIdAsync(userId);
        }
        catch
        {
            return null;
        }
    }

    // Şifre Değiştirme
    public async Task<bool> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword)
    {
        var user = await GetUserByIdAsync(userId);
        
        if (user == null)
        {
            throw new Exception("Kullanıcı bulunamadı.");
        }

        if (!VerifyPassword(currentPassword, user.PasswordHash))
        {
            throw new Exception("Mevcut şifre hatalı.");
        }

        user.PasswordHash = HashPassword(newPassword);
        user.UpdatedDate = DateTime.UtcNow;
        
        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync();

        return true;
    }

    // Şifre Sıfırlama Talebi
    public async Task<string> ForgotPasswordAsync(string email)
    {
        var user = await GetUserByEmailAsync(email);
        
        if (user == null)
        {
            throw new Exception("Bu email ile kayıtlı kullanıcı bulunamadı.");
        }

        // Reset token oluştur (normalde bu token veritabanına kaydedilmeli)
        var resetToken = GenerateRefreshToken();
        
        // TODO: Reset token'ı veritabanına kaydet ve email gönder
        
        return resetToken;
    }

    // Şifre Sıfırlama (Token ile)
    public async Task<bool> ResetPasswordAsync(string email, string resetToken, string newPassword)
    {
        var user = await GetUserByEmailAsync(email);
        
        if (user == null)
        {
            throw new Exception("Kullanıcı bulunamadı.");
        }

        // TODO: Reset token'ı doğrula (veritabanından)
        
        user.PasswordHash = HashPassword(newPassword);
        user.UpdatedDate = DateTime.UtcNow;
        
        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync();

        return true;
    }

    // Email Doğrulama
    public async Task<bool> VerifyEmailAsync(Guid userId, string verificationToken)
    {
        var user = await GetUserByIdAsync(userId);
        
        if (user == null)
        {
            return false;
        }

        // TODO: Verification token'ı doğrula
        
        user.IsActive = true;
        user.UpdatedDate = DateTime.UtcNow;
        
        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync();

        return true;
    }

    // Kullanıcı Bilgilerini Getir
    public async Task<User> GetUserByIdAsync(Guid userId)
    {
        return await _userRepository.GetByIdAsync(userId);
    }

    // Email ile Kullanıcı Getir
    public async Task<User> GetUserByEmailAsync(string email)
    {
        var users = await _userRepository.FindAsync(u => u.Email == email);
        return users.FirstOrDefault();
    }

    // Kullanıcı Profili Güncelleme
    public async Task<User> UpdateUserProfileAsync(Guid userId, string firstName, string lastName, string phoneNumber, string profilePictureUrl)
    {
        var user = await GetUserByIdAsync(userId);
        
        if (user == null)
        {
            throw new Exception("Kullanıcı bulunamadı.");
        }

        user.FirstName = firstName;
        user.LastName = lastName;
        user.PhoneNumber = phoneNumber;
        user.ProfilePictureUrl = profilePictureUrl;
        user.UpdatedDate = DateTime.UtcNow;
        
        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync();

        return user;
    }

    // Kullanıcı Rolü Güncelleme (Admin için)
    public async Task<bool> UpdateUserRoleAsync(Guid userId, string role)
    {
        var user = await GetUserByIdAsync(userId);
        
        if (user == null)
        {
            throw new Exception("Kullanıcı bulunamadı.");
        }

        user.Role = role;
        user.UpdatedDate = DateTime.UtcNow;
        
        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync();

        return true;
    }

    // Kullanıcı Aktiflik Durumu Güncelleme (Admin için)
    public async Task<bool> UpdateUserStatusAsync(Guid userId, bool isActive)
    {
        var user = await GetUserByIdAsync(userId);
        
        if (user == null)
        {
            throw new Exception("Kullanıcı bulunamadı.");
        }

        user.IsActive = isActive;
        user.UpdatedDate = DateTime.UtcNow;
        
        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync();

        return true;
    }

    // Kullanıcı Departman ve Yönetici Atama
    public async Task<bool> AssignDepartmentAndManagerAsync(Guid userId, Guid? departmentId, Guid? managerId)
    {
        var user = await GetUserByIdAsync(userId);
        
        if (user == null)
        {
            throw new Exception("Kullanıcı bulunamadı.");
        }

        user.DepartmentId = departmentId;
        user.ManagerId = managerId;
        user.UpdatedDate = DateTime.UtcNow;
        
        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync();

        return true;
    }

    // Son Giriş Tarihini Güncelle
    public async Task UpdateLastLoginDateAsync(Guid userId)
    {
        var user = await GetUserByIdAsync(userId);
        
        if (user != null)
        {
            user.LastLoginDate = DateTime.UtcNow;
            _userRepository.Update(user);
            await _userRepository.SaveChangesAsync();
        }
    }

    // Token Oluşturma (JWT)
    public string GenerateJwtToken(User user)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? "YourSuperSecretKeyHere123456789012"));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim("userId", user.UserId.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
            new Claim(ClaimTypes.Role, user.Role ?? "Employee"),
            new Claim("departmentId", user.DepartmentId?.ToString() ?? ""),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"] ?? "MobilOfis",
            audience: _configuration["Jwt:Audience"] ?? "MobilOfisUsers",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(24),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    // Refresh Token Oluşturma
    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    // Şifre Hash'leme
    public string HashPassword(string password)
    {
        // BCrypt kullanarak şifre hash'leme
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    // Şifre Doğrulama
    public bool VerifyPassword(string password, string hashedPassword)
    {
        // BCrypt kullanarak şifre doğrulama
        return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
    }
}

