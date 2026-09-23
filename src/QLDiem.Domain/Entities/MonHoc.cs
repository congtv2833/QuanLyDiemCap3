using QLDiem.Domain.Enums;
using QLDiem.Domain.QuyChe;

namespace QLDiem.Domain.Entities;

public class MonHoc
{
    public int Id { get; set; }
    public string MaMon { get; set; } = string.Empty;
    public string TenMon { get; set; } = string.Empty;

    /// <summary>Tổng số tiết trong một năm học. Quyết định số đầu điểm thường xuyên mỗi học kỳ.</summary>
    public int SoTietNam { get; set; }

    public LoaiDanhGia LoaiDanhGia { get; set; } = LoaiDanhGia.DiemSo;

    /// <summary>Môn bắt buộc hay môn lựa chọn (Chương trình GDPT 2018).</summary>
    public bool BatBuoc { get; set; } = true;

    public int ThuTuHienThi { get; set; }

    /// <summary>
    /// Số ĐĐGtx của môn trong một học kỳ, suy ra từ <see cref="SoTietNam"/>
    /// theo Thông tư 22/2021/TT-BGDĐT, Điều 6 khoản 1 điểm b.
    /// </summary>
    public int SoDauDiemThuongXuyen => QuyCheDanhGia.SoDauDiemThuongXuyen(SoTietNam);

    public ICollection<PhanCongGiangDay> PhanCongs { get; set; } = new List<PhanCongGiangDay>();
}
