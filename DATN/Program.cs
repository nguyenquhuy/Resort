using DATN.Models;
using DATN.Models.Context;
using DATN.Models.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<DATNDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddSingleton<EmailService>(); // Thêm EmailService vào DI container
// Cấu hình Cookie Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";  // Đường dẫn đến trang đăng nhập
        options.AccessDeniedPath = "/Error";  // Trang báo lỗi khi không đủ quyền
    });

builder.Services.AddSingleton<IVnPayService, VnPayService>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

//routes
app.UseEndpoints(endpoints =>
{
    // Route for actions without parameters (e.g., /RoomUser)
    endpoints.MapControllerRoute(
        name: "nonParameterized",
        pattern: "{action}",
        defaults: new { controller = "Home" }
    );

    // Route for actions with an id parameter (e.g., /Room/Details/1)
    endpoints.MapControllerRoute(
        name: "parameterizedWithId",
        pattern: "{action}/{id?}",
        defaults: new { controller = "Home" }
    );

    // Default route for other actions or controllers
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");
});



app.Run();
