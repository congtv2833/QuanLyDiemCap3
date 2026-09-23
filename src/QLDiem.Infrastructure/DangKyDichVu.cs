using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using QLDiem.Application.Interfaces;
using QLDiem.Infrastructure.Data;
using QLDiem.Infrastructure.Services;

namespace QLDiem.Infrastructure;

public static class DangKyDichVu
{
    /// <summary>
    /// Đăng ký DbContext và các dịch vụ nghiệp vụ vào DI container.
    /// Phần Identity được cấu hình ở tầng Web vì nó thuộc về ASP.NET Core.
    /// </summary>
    public static IServiceCollection ThemHaTang(this IServiceCollection services, string chuoiKetNoi)
    {
        services.AddDbContext<QLDiemDbContext>(opt => opt.UseSqlServer(chuoiKetNoi));

        services.AddScoped<IDiemService, DiemService>();
        services.AddScoped<KetQuaHocTapService>();
        services.AddScoped<IKetQuaHocTapService>(sp => sp.GetRequiredService<KetQuaHocTapService>());
        services.AddScoped<IThongKeService, ThongKeService>();

        return services;
    }
}
