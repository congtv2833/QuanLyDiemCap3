using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using QLDiem.Services;

namespace QLDiem.Models;

// Các lớp thực thể ánh xạ xuống bảng trong cơ sở dữ liệu (EF Core Code First).
// Gom chung một file cho dễ theo dõi toàn bộ lược đồ.
//
// Các thuộc tính [Required], [StringLength], [RegularExpression]... vừa sinh ra
// kiểm tra ở trình duyệt (qua jQuery Validation), vừa được máy chủ kiểm tra lại
// trong ModelState - người dùng tắt JavaScript vẫn không lách qua được.

/// <summary>Năm học, ví dụ "2025-2026".</summary>
public class NamHoc
{
    public int Id { get; set; }

    [Display(Name = "Tên năm học")]
    [Required(ErrorMessage = "Vui lòng nhập tên năm học.")]
    [RegularExpression(QuyTacNhapLieu.TenNamHoc, ErrorMessage = QuyTacNhapLieu.LoiTenNamHoc)]
    public string Ten { get; set; } = string.Empty;

    [Display(Name = "Ngày bắt đầu")]
    [Required(ErrorMessage = "Vui lòng chọn ngày bắt đầu.")]
    [DataType(DataType.Date)]
    public DateOnly NgayBatDau { get; set; }

    [Display(Name = "Ngày kết thúc")]
    [Required(ErrorMessage = "Vui lòng chọn ngày kết thúc.")]
    [DataType(DataType.Date)]
    [SauNgay(nameof(NgayBatDau))]
    public DateOnly NgayKetThuc { get; set; }

    /// <summary>Năm học đang diễn ra. Chỉ một năm học được phép bật cờ này.</summary>
    [Display(Name = "Đang hoạt động")]
    public bool DangHoatDong { get; set; }

    [ValidateNever]
    public List<Lop> DanhSachLop { get; set; } = new();
}

public class GiaoVien
{
    public int Id { get; set; }

    [Display(Name = "Mã giáo viên")]
    [Required(ErrorMessage = "Vui lòng nhập mã giáo viên.")]
    [RegularExpression(QuyTacNhapLieu.MaDinhDanh, ErrorMessage = QuyTacNhapLieu.LoiMaDinhDanh)]
    public string MaGiaoVien { get; set; } = string.Empty;

    [Display(Name = "Họ và tên")]
    [Required(ErrorMessage = "Vui lòng nhập họ và tên.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Họ và tên phải từ 2 đến 100 ký tự.")]
    public string HoTen { get; set; } = string.Empty;

    [Display(Name = "Giới tính")]
    public GioiTinh GioiTinh { get; set; } = GioiTinh.Nam;

    [Display(Name = "Ngày sinh")]
    [DataType(DataType.Date)]
    [NgaySinhHopLe(TuoiToiThieu = 18, TuoiToiDa = 80)]
    public DateOnly? NgaySinh { get; set; }

    [Display(Name = "Chuyên môn")]
    [StringLength(100, ErrorMessage = "Chuyên môn tối đa 100 ký tự.")]
    public string? ChuyenMon { get; set; }

    [Display(Name = "Email")]
    [EmailAddress(ErrorMessage = "Email không đúng định dạng, ví dụ: tenban@thpt.edu.vn")]
    [StringLength(100)]
    public string? Email { get; set; }

    [Display(Name = "Điện thoại")]
    [RegularExpression(QuyTacNhapLieu.SoDienThoai, ErrorMessage = QuyTacNhapLieu.LoiSoDienThoai)]
    public string? DienThoai { get; set; }

    [Display(Name = "Đang công tác")]
    public bool DangCongTac { get; set; } = true;

    [ValidateNever]
    public List<Lop> LopChuNhiem { get; set; } = new();

    [ValidateNever]
    public List<PhanCongGiangDay> PhanCongs { get; set; } = new();
}

public class Lop
{
    public int Id { get; set; }

    [Display(Name = "Tên lớp")]
    [Required(ErrorMessage = "Vui lòng nhập tên lớp.")]
    [StringLength(20, MinimumLength = 2, ErrorMessage = "Tên lớp phải từ 2 đến 20 ký tự.")]
    public string Ten { get; set; } = string.Empty;

    /// <summary>Khối 10, 11 hoặc 12.</summary>
    [Display(Name = "Khối")]
    [Range(10, 12, ErrorMessage = "Khối phải là 10, 11 hoặc 12.")]
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

    [Display(Name = "Mã học sinh")]
    [Required(ErrorMessage = "Vui lòng nhập mã học sinh.")]
    [RegularExpression(QuyTacNhapLieu.MaDinhDanh, ErrorMessage = QuyTacNhapLieu.LoiMaDinhDanh)]
    public string MaHocSinh { get; set; } = string.Empty;

    [Display(Name = "Họ và tên")]
    [Required(ErrorMessage = "Vui lòng nhập họ và tên.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Họ và tên phải từ 2 đến 100 ký tự.")]
    public string HoTen { get; set; } = string.Empty;

    [Display(Name = "Ngày sinh")]
    [Required(ErrorMessage = "Vui lòng chọn ngày sinh.")]
    [DataType(DataType.Date)]
    [NgaySinhHopLe(TuoiToiThieu = 10, TuoiToiDa = 30)]
    public DateOnly NgaySinh { get; set; }

    [Display(Name = "Giới tính")]
    public GioiTinh GioiTinh { get; set; } = GioiTinh.Nam;

    [Display(Name = "Nơi sinh")]
    [StringLength(100, ErrorMessage = "Nơi sinh tối đa 100 ký tự.")]
    public string? NoiSinh { get; set; }

    [Display(Name = "Địa chỉ")]
    [StringLength(200, ErrorMessage = "Địa chỉ tối đa 200 ký tự.")]
    public string? DiaChi { get; set; }

    [Display(Name = "Điện thoại")]
    [RegularExpression(QuyTacNhapLieu.SoDienThoai, ErrorMessage = QuyTacNhapLieu.LoiSoDienThoai)]
    public string? DienThoai { get; set; }

    [Display(Name = "Email")]
    [EmailAddress(ErrorMessage = "Email không đúng định dạng, ví dụ: tenban@gmail.com")]
    [StringLength(100)]
    public string? Email { get; set; }

    [Display(Name = "Họ tên phụ huynh")]
    [StringLength(100, ErrorMessage = "Họ tên phụ huynh tối đa 100 ký tự.")]
    public string? HoTenPhuHuynh { get; set; }

    [Display(Name = "Điện thoại phụ huynh")]
    [RegularExpression(QuyTacNhapLieu.SoDienThoai, ErrorMessage = QuyTacNhapLieu.LoiSoDienThoai)]
    public string? DienThoaiPhuHuynh { get; set; }

    [Display(Name = "Đang theo học")]
    public bool DangHoc { get; set; } = true;

    [Display(Name = "Lớp")]
    [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn lớp.")]
    public int LopId { get; set; }

    [ValidateNever]
    public Lop Lop { get; set; } = null!;

    [ValidateNever]
    public List<Diem> DanhSachDiem { get; set; } = new();

    [ValidateNever]
    public List<DanhGiaNhanXet> DanhSachNhanXet { get; set; } = new();
}

public class MonHoc
{
    public int Id { get; set; }

    [Display(Name = "Mã môn")]
    [Required(ErrorMessage = "Vui lòng nhập mã môn.")]
    [RegularExpression(QuyTacNhapLieu.MaDinhDanh, ErrorMessage = QuyTacNhapLieu.LoiMaDinhDanh)]
    public string MaMon { get; set; } = string.Empty;

    [Display(Name = "Tên môn")]
    [Required(ErrorMessage = "Vui lòng nhập tên môn.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Tên môn phải từ 2 đến 100 ký tự.")]
    public string TenMon { get; set; } = string.Empty;

    /// <summary>Tổng số tiết trong một năm học. Quyết định số đầu điểm thường xuyên mỗi học kỳ.</summary>
    [Display(Name = "Số tiết trong năm")]
    [Range(1, 500, ErrorMessage = "Số tiết trong năm phải từ 1 đến 500.")]
    public int SoTietNam { get; set; }

    [Display(Name = "Hình thức đánh giá")]
    public LoaiDanhGia LoaiDanhGia { get; set; } = LoaiDanhGia.DiemSo;

    /// <summary>Môn bắt buộc hay môn lựa chọn (Chương trình GDPT 2018).</summary>
    [Display(Name = "Môn bắt buộc")]
    public bool BatBuoc { get; set; } = true;

    [Display(Name = "Thứ tự hiển thị")]
    [Range(1, 100, ErrorMessage = "Thứ tự hiển thị phải từ 1 đến 100.")]
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

    [Display(Name = "Giáo viên")]
    [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn giáo viên.")]
    public int GiaoVienId { get; set; }
    [ValidateNever]
    public GiaoVien GiaoVien { get; set; } = null!;

    [Display(Name = "Lớp")]
    [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn lớp.")]
    public int LopId { get; set; }
    [ValidateNever]
    public Lop Lop { get; set; } = null!;

    [Display(Name = "Môn học")]
    [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn môn học.")]
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
    [Display(Name = "Điểm")]
    [Range(0, 10, ErrorMessage = "Điểm phải nằm trong khoảng 0 đến 10.")]
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
