using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Warehouse_System_BackEnd.DTOs.Auth;
using Warehouse_System_BackEnd.ResponseDto.Auth;
using Warehouse_System_BackEnd.Table;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Warehouse_System_BackEnd.Data;

using System.Text;
using Warehouse_System_BackEnd.DTOs;
using Warehouse_System_BackEnd.DTOs.Online_Store_Backend.DTOs.Auth;

[Route("api/")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IConfiguration _configuration;
    private readonly ApplicationDbContext _context;

    public AuthController(
        ApplicationDbContext context,
        UserManager<IdentityUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _configuration = configuration;
        _context = context;
    }


    /// إنشاء حساب 
    [HttpPost("createEmployee")]
    [Authorize(Roles = "Manager")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateEmployee([FromBody] CreateEmployeeDto model)
    {
        // 1. التحقق من وجود الدور (Role)
        if (!await _roleManager.RoleExistsAsync(model.Role))
        {
            return BadRequest(new { message = $"الدور الموظف المحدد '{model.Role}' غير موجود بالنظام." });
        }

        // 2. إنشاء كائن المستخدم
        var user = new IdentityUser
        {
            UserName = model.FullName,
            Email = model.Email,
        };

        // 3. حفظ المستخدم وتشفير كلمة السر
        var result = await _userManager.CreateAsync(user, model.Password);
        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description);
            return BadRequest(new { message = "فشل إنشاء الحساب", errors });
        }

        // 4. تعيين الدور للموظف
        await _userManager.AddToRoleAsync(user, model.Role);

        return Ok(new
        {
            message = "تم إنشاء الحساب بنجاح وتوليد التوكن",
        });
    }


    /// تسجيل الدخول وإرجاع Token
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginDto model)
    {
        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
        {
            return Unauthorized(new { message = "البريد الإلكتروني أو كلمة السر غير صحيحة" });
        }

        //  التحقق مما إذا كان الحساب معطلاً/محظوراً
        if (await _userManager.IsLockedOutAsync(user))
        {
            return Unauthorized(new { message = "هذا الحساب معطل حالياً، يرجى مراجعة مدير النظام." });
        }

        var token = await GenerateJwtTokenAsync(user);
        var refreshToken = await GenerateRefreshTokenAsync(user);

        return Ok(new
        {
            message = "تم تسجيل الدخول بنجاح",
            token = new JwtSecurityTokenHandler().WriteToken(token),
            refreshToken = refreshToken.Token,
            expiration = token.ValidTo
        });
    }

    /// تسجيل الخروج وإلغاء الـ Refresh Token
    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout([FromBody] LogoutDto model)
    {
        if (string.IsNullOrEmpty(model.RefreshToken))
        {
            return BadRequest(new { message = "الـ Refresh Token مطلوب" });
        }

        // 1. البحث عن الـ Refresh Token في قاعدة البيانات
        var storedToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(t => t.Token == model.RefreshToken);

        if (storedToken == null)
        {
            return BadRequest(new { message = "الـ Refresh Token غير موجود" });
        }

        // 2. إلغاء التوكن (Revoke)
        storedToken.IsRevoked = true;
        await _context.SaveChangesAsync();

        return Ok(new { message = "تم تسجيل الخروج بنجاح" });
    }

    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest("البيانات الممررة غير صالحة");

        // 1. البحث عن الـ Refresh Token في قاعدة البيانات
           var storedToken = await _context.RefreshTokens
  .          Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Token == request.RefreshToken);

        if (storedToken == null)
            return BadRequest(new { message = "الـ Refresh Token غير موجود" });

        // 2. التحقق من أن التوكن لم ينته ولم يُلغَ
        if (storedToken.IsRevoked || DateTime.UtcNow >= storedToken.ExpiryDate)
            return BadRequest(new { message = "انتهت صلاحية الجلسة، يرجى إعادة تسجيل الدخول" });

        // 3. إنتاج Access Token جديد و Refresh Token جديد
        var newJwtToken = await GenerateJwtTokenAsync(storedToken.User);
        var newRefreshToken = await GenerateRefreshTokenAsync(storedToken.User);

        // إلغاء الـ Refresh Token القديم بعد الاستخدام
        storedToken.IsRevoked = true;
        await _context.SaveChangesAsync();

        return Ok(new
        {
            token = new JwtSecurityTokenHandler().WriteToken(newJwtToken),
            refreshToken = newRefreshToken.Token,
            expiration = newJwtToken.ValidTo
        });
    }

    /// دالة مساعدة لتوليد الـ JWT Token ومطابقته مع بيانات المستخدم وأدواره
    private async Task<JwtSecurityToken> GenerateJwtTokenAsync(IdentityUser user)
    {
        var userRoles = await _userManager.GetRolesAsync(user);

        var authClaims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
            new Claim(ClaimTypes.Email, user.Email!),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        foreach (var role in userRoles)
        {
            authClaims.Add(new Claim(ClaimTypes.Role, role));
        }

        var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]!));

        return new JwtSecurityToken(
            issuer: _configuration["JWT:ValidIssuer"],
            audience: _configuration["JWT:ValidAudience"],
            expires: DateTime.UtcNow.AddMinutes(30), // مدة صلاحية التوكن 30 دقيقة
            claims: authClaims,
            signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
        );
    }

    private async Task<RefreshToken> GenerateRefreshTokenAsync(IdentityUser user)
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);

        var refreshToken = new RefreshToken
        {
            Token = Convert.ToBase64String(randomNumber),
            UserId = user.Id,
            AddedDate = DateTime.UtcNow,
            ExpiryDate = DateTime.UtcNow.AddDays(30),
            IsRevoked = false
        };

        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync();

        return refreshToken;
    }
}


