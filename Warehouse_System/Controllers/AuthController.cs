using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Online_Store_Backend.DTOs;
using Online_Store_Backend.ResponseDto;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IConfiguration _configuration;

    public AuthController(
        UserManager<IdentityUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _configuration = configuration;
    }


    /// إنشاء حساب 

    [HttpPost("create-employee")]
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

        var usernameFromEmail = model.Email.Split('@')[0];

        // 2. إنشاء كائن المستخدم
        var user = new IdentityUser
        {
            UserName = usernameFromEmail,
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

    /// <summary>
    /// تسجيل الدخول وإرجاع Token
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginDto model)
    {
        // 1. البحث عن المستخدم بالبريد
        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
        {
            return Unauthorized(new { message = "البريد الإلكتروني أو كلمة السر غير صحيحة" });
        }

        // 2. توليد Token عند نجاح تسجيل الدخول
        var token = await GenerateJwtTokenAsync(user);

        return Ok(new
        {
            message = "تم تسجيل الدخول بنجاح",
            token = new JwtSecurityTokenHandler().WriteToken(token),
            expiration = token.ValidTo
        });
    }

    /// <summary>
    /// دالة مساعدة لتوليد الـ JWT Token ومطابقته مع بيانات المستخدم وأدواره
    /// </summary>
    private async Task<JwtSecurityToken> GenerateJwtTokenAsync(IdentityUser user)
    {
        var userRoles = await _userManager.GetRolesAsync(user);

        var authClaims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
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
            expires: DateTime.UtcNow.AddMonths(6), // مدة صلاحية التوكن 6 أشهر
            claims: authClaims,
            signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
        );
    }
}