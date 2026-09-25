using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace QLDiem.Models;

/// <summary>
/// Gắn thêm GiaoVienId và HocSinhId vào phiếu đăng nhập ngay lúc người dùng đăng nhập.
/// Nhờ vậy các controller đọc được hai giá trị này trực tiếp từ User, không phải
/// truy vấn cơ sở dữ liệu lại mỗi request.
/// </summary>
public class NguoiDungClaimsFactory : UserClaimsPrincipalFactory<ApplicationUser, IdentityRole>
{
    public const string ClaimGiaoVienId = "GiaoVienId";
    public const string ClaimHocSinhId = "HocSinhId";
    public const string ClaimHoTen = "HoTen";

    public NguoiDungClaimsFactory(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IOptions<IdentityOptions> options)
        : base(userManager, roleManager, options) { }

    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user)
    {
        var identity = await base.GenerateClaimsAsync(user);

        if (user.GiaoVienId is not null)
            identity.AddClaim(new Claim(ClaimGiaoVienId, user.GiaoVienId.Value.ToString()));

        if (user.HocSinhId is not null)
            identity.AddClaim(new Claim(ClaimHocSinhId, user.HocSinhId.Value.ToString()));

        if (!string.IsNullOrWhiteSpace(user.HoTen))
            identity.AddClaim(new Claim(ClaimHoTen, user.HoTen));

        return identity;
    }
}

/// <summary>Các hàm tiện ích đọc thông tin người đang đăng nhập.</summary>
public static class NguoiDungExtensions
{
    /// <summary>Id hồ sơ giáo viên của người đang đăng nhập; null nếu không phải giáo viên.</summary>
    public static int? LayGiaoVienId(this ClaimsPrincipal user) =>
        DocSo(user, NguoiDungClaimsFactory.ClaimGiaoVienId);

    /// <summary>Id hồ sơ học sinh của người đang đăng nhập; null nếu không phải học sinh.</summary>
    public static int? LayHocSinhId(this ClaimsPrincipal user) =>
        DocSo(user, NguoiDungClaimsFactory.ClaimHocSinhId);

    public static string LayHoTen(this ClaimsPrincipal user) =>
        user.FindFirst(NguoiDungClaimsFactory.ClaimHoTen)?.Value
        ?? user.Identity?.Name
        ?? string.Empty;

    private static int? DocSo(ClaimsPrincipal user, string tenClaim)
    {
        var giaTri = user.FindFirst(tenClaim)?.Value;
        return int.TryParse(giaTri, out var so) ? so : null;
    }
}
