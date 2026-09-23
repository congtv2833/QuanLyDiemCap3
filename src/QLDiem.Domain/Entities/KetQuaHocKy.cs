using QLDiem.Domain.Enums;

namespace QLDiem.Domain.Entities;

/// <summary>
/// Bản ghi tổng kết học kỳ / cả năm của một học sinh.
/// Được tính lại từ bảng điểm và lưu lại để phục vụ tra cứu và thống kê nhanh.
/// </summary>
public class KetQuaHocKy
{
    public int Id { get; set; }

    public int HocSinhId { get; set; }
    public HocSinh HocSinh { get; set; } = null!;

    public int NamHocId { get; set; }
    public NamHoc NamHoc { get; set; } = null!;

    public HocKy HocKy { get; set; }

    public MucDanhGia XepLoaiHocTap { get; set; }
    public MucDanhGia XepLoaiRenLuyen { get; set; } = MucDanhGia.Dat;
    public DanhHieu DanhHieu { get; set; } = DanhHieu.KhongDat;

    /// <summary>Số môn tính điểm đã có đủ dữ liệu để tổng kết.</summary>
    public int SoMonTinhDiem { get; set; }

    public string? NhanXetCuaGVCN { get; set; }
    public DateTime NgayTongKet { get; set; } = DateTime.Now;
}
