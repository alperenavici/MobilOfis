using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MobilOfis.Core.IServices;
using MobilOfis.Web.Models.DTOs;

namespace MobilOfis.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthServices _authServices;

    public AuthController(IAuthServices authServices)
    {
        _authServices = authServices;
    }

    /// <summary>
    /// Yeni kullanıcı kaydı
    /// </summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
    {
        try
        {
            var user = await _authServices.RegisterAsync(
                registerDto.FirstName,
                registerDto.LastName,
                registerDto.Email,
                registerDto.Password,
                registerDto.PhoneNumber
            );

            return Ok(new
            {
                message = "Kullanıcı başarıyla kaydedildi.",
                userId = user.UserId,
                email = user.Email
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Kullanıcı girişi
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
    {
        try
        {
            var user = await _authServices.LoginAsync(loginDto.Email, loginDto.Password);
            var token = _authServices.GenerateJwtToken(user);
            var refreshToken = _authServices.GenerateRefreshToken();

            return Ok(new
            {
                message = "Giriş başarılı.",
                token = token,
                refreshToken = refreshToken,
                user = new
                {
                    userId = user.UserId,
                    firstName = user.FirstName,
                    lastName = user.LastName,
                    email = user.Email,
                    role = user.Role,
                    departmentId = user.DepartmentId
                }
            });
        }
        catch (Exception ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Token doğrulama
    /// </summary>
    [HttpPost("validate-token")]
    public async Task<IActionResult> ValidateToken([FromBody] TokenDto tokenDto)
    {
        try
        {
            var user = await _authServices.ValidateTokenAsync(tokenDto.Token);
            
            if (user == null)
            {
                return Unauthorized(new { message = "Geçersiz token." });
            }

            return Ok(new
            {
                message = "Token geçerli.",
                user = new
                {
                    userId = user.UserId,
                    firstName = user.FirstName,
                    lastName = user.LastName,
                    email = user.Email,
                    role = user.Role
                }
            });
        }
        catch (Exception ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Şifre değiştirme
    /// </summary>
    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto changePasswordDto)
    {
        try
        {
            var result = await _authServices.ChangePasswordAsync(
                changePasswordDto.UserId,
                changePasswordDto.CurrentPassword,
                changePasswordDto.NewPassword
            );

            return Ok(new { message = "Şifre başarıyla değiştirildi." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Şifre sıfırlama talebi
    /// </summary>
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto forgotPasswordDto)
    {
        try
        {
            var resetToken = await _authServices.ForgotPasswordAsync(forgotPasswordDto.Email);
            
            return Ok(new
            {
                message = "Şifre sıfırlama bağlantısı email adresinize gönderildi.",
                resetToken = resetToken // Production'da email ile gönderilmeli
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Şifre sıfırlama
    /// </summary>
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto resetPasswordDto)
    {
        try
        {
            var result = await _authServices.ResetPasswordAsync(
                resetPasswordDto.Email,
                resetPasswordDto.ResetToken,
                resetPasswordDto.NewPassword
            );

            return Ok(new { message = "Şifre başarıyla sıfırlandı." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Email doğrulama
    /// </summary>
    [HttpPost("verify-email")]
    public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailDto verifyEmailDto)
    {
        try
        {
            var result = await _authServices.VerifyEmailAsync(
                verifyEmailDto.UserId,
                verifyEmailDto.VerificationToken
            );

            if (result)
            {
                return Ok(new { message = "Email başarıyla doğrulandı." });
            }

            return BadRequest(new { message = "Email doğrulama başarısız." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Kullanıcı bilgilerini getir
    /// </summary>
    [Authorize]
    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetUser(Guid userId)
    {
        try
        {
            var user = await _authServices.GetUserByIdAsync(userId);
            
            if (user == null)
            {
                return NotFound(new { message = "Kullanıcı bulunamadı." });
            }

            return Ok(new
            {
                userId = user.UserId,
                firstName = user.FirstName,
                lastName = user.LastName,
                email = user.Email,
                phoneNumber = user.PhoneNumber,
                jobTitle = user.JobTitle,
                departmentId = user.DepartmentId,
                managerId = user.ManagerId,
                role = user.Role,
                isActive = user.IsActive,
                profilePictureUrl = user.ProfilePictureUrl,
                hireDate = user.HireDate,
                lastLoginDate = user.LastLoginDate
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Kullanıcı profili güncelleme
    /// </summary>
    [Authorize]
    [HttpPut("update-profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto updateProfileDto)
    {
        try
        {
            var user = await _authServices.UpdateUserProfileAsync(
                updateProfileDto.UserId,
                updateProfileDto.FirstName,
                updateProfileDto.LastName,
                updateProfileDto.PhoneNumber,
                updateProfileDto.ProfilePictureUrl
            );

            return Ok(new
            {
                message = "Profil başarıyla güncellendi.",
                user = new
                {
                    userId = user.UserId,
                    firstName = user.FirstName,
                    lastName = user.LastName,
                    phoneNumber = user.PhoneNumber,
                    profilePictureUrl = user.ProfilePictureUrl
                }
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Kullanıcı rolü güncelleme (Admin)
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpPut("update-role")]
    public async Task<IActionResult> UpdateRole([FromBody] UpdateRoleDto updateRoleDto)
    {
        try
        {
            var result = await _authServices.UpdateUserRoleAsync(
                updateRoleDto.UserId,
                updateRoleDto.Role
            );

            return Ok(new { message = "Kullanıcı rolü başarıyla güncellendi." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Kullanıcı aktiflik durumu güncelleme (Admin)
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpPut("update-status")]
    public async Task<IActionResult> UpdateStatus([FromBody] UpdateStatusDto updateStatusDto)
    {
        try
        {
            var result = await _authServices.UpdateUserStatusAsync(
                updateStatusDto.UserId,
                updateStatusDto.IsActive
            );

            return Ok(new { message = "Kullanıcı durumu başarıyla güncellendi." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Departman ve yönetici atama (Admin)
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpPut("assign-department-manager")]
    public async Task<IActionResult> AssignDepartmentAndManager([FromBody] AssignDepartmentManagerDto dto)
    {
        try
        {
            var result = await _authServices.AssignDepartmentAndManagerAsync(
                dto.UserId,
                dto.DepartmentId,
                dto.ManagerId
            );

            return Ok(new { message = "Departman ve yönetici ataması başarıyla yapıldı." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}

