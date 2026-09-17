using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace Online_Store_Backend.Extensions
{
    public class CheckUserStatusMiddleware
    {
        private readonly RequestDelegate _next;

        public CheckUserStatusMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, UserManager<IdentityUser> userManager)
        {
            if (context.User.Identity?.IsAuthenticated == true)
            {
                var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!string.IsNullOrEmpty(userId))
                {
                    var user = await userManager.FindByIdAsync(userId);

                    // إذا كان المستخدم غير موجود أو محظور حالياً
                    if (user == null || await userManager.IsLockedOutAsync(user))
                    {
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        await context.Response.WriteAsJsonAsync(new { message = "تم تعطيل حسابك، يرجى التواصل مع الإدارة." });
                        return; // إيقاف الطلب وعدم إكماله
                    }
                }
            }

            await _next(context);
        }
    }
}
