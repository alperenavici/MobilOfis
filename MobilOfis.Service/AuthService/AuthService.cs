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
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConfiguration _configuration;

    public AuthService(IUnitOfWork unitOfWork, IConfiguration configuration)
    {
        _unitOfWork = unitOfWork;
        _configuration = configuration;
    }

    public async Task<User> RegisterAsync(string firstName, string lastName, string email, string password, string phoneNumber)
    {
        var existingUser = await GetUserByEmailAsync(email);
        if (existingUser != null)
        {
            throw new Exception("Bu email adresi zaten kullanılıyor.");
        }

        var user = new User
        {
            UserId = Guid.NewGuid(),
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            PhoneNumber = phoneNumber,
            PasswordHash = HashPassword(password),
            IsActive = true,
            Role = "Employee", 
            CreatedDate = DateTime.UtcNow
        };

        await _unitOfWork.Users.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        return user;
    }

    
    public async Task<(string accessToken, string refreshToken)> LoginAsync(string email, string password)
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

        // JWT Access Token oluştur (15 dakika)
        var accessToken = GenerateJwtToken(user);
        
        // Refresh Token oluştur (7 gün)
        var refreshToken = GenerateRefreshToken();
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
        user.LastLoginDate = DateTime.UtcNow;
        user.UpdatedDate = DateTime.UtcNow;
        
        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync();

        return (accessToken, refreshToken);
    }

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

    public async Task<(string accessToken, string refreshToken)> RefreshTokenAsync(string refreshToken)
    {
        // Refresh token'a sahip kullanıcıyı bul
        var user = await _unitOfWork.Users.GetByRefreshTokenAsync(refreshToken);
        
        if (user == null)
        {
            throw new Exception("Geçersiz refresh token.");
        }

        // Token süresini kontrol et
        if (user.RefreshTokenExpiry == null || user.RefreshTokenExpiry < DateTime.UtcNow)
        {
            throw new Exception("Refresh token'ın süresi dolmuş. Lütfen tekrar giriş yapın.");
        }

        // Yeni access token oluştur
        var newAccessToken = GenerateJwtToken(user);
        
        // Yeni refresh token oluştur (rotation için)
        var newRefreshToken = GenerateRefreshToken();
        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
        user.UpdatedDate = DateTime.UtcNow;
        
        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync();

        return (newAccessToken, newRefreshToken);
    }

    public async Task<bool> RevokeRefreshTokenAsync(Guid userId)
    {
        var user = await GetUserByIdAsync(userId);
        
        if (user == null)
        {
            return false;
        }

        user.RefreshToken = null;
        user.RefreshTokenExpiry = null;
        user.UpdatedDate = DateTime.UtcNow;
        
        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    
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
        
        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

  
    public async Task<string> ForgotPasswordAsync(string email)
    {
        var user = await GetUserByEmailAsync(email);
        
        if (user == null)
        {
            throw new Exception("Bu email ile kayıtlı kullanıcı bulunamadı.");
        }

        var resetToken = GeneratePasswordResetToken();
        user.PasswordResetToken = resetToken;
        user.PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(1); 
        user.UpdatedDate = DateTime.UtcNow;
        
        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync();
        
        return resetToken;
    }

    public async Task<bool> ResetPasswordAsync(string email, string resetToken, string newPassword)
    {
        var user = await GetUserByEmailAsync(email);
        
        if (user == null)
        {
            throw new Exception("Kullanıcı bulunamadı.");
        }

        // Token doğrulaması 
        if (string.IsNullOrEmpty(user.PasswordResetToken) || user.PasswordResetToken != resetToken)
        {
            throw new Exception("Geçersiz reset token.");
        }

        // Token süresinin dolup dolmadığını kontrol et
        if (user.PasswordResetTokenExpiry == null || user.PasswordResetTokenExpiry < DateTime.UtcNow)
        {
            throw new Exception("Reset token'ın süresi dolmuş. Lütfen yeni bir şifre sıfırlama talebi oluşturun.");
        }

        user.PasswordHash = HashPassword(newPassword);
        user.PasswordResetToken = null; 
        user.PasswordResetTokenExpiry = null;
        user.UpdatedDate = DateTime.UtcNow;
        
        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

   
    public async Task<bool> VerifyEmailAsync(Guid userId, string verificationToken)
    {
        var user = await GetUserByIdAsync(userId);
        
        if (user == null)
        {
            return false;
        }

        
        user.IsActive = true;
        user.UpdatedDate = DateTime.UtcNow;
        
        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }


    public async Task<User> GetUserByIdAsync(Guid userId)
    {
        return await _unitOfWork.Users.GetByIdAsync(userId);
    }

 
    public async Task<User> GetUserByEmailAsync(string email)
    {
        return await _unitOfWork.Users.GetByEmailAsync(email);
    }

    
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
        
        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync();

        return user;
    }

  
  
    public async Task<bool> UpdateUserRoleAsync(Guid userId, string role)
    {
        var user = await GetUserByIdAsync(userId);
        
        if (user == null)
        {
            throw new Exception("Kullanıcı bulunamadı.");
        }

        user.Role = role;
        user.UpdatedDate = DateTime.UtcNow;
        
        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

 
    public async Task<bool> UpdateUserStatusAsync(Guid userId, bool isActive)
    {
        var user = await GetUserByIdAsync(userId);
        
        if (user == null)
        {
            throw new Exception("Kullanıcı bulunamadı.");
        }

        user.IsActive = isActive;
        user.UpdatedDate = DateTime.UtcNow;
        
        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

  
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
        
        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    
    public async Task UpdateLastLoginDateAsync(Guid userId)
    {
        var user = await GetUserByIdAsync(userId);
        
        if (user != null)
        {
            user.LastLoginDate = DateTime.UtcNow;
            _unitOfWork.Users.Update(user);
            await _unitOfWork.SaveChangesAsync();
        }
    }

 
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
            expires: DateTime.UtcNow.AddMinutes(15), // 15 dakika (güvenlik için kısa tutuldu)
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

    // Password Reset Token Oluşturma
    public string GeneratePasswordResetToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    public string HashPassword(string password)
    {
        // BCrypt kullanarak şifre hash'leme
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    
    public bool VerifyPassword(string password, string hashedPassword)
    {
        // BCrypt kullanarak şifre doğrulama
        return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
    }
}

