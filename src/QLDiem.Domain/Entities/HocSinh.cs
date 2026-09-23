using QLDiem.Domain.Enums;

namespace QLDiem.Domain.Entities;

public class HocSinh
{
    public int Id { get; set; }
    public string MaHocSinh { get; set; } = string.Empty;
    public string HoTen { get; set; } = string.Empty;
    public DateOnly NgaySinh { get; set; }
    public GioiTinh GioiTinh { get; set; } = GioiTinh.Nam;
    public string? NoiSinh { get; set; }
    public string? DiaChi { get; set; }
    public string? DienThoai { get; set; }
    public string? Email { get; set; }
    public string? HoTenPhuHuynh { get; set; }
    public string? DienThoaiPhuHuynh { get; set; }
    public bool DangHoc { get; set; } = true;

    public int LopId { get; set; }
    public Lop Lop { get; set; } = null!;

    /// <summary>Id tài khoản đăng nhập (ASP.NET Identity) tương ứng, nếu có.</summary>
    public string? UserId { get; set; }

    public ICollection<Diem> DanhSachDiem { get; set; } = new List<Diem>();
    public ICollection<DanhGiaNhanXet> DanhSachNhanXet { get; set; } = new List<DanhGiaNhanXet>();
}
