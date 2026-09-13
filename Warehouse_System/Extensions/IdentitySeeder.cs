using Microsoft.AspNetCore.Identity;

namespace Online_Store_Backend.Extensions
{
    public static class IdentitySeeder
    {
        public static async Task SeedRolesAndAdminAsync(this IApplicationBuilder app)
        {
            using (var scope = app.ApplicationServices.CreateScope())
            {
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

                // 1. إنشاء الأدوار (Roles) إذا لم تكن موجودة
                string[] roleNames = { "Manager", "Sales", "Storekeeper" };

                foreach (var roleName in roleNames)
                {
                    if (!await roleManager.RoleExistsAsync(roleName))
                    {
                        await roleManager.CreateAsync(new IdentityRole(roleName));
                    }
                }

                // 2. إنشاء حساب المدير الافتراضي (Default Manager)
                string defaultManagerEmail = "manager@gmail.com";
                string defaultPassword = "Manager12345!";

                var managerUser = await userManager.FindByEmailAsync(defaultManagerEmail);

                if (managerUser == null)
                {
                    var newManager = new IdentityUser
                    {
                        UserName = "Manager",
                        Email = defaultManagerEmail,
                        EmailConfirmed = true
                    };

                    var createResult = await userManager.CreateAsync(newManager, defaultPassword);

                    if (createResult.Succeeded)
                    {
                        await userManager.AddToRoleAsync(newManager, "Manager");
                    }
                }
            }
        }
    }
}