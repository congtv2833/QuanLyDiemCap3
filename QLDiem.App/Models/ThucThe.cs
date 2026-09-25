using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using QLDiem.Services;

namespace QLDiem.Models;

// Các lớp thực thể ánh xạ xuống bảng trong cơ sở dữ liệu (EF Core Code First).
// Gom chung một file cho dễ theo dõi toàn bộ lược đồ.

/// <summary>Năm học, ví dụ "2025-2026".</summary>
public class NamHoc
{
    public int Id { get; set; }
    public string Ten { get; set; } = string.Empty;
    public DateOnly NgayBatDau { get; set; }
    public DateOnly NgayKetThuc { get; set; }

    /// <summary>Năm học đang diễn ra. Chỉ một năm học được phép bật cờ này.</summary>
    public bool DangHoatDong { get; set; }

    public List<Lop> DanhSachLop { get; set; } = new();
}

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

    public List<Lop> LopChuNhiem { get; set; } = new();
    public List<PhanCongGiangDay> PhanCongs { get; set; } = new();
}

public class Lop
{
    public int Id { get; set; }
    public string Ten { get; set; } = string.Empty;

    /// <summary>Khối 10, 11 hoặc 12.</summary>
    public int Khoi { get; set; }

    public int NamHocId { get; set; }
    [ValidateNever]
    public NamHoc NamHoc { get; set; } = null!;

    public int? GiaoVienChuNhiemId { get; set; }
    [ValidateNever]
    public GiaoVien? GiaoVienChuNhiem { get; set; }

    public List<HocSinh> HocSinhs { get; set; } = new();
    public List<PhanCongGiangDay> PhanCongs { get; set; } = new();
}

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
    [ValidateNever]
    public Lop Lop { get; set; } = null!;

    public List<Diem> DanhSachDiem { get; set; } = new();
    public List<DanhGiaNhanXet> DanhSachNhanXet { get; set; } = new();
}

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
    /// theo Thông tư 22, Điều 6 khoản 1 điểm b. Không lưu xuống cơ sở dữ liệu.
    /// </summary>
    public int SoDauDiemThuongXuyen => QuyCheDanhGia.SoDauDiemThuongXuyen(SoTietNam);

    public List<PhanCongGiangDay> PhanCongs { get; set; } = new();
}

/// <summary>Phân công một giáo viên dạy một môn ở một lớp trong năm học.</summary>
public class PhanCongGiangDay
{
    public int Id { get; set; }

    public int GiaoVienId { get; set; }
    [ValidateNever]
    public GiaoVien GiaoVien { get; set; } = null!;

    public int LopId { get; set; }
    [ValidateNever]
    public Lop Lop { get; set; } = null!;

    public int MonHocId { get; set; }
    [ValidateNever]
    public MonHoc MonHoc { get; set; } = null!;

    public int NamHocId { get; set; }
    [ValidateNever]
    public NamHoc NamHoc { get; set; } = null!;
}

/// <summary>
/// Một đầu điểm của học sinh ở một môn, trong một học kỳ.
/// Lưu theo dòng (không phải cột cố định) vì số ĐĐGtx thay đổi theo môn.
/// </summary>
public class Diem
{
    public int Id { get; set; }

    public int HocSinhId { get; set; }
    [ValidateNever]
    public HocSinh HocSinh { get; set; } = null!;

    public int MonHocId { get; set; }
    [ValidateNever]
    public MonHoc MonHoc { get; set; } = null!;

    public int NamHocId { get; set; }
    [ValidateNever]
    public NamHoc NamHoc { get; set; } = null!;

    public HocKy HocKy { get; set; }
    public LoaiDiem LoaiDiem { get; set; }

    /// <summary>Thứ tự đầu điểm trong cùng loại: 1..4 với ĐĐGtx, luôn là 1 với ĐĐGgk / ĐĐGck.</summary>
    public int ThuTu { get; set; } = 1;

    /// <summary>Giá trị điểm theo thang 10, lấy đến 1 chữ số thập phân.</summary>
    public decimal GiaTri { get; set; }

    /// <summary>Hình thức kiểm tra: hỏi đáp, thuyết trình, viết, thực hành, dự án...</summary>
    public string? HinhThuc { get; set; }

    public DateTime NgayNhap { get; set; } = DateTime.Now;
    public DateTime? NgayCapNhat { get; set; }
}

/// <summary>Kết quả Đạt / Chưa đạt của các môn chỉ đánh giá bằng nhận xét.</summary>
public class DanhGiaNhanXet
{
    public int Id { get; set; }

    public int HocSinhId { get; set; }
    [ValidateNever]
    public HocSinh HocSinh { get; set; } = null!;

    public int MonHocId { get; set; }
    [ValidateNever]
    public MonHoc MonHoc { get; set; } = null!;

    public int NamHocId { get; set; }
    [ValidateNever]
    public NamHoc NamHoc { get; set; } = null!;

    public HocKy HocKy { get; set; }
    public KetQuaNhanXet KetQua { get; set; } = KetQuaNhanXet.Dat;
    public string? NoiDungNhanXet { get; set; }
    public DateTime NgayNhap { get; set; } = DateTime.Now;
}

/// <summary>
/// Bản ghi tổng kết học kỳ / cả năm của một học sinh.
/// Được tính lại từ bảng điểm và lưu lại để tra cứu nhanh.
/// </summary>
public class KetQuaHocKy
{
    public int Id { get; set; }

    public int HocSinhId { get; set; }
    [ValidateNever]
    public HocSinh HocSinh { get; set; } = null!;

    public int NamHocId { get; set; }
    [ValidateNever]
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
