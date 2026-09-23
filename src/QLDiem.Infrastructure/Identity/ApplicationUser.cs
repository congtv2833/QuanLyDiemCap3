using Microsoft.AspNetCore.Identity;

namespace QLDiem.Infrastructure.Identity;

/// <summary>Tài khoản đăng nhập. Gắn với hồ sơ giáo viên hoặc học sinh tương ứng.</summary>
public class ApplicationUser : IdentityUser
{
    public string HoTen { get; set; } = string.Empty;

    /// <summary>Id hồ sơ giáo viên, nếu tài khoản thuộc vai trò Giáo viên.</summary>
    public int? GiaoVienId { get; set; }

    /// <summary>Id hồ sơ học sinh, nếu tài khoản thuộc vai trò Học sinh.</summary>
    public int? HocSinhId { get; set; }

    public bool DangHoatDong { get; set; } = true;
}
