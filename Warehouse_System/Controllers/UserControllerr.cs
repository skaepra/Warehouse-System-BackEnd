using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Warehouse_System_BackEnd.Data;
using Warehouse_System_BackEnd.DTOs.User;

namespace Warehouse_System_BackEnd.Controllers
{
    [ApiController]
    [Route("api/")]
    [Authorize(Roles = "Manager")] 
    public class UsersController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ApplicationDbContext _context;

        public UsersController(UserManager<IdentityUser> userManager, ApplicationDbContext context)

        {
            _userManager = userManager;
            _context = context;
        }


        /// جلب جميع المستخدمين مع أدوارهم وحالة تفعيل حساباتهم
        [HttpGet("users")]
        [ProducesResponseType(typeof(IEnumerable<UserDetailsDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userManager.Users.ToListAsync();
            var userList = new List<UserDetailsDto>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);

                // يعتبر الحساب مفعلاً إذا لم يكن موقوفاً أو تاريخ الحظر قد انتهى
                bool isActive = user.LockoutEnd == null ;

                userList.Add(new UserDetailsDto
                {
                    Id = user.Id,
                    Email = user.Email ?? string.Empty,
                    UserName = user.UserName ?? string.Empty,
                    IsActive = isActive,
                    Roles = roles.ToList()
                });
            }

            return Ok(userList);
        }

        /// تفعيل أو تعطيل حساب المستخدم 
        [HttpPatch("user/activationStatus")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ToggleUserStatus([FromBody] ToggleUserStatusDto model)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (currentUserId == model.UserId)
                return BadRequest(new { message = "لا يمكنك تعطيل حسابك الشخصي." });

            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null)
                return NotFound(new { message = "المستخدم غير موجود." });

            if (model.IsActive)
            {
                // إلغاء الحظر وإعادة التفعيل
                await _userManager.SetLockoutEndDateAsync(user, null);
            }
            else
            {
                // 1. حظر الحساب لمنعه من تسجيل الدخول
                await _userManager.SetLockoutEnabledAsync(user, true);
                await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);

                // 2. الخاصة بالمستخدم  Refresh Tokens  التعامل مع الـ
                var userTokens = await _context.RefreshTokens
                    .Where(t => t.UserId == user.Id)
                    .ToListAsync();

                if (userTokens.Any())
                {
                    // الخيار الأول: الحذف النهائى من قاعدة البيانات
                    _context.RefreshTokens.RemoveRange(userTokens);
                    await _context.SaveChangesAsync();
                }

                // 3. تحديث الـ SecurityStamp لإبطال جيل الـ Access Tokens الحالي فوراً
                await _userManager.UpdateSecurityStampAsync(user);
            }

            string statusMessage = model.IsActive ? "تم تفعيل الحساب بنجاح." : "تم تعطيل الحساب بنجاح والإلغاء الفوري لجلساته.";
            return Ok(new { message = statusMessage });
        }
    }
}