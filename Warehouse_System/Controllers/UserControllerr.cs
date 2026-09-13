using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Online_Store_Backend.DTOs.User;

namespace Online_Store_Backend.Controllers
{
    [ApiController]
    [Route("api/")]
    //[Authorize(Roles = "Manager")] // حصر جميع العمليات بالمدير فقط
    public class UsersController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;

        public UsersController(
            UserManager<IdentityUser> userManager)
        {
            _userManager = userManager;
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
                // حظر الحساب لمنعه من تسجيل الدخول (حظر لـ 100 سنة)
                await _userManager.SetLockoutEnabledAsync(user, true);
                await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);

            }

            string statusMessage = model.IsActive ? "تم تفعيل الحساب بنجاح." : "تم تعطيل الحساب بنجاح.";
            return Ok(new { message = statusMessage });
        }
    }
}