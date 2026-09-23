using QLDiem.Domain.Enums;

namespace QLDiem.Domain.Entities;

/// <summary>Kết quả Đạt / Chưa đạt của các môn chỉ đánh giá bằng nhận xét.</summary>
public class DanhGiaNhanXet
{
    public int Id { get; set; }

    public int HocSinhId { get; set; }
    public HocSinh HocSinh { get; set; } = null!;

    public int MonHocId { get; set; }
    public MonHoc MonHoc { get; set; } = null!;

    public int NamHocId { get; set; }
    public NamHoc NamHoc { get; set; } = null!;

    public HocKy HocKy { get; set; }
    public KetQuaNhanXet KetQua { get; set; } = KetQuaNhanXet.Dat;
    public string? NoiDungNhanXet { get; set; }
    public DateTime NgayNhap { get; set; } = DateTime.Now;
}
