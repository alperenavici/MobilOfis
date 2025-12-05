using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using MobilOfis.Core.IServices;
using MobilOfis.Web.Models.DTOs;
using MobilOfis.Web.Models.ViewModels;
using System.Security.Claims;

namespace MobilOfis.Web.Controllers;

public class AuthController : Controller
{
    private readonly IAuthServices _authServices;

    public AuthController(IAuthServices authServices)
    {
        _authServices = authServices;
    }
    
    #region MVC Actions
    
    /// <summary>
    /// Login sayfasını göster
    /// </summary>
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Dashboard");
        }
        
        ViewData["ReturnUrl"] = returnUrl;
        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }
    
    /// <summary>
    /// Login form submit
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }
        
        try
        {
            var (accessToken, refreshToken) = await _authServices.LoginAsync(model.Email, model.Password);
            
            // Token'ları cookie'ye kaydet
            Response.Cookies.Append("AccessToken", accessToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddMinutes(15)
            });
            
            Response.Cookies.Append("RefreshToken", refreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddDays(7)
            });
            
            // Kullanıcı bilgilerini al ve claims oluştur
            var user = await _authServices.ValidateTokenAsync(accessToken);
            
            if (user == null)
            {
                throw new Exception("Kullanıcı doğrulanamadı.");
            }
            
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim("userId", user.UserId.ToString()), // Keep custom claim for backward compatibility if needed
                new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role ?? "Employee"),
                new Claim("departmentId", user.DepartmentId?.ToString() ?? "")
            };
            
            var claimsIdentity = new ClaimsIdentity(claims, "Cookies");
            var authProperties = new Microsoft.AspNetCore.Authentication.AuthenticationProperties
            {
                IsPersistent = model.RememberMe,
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(model.RememberMe ? 7 : 1)
            };
            
            await HttpContext.SignInAsync("Cookies", new ClaimsPrincipal(claimsIdentity), authProperties);
            
            TempData["SuccessMessage"] = "Giriş başarılı. Hoş geldiniz!";
            
            if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            {
                return Redirect(model.ReturnUrl);
            }
            
            return RedirectToAction("Index", "Dashboard");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }
    
    /// <summary>
    /// Register sayfasını göster
    /// </summary>
    [HttpGet]
    public IActionResult Register()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Dashboard");
        }
        
        return View(new RegisterViewModel());
    }
    
    /// <summary>
    /// Register form submit
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }
        
        try
        {
            var user = await _authServices.RegisterAsync(
                model.FirstName,
                model.LastName,
                model.Email,
                model.Password,
                model.PhoneNumber
            );
            
            TempData["SuccessMessage"] = "Kayıt başarılı! Giriş yapabilirsiniz.";
            return RedirectToAction(nameof(Login));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }
    
    /// <summary>
    /// Şifremi unuttum sayfası
    /// </summary>
    [HttpGet]
    public IActionResult ForgotPassword()
    {
        return View(new ForgotPasswordViewModel());
    }
    
    /// <summary>
    /// Şifremi unuttum form submit
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }
        
        try
        {
            var resetToken = await _authServices.ForgotPasswordAsync(model.Email);
            
            TempData["SuccessMessage"] = "Şifre sıfırlama bağlantısı email adresinize gönderildi.";
            return RedirectToAction(nameof(Login));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }
    
    /// <summary>
    /// Şifre sıfırlama sayfası
    /// </summary>
    [HttpGet]
    public IActionResult ResetPassword(string email, string token)
    {
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(token))
        {
            return RedirectToAction(nameof(Login));
        }
        
        var model = new ResetPasswordViewModel
        {
            Email = email,
            ResetToken = token
        };
        
        return View(model);
    }
    
    /// <summary>
    /// Şifre sıfırlama form submit
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }
        
        try
        {
            var result = await _authServices.ResetPasswordAsync(
                model.Email,
                model.ResetToken,
                model.NewPassword
            );
            
            TempData["SuccessMessage"] = "Şifreniz başarıyla sıfırlandı. Giriş yapabilirsiniz.";
            return RedirectToAction(nameof(Login));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }
    
    /// <summary>
    /// Çıkış yap
    /// </summary>
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        try
        {
            var userIdClaim = User.FindFirst("userId");
            if (userIdClaim != null)
            {
                var userId = Guid.Parse(userIdClaim.Value);
                await _authServices.RevokeRefreshTokenAsync(userId);
            }
            
            Response.Cookies.Delete("AccessToken");
            Response.Cookies.Delete("RefreshToken");
            
            await HttpContext.SignOutAsync("Cookies");
            
            TempData["SuccessMessage"] = "Çıkış başarılı.";
            return RedirectToAction(nameof(Login));
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction("Index", "Dashboard");
        }
    }
    
    /// <summary>
    /// Access Denied sayfası
    /// </summary>
    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }
    
    #endregion
    
    #region API Actions

    /// <summary>
    /// Yeni kullanıcı kaydı (API)
    /// </summary>
    [HttpPost]
    [Route("api/[controller]/register")]
    public async Task<IActionResult> RegisterApi([FromBody] RegisterDto registerDto)
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
    /// Kullanıcı girişi (API)
    /// </summary>
    [HttpPost]
    [Route("api/[controller]/login")]
    public async Task<IActionResult> LoginApi([FromBody] LoginDto loginDto)
    {
        try
        {
            var (accessToken, refreshToken) = await _authServices.LoginAsync(
                loginDto.Email, 
                loginDto.Password
            );

            // Refresh token'ı HttpOnly cookie'de sakla (XSS koruması)
            Response.Cookies.Append("refreshToken", refreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true, // HTTPS'de çalışır
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddDays(7)
            });

            // Kullanıcı bilgilerini al
            var user = await _authServices.ValidateTokenAsync(accessToken);

            if (user == null)
            {
                return Unauthorized(new { message = "Geçersiz token." });
            }

            return Ok(new
            {
                message = "Giriş başarılı.",
                accessToken = accessToken,
                expiresIn = 900, // 15 dakika (saniye cinsinden)
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
    /// Refresh Token ile Yeni Access Token Al (API)
    /// </summary>
    [HttpPost]
    [Route("api/[controller]/refresh")]
    public async Task<IActionResult> RefreshToken()
    {
        try
        {
            // Cookie'den refresh token'ı al
            if (!Request.Cookies.TryGetValue("refreshToken", out var refreshToken))
            {
                return Unauthorized(new { message = "Refresh token bulunamadı." });
            }

            var (accessToken, newRefreshToken) = await _authServices.RefreshTokenAsync(refreshToken);

            // Yeni refresh token'ı cookie'ye yaz (rotation)
            Response.Cookies.Append("refreshToken", newRefreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddDays(7)
            });

            return Ok(new
            {
                message = "Token yenilendi.",
                accessToken = accessToken,
                expiresIn = 900 // 15 dakika
            });
        }
        catch (Exception ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

 
    /// <summary>
    /// Çıkış (Logout) (API)
    /// </summary>
    [Authorize]
    [HttpPost]
    [Route("api/[controller]/logout")]
    public async Task<IActionResult> LogoutApi()
    {
        try
        {
            var userIdClaim = User.FindFirst("userId");
            if (userIdClaim == null)
            {
                return Unauthorized(new { message = "Kullanıcı bilgisi bulunamadı." });
            }

            var userId = Guid.Parse(userIdClaim.Value);
            await _authServices.RevokeRefreshTokenAsync(userId);

            Response.Cookies.Delete("refreshToken");

            return Ok(new { message = "Çıkış başarılı." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Token doğrulama (API)
    /// </summary>
    [HttpPost]
    [Route("api/[controller]/validate-token")]
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
    /// Şifre değiştirme (API)
    /// </summary>
    [Authorize]
    [HttpPost]
    [Route("api/[controller]/change-password")]
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
    /// Şifre sıfırlama talebi (API)
    /// </summary>
    [HttpPost]
    [Route("api/[controller]/forgot-password")]
    public async Task<IActionResult> ForgotPasswordApi([FromBody] ForgotPasswordDto forgotPasswordDto)
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
    /// Şifre sıfırlama (API)
    /// </summary>
    [HttpPost]
    [Route("api/[controller]/reset-password")]
    public async Task<IActionResult> ResetPasswordApi([FromBody] ResetPasswordDto resetPasswordDto)
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
    /// Email doğrulama (API)
    /// </summary>
    [HttpPost]
    [Route("api/[controller]/verify-email")]
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
    /// Kullanıcı bilgilerini getir (API)
    /// </summary>
    [Authorize]
    [HttpGet]
    [Route("api/[controller]/user/{userId}")]
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
    /// Kullanıcı profili güncelleme (API)
    /// </summary>
    [Authorize]
    [HttpPut]
    [Route("api/[controller]/update-profile")]
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
    /// Kullanıcı rolü güncelleme (Admin) (API)
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpPut]
    [Route("api/[controller]/update-role")]
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
    /// Kullanıcı aktiflik durumu güncelleme (Admin) (API)
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpPut]
    [Route("api/[controller]/update-status")]
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
    /// Departman ve yönetici atama (Admin) (API)
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpPut]
    [Route("api/[controller]/assign-department-manager")]
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
    
    #endregion
}
