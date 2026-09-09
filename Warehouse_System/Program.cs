using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Online_Store_Backend.Data;
using Online_Store_Backend.Extensions;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerWithJwtAuth();

// 1. ربط الـ DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. إعداد Identity (تخفيف القيود للتسهيل على أرقام الموظفين)
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    options.Password.RequiredLength = 8;           // الحد الأدنى للطول 8 خانات
    options.Password.RequireDigit = true;          // يجب أن تحتوي على رقم واحد على الأقل (0-9)
    options.Password.RequireLowercase = true;      // يجب أن تحتوي على حرف صغير (a-z)
    options.Password.RequireUppercase = false;      // يجب أن تحتوي على حرف كبير (A-Z)
    options.Password.RequireNonAlphanumeric = true; // يجب أن تحتوي على رمز خاص مثل (@, #, $, !)
    options.Password.RequiredUniqueChars = 1;      // عدد الخانات الفريدة المطلوبة

    options.User.RequireUniqueEmail = true;       // عدم إجبار وجود إيميل فريد

})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// 3. إضافة مصادقة JWT Bearer
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidAudience = builder.Configuration["JWT:ValidAudience"],
        ValidIssuer = builder.Configuration["JWT:ValidIssuer"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:Secret"]!))
    };
});



var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();


app.UseAuthentication(); // 1. التعرف على هوية المستخدم من التوكن
app.UseAuthorization();  // 2. التحقق من الصلاحيات

app.MapControllers();

await app.SeedRolesAndAdminAsync();

app.Run();
