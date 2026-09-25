using System.Globalization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using QLDiem.Data;
using QLDiem.Models;
using QLDiem.Services;

var builder = WebApplication.CreateBuilder(args);

var chuoiKetNoi = builder.Configuration.GetConnectionString("QLDiem")
                  ?? throw new InvalidOperationException("Chưa cấu hình chuỗi kết nối 'QLDiem'.");

// ----- Cơ sở dữ liệu -----
builder.Services.AddDbContext<QLDiemDbContext>(opt => opt.UseSqlServer(chuoiKetNoi));

// ----- Đăng nhập, phân quyền -----
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(opt =>
    {
        // Yêu cầu mật khẩu nới lỏng cho môi trường học tập; siết lại khi triển khai thật.
        opt.Password.RequireDigit = true;
        opt.Password.RequiredLength = 6;
        opt.Password.RequireNonAlphanumeric = false;
        opt.Password.RequireUppercase = false;
        opt.User.RequireUniqueEmail = false;
        opt.Lockout.MaxFailedAccessAttempts = 5;
    })
    .AddEntityFrameworkStores<QLDiemDbContext>()
    .AddClaimsPrincipalFactory<NguoiDungClaimsFactory>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(opt =>
{
    opt.LoginPath = "/Account/Login";
    opt.LogoutPath = "/Account/Logout";
    opt.AccessDeniedPath = "/Account/AccessDenied";
    opt.ExpireTimeSpan = TimeSpan.FromHours(8);
    opt.SlidingExpiration = true;
});

builder.Services.AddAuthorization(opt =>
{
    opt.AddPolicy("QuanTri", p => p.RequireRole(VaiTro.Admin));
    opt.AddPolicy("NhapDiem", p => p.RequireRole(VaiTro.Admin, VaiTro.GiaoVien));
});

// ----- Các dịch vụ nghiệp vụ -----
builder.Services.AddScoped<DiemService>();
builder.Services.AddScoped<KetQuaHocTapService>();
builder.Services.AddScoped<ThongKeService>();

builder.Services.AddControllersWithViews();

var app = builder.Build();

// Dùng culture bất biến khi phân tích dữ liệu gửi lên: điểm luôn nhập bằng dấu chấm
// ("8.5") và ngày theo chuẩn ISO, không phụ thuộc cấu hình vùng của máy chủ.
// Phần hiển thị tiếng Việt được xử lý riêng trong lớp HienThi và các view.
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(CultureInfo.InvariantCulture, new CultureInfo("vi-VN")),
    SupportedCultures = new[] { CultureInfo.InvariantCulture },
    SupportedUICultures = new[] { new CultureInfo("vi-VN") }
});

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Áp dụng migration và nạp dữ liệu mẫu khi khởi động.
using (var scope = app.Services.CreateScope())
{
    var sp = scope.ServiceProvider;
    DuLieuMau.KhoiTao(
        sp.GetRequiredService<QLDiemDbContext>(),
        sp.GetRequiredService<UserManager<ApplicationUser>>(),
        sp.GetRequiredService<RoleManager<IdentityRole>>());
}

app.Run();
