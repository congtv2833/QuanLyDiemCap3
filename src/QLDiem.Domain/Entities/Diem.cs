using QLDiem.Domain.Enums;

namespace QLDiem.Domain.Entities;

/// <summary>
/// Một đầu điểm của học sinh ở một môn, trong một học kỳ.
/// Lưu theo dòng (không phải cột cố định) vì số ĐĐGtx thay đổi theo môn.
/// </summary>
public class Diem
{
    public int Id { get; set; }

    public int HocSinhId { get; set; }
    public HocSinh HocSinh { get; set; } = null!;

    public int MonHocId { get; set; }
    public MonHoc MonHoc { get; set; } = null!;

    public int NamHocId { get; set; }
    public NamHoc NamHoc { get; set; } = null!;

    public HocKy HocKy { get; set; }
    public LoaiDiem LoaiDiem { get; set; }

    /// <summary>Thứ tự đầu điểm trong cùng loại: 1..4 với ĐĐGtx, luôn là 1 với ĐĐGgk / ĐĐGck.</summary>
    public int ThuTu { get; set; } = 1;

    /// <summary>Giá trị điểm theo thang 10, lấy đến 1 chữ số thập phân.</summary>
    public decimal GiaTri { get; set; }

    /// <summary>Hình thức kiểm tra: hỏi đáp, thuyết trình, viết, thực hành, dự án...</summary>
    public string? HinhThuc { get; set; }

    public string? GhiChu { get; set; }
    public DateTime NgayNhap { get; set; } = DateTime.Now;
    public DateTime? NgayCapNhat { get; set; }
}
