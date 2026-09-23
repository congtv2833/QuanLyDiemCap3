using QLDiem.Domain.Enums;

namespace QLDiem.Domain.Entities;

public class GiaoVien
{
    public int Id { get; set; }
    public string MaGiaoVien { get; set; } = string.Empty;
    public string HoTen { get; set; } = string.Empty;
    public GioiTinh GioiTinh { get; set; } = GioiTinh.Nam;
    public DateOnly? NgaySinh { get; set; }
    public string? ChuyenMon { get; set; }
    public string? Email { get; set; }
    public string? DienThoai { get; set; }
    public bool DangCongTac { get; set; } = true;

    /// <summary>Id tài khoản đăng nhập (ASP.NET Identity) tương ứng, nếu có.</summary>
    public string? UserId { get; set; }

    public ICollection<Lop> LopChuNhiem { get; set; } = new List<Lop>();
    public ICollection<PhanCongGiangDay> PhanCongs { get; set; } = new List<PhanCongGiangDay>();
}
