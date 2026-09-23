using QLDiem.Domain.Enums;

namespace QLDiem.Application.DTOs;

/// <summary>Bảng tổng hợp kết quả học tập cả năm của một học sinh.</summary>
public class HocBaDto
{
    public int HocSinhId { get; set; }
    public string MaHocSinh { get; set; } = string.Empty;
    public string HoTen { get; set; } = string.Empty;
    public DateOnly NgaySinh { get; set; }
    public string TenLop { get; set; } = string.Empty;
    public string TenNamHoc { get; set; } = string.Empty;
    public string? GiaoVienChuNhiem { get; set; }

    public List<DongHocBaDto> CacMon { get; set; } = new();

    public MucDanhGia XepLoaiHocKyI { get; set; }
    public MucDanhGia XepLoaiHocKyII { get; set; }
    public MucDanhGia XepLoaiCaNam { get; set; }
    public MucDanhGia XepLoaiRenLuyen { get; set; } = MucDanhGia.Dat;
    public DanhHieu DanhHieu { get; set; } = DanhHieu.KhongDat;

    /// <summary>Đã có đủ dữ liệu của cả hai học kỳ để tổng kết cả năm hay chưa.</summary>
    public bool DuDieuKienTongKet { get; set; }
}

/// <summary>Kết quả một môn học trong học bạ.</summary>
public class DongHocBaDto
{
    public int MonHocId { get; set; }
    public string TenMon { get; set; } = string.Empty;
    public LoaiDanhGia LoaiDanhGia { get; set; }
    public int ThuTuHienThi { get; set; }

    public decimal? DiemTrungBinhHocKyI { get; set; }
    public decimal? DiemTrungBinhHocKyII { get; set; }
    public decimal? DiemTrungBinhCaNam { get; set; }

    public KetQuaNhanXet? NhanXetHocKyI { get; set; }
    public KetQuaNhanXet? NhanXetHocKyII { get; set; }
    public KetQuaNhanXet? NhanXetCaNam { get; set; }
}

/// <summary>Chi tiết các đầu điểm của một học sinh ở một môn, một học kỳ.</summary>
public class ChiTietDiemMonDto
{
    public string TenMon { get; set; } = string.Empty;
    public HocKy HocKy { get; set; }
    public List<decimal> DiemThuongXuyen { get; set; } = new();
    public decimal? DiemGiuaKy { get; set; }
    public decimal? DiemCuoiKy { get; set; }
    public decimal? DiemTrungBinh { get; set; }
}
