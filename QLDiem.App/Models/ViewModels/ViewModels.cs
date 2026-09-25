using System.ComponentModel.DataAnnotations;

namespace QLDiem.Models.ViewModels;

// ---------- Đăng nhập, tài khoản ----------

public class DangNhapVM
{
    [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập.")]
    [Display(Name = "Tên đăng nhập")]
    public string TenDangNhap { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập mật khẩu.")]
    [DataType(DataType.Password)]
    [Display(Name = "Mật khẩu")]
    public string MatKhau { get; set; } = string.Empty;

    [Display(Name = "Ghi nhớ đăng nhập")]
    public bool GhiNho { get; set; }

    public string? ReturnUrl { get; set; }
}

public class DoiMatKhauVM
{
    [Required(ErrorMessage = "Vui lòng nhập mật khẩu hiện tại.")]
    [DataType(DataType.Password)]
    [Display(Name = "Mật khẩu hiện tại")]
    public string MatKhauCu { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập mật khẩu mới.")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu mới phải có ít nhất 6 ký tự.")]
    [DataType(DataType.Password)]
    [Display(Name = "Mật khẩu mới")]
    public string MatKhauMoi { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Display(Name = "Xác nhận mật khẩu mới")]
    [Compare(nameof(MatKhauMoi), ErrorMessage = "Xác nhận mật khẩu không khớp.")]
    public string XacNhan { get; set; } = string.Empty;
}

// ---------- Bảng điểm ----------

/// <summary>Bảng điểm của một lớp, cho một môn học, trong một học kỳ.</summary>
public class BangDiemLopVM
{
    public int NamHocId { get; set; }
    public string TenNamHoc { get; set; } = string.Empty;
    public int LopId { get; set; }
    public string TenLop { get; set; } = string.Empty;
    public int MonHocId { get; set; }
    public string TenMon { get; set; } = string.Empty;
    public HocKy HocKy { get; set; }

    public LoaiDanhGia LoaiDanhGia { get; set; }

    /// <summary>Số cột ĐĐGtx phải hiển thị, suy ra từ số tiết của môn.</summary>
    public int SoDauDiemThuongXuyen { get; set; }

    public List<DongBangDiemVM> DanhSach { get; set; } = new();
}

/// <summary>Một dòng trong bảng điểm - tương ứng một học sinh.</summary>
public class DongBangDiemVM
{
    public int HocSinhId { get; set; }
    public string MaHocSinh { get; set; } = string.Empty;
    public string HoTen { get; set; } = string.Empty;

    /// <summary>Các ĐĐGtx theo thứ tự 1..n; phần tử null nghĩa là chưa nhập.</summary>
    public decimal?[] DiemThuongXuyen { get; set; } = Array.Empty<decimal?>();

    public decimal? DiemGiuaKy { get; set; }
    public decimal? DiemCuoiKy { get; set; }

    /// <summary>ĐTBmhk được tính lại mỗi lần tải bảng điểm; null khi chưa đủ điểm.</summary>
    public decimal? DiemTrungBinh { get; set; }

    public bool DaDuDiem { get; set; }

    // Dành cho môn chỉ đánh giá bằng nhận xét
    public KetQuaNhanXet? KetQuaNhanXet { get; set; }
    public string? NoiDungNhanXet { get; set; }
}

/// <summary>Dữ liệu người dùng gửi lên khi lưu một bảng điểm.</summary>
public class LuuBangDiemVM
{
    public int NamHocId { get; set; }
    public int LopId { get; set; }
    public int MonHocId { get; set; }
    public HocKy HocKy { get; set; }
    public List<LuuDiemHocSinhVM> DanhSach { get; set; } = new();
}

public class LuuDiemHocSinhVM
{
    public int HocSinhId { get; set; }
    public decimal?[] DiemThuongXuyen { get; set; } = Array.Empty<decimal?>();
    public decimal? DiemGiuaKy { get; set; }
    public decimal? DiemCuoiKy { get; set; }
    public KetQuaNhanXet? KetQuaNhanXet { get; set; }
    public string? NoiDungNhanXet { get; set; }
}

/// <summary>Kết quả của một thao tác nghiệp vụ, kèm danh sách lỗi để hiển thị lên giao diện.</summary>
public class KetQuaThaoTac
{
    public bool ThanhCong => Loi.Count == 0;
    public List<string> Loi { get; } = new();
    public string? ThongBao { get; set; }

    public static KetQuaThaoTac Ok(string? thongBao = null) => new() { ThongBao = thongBao };

    public static KetQuaThaoTac ThatBai(params string[] loi)
    {
        var kq = new KetQuaThaoTac();
        kq.Loi.AddRange(loi);
        return kq;
    }
}

/// <summary>Bộ lọc chọn lớp - môn - học kỳ ở màn hình nhập điểm.</summary>
public class ChonBangDiemVM
{
    public int? LopId { get; set; }
    public int? MonHocId { get; set; }
    public HocKy HocKy { get; set; } = HocKy.HocKyI;

    public List<LopGonVM> DanhSachLop { get; set; } = new();
    public List<MonHocGonVM> DanhSachMon { get; set; } = new();
    public BangDiemLopVM? BangDiem { get; set; }
}

public record LopGonVM(int Id, string Ten, int Khoi);

public record MonHocGonVM(int Id, string TenMon, LoaiDanhGia LoaiDanhGia, int SoDauDiemThuongXuyen);

// ---------- Học bạ ----------

/// <summary>Bảng tổng hợp kết quả học tập cả năm của một học sinh.</summary>
public class HocBaVM
{
    public int HocSinhId { get; set; }
    public string MaHocSinh { get; set; } = string.Empty;
    public string HoTen { get; set; } = string.Empty;
    public DateOnly NgaySinh { get; set; }
    public string TenLop { get; set; } = string.Empty;
    public string TenNamHoc { get; set; } = string.Empty;
    public string? GiaoVienChuNhiem { get; set; }

    public List<DongHocBaVM> CacMon { get; set; } = new();

    public MucDanhGia XepLoaiHocKyI { get; set; }
    public MucDanhGia XepLoaiHocKyII { get; set; }
    public MucDanhGia XepLoaiCaNam { get; set; }
    public MucDanhGia XepLoaiRenLuyen { get; set; } = MucDanhGia.Dat;
    public DanhHieu DanhHieu { get; set; } = DanhHieu.KhongDat;

    public bool DuDieuKienTongKet { get; set; }
}

public class DongHocBaVM
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
public class ChiTietDiemMonVM
{
    public string TenMon { get; set; } = string.Empty;
    public HocKy HocKy { get; set; }
    public List<decimal> DiemThuongXuyen { get; set; } = new();
    public decimal? DiemGiuaKy { get; set; }
    public decimal? DiemCuoiKy { get; set; }
    public decimal? DiemTrungBinh { get; set; }
}

/// <summary>Màn hình tra cứu: tìm học sinh rồi xem học bạ.</summary>
public class TraCuuVM
{
    public string? TuKhoa { get; set; }
    public int? LopId { get; set; }
    public List<LopGonVM> DanhSachLop { get; set; } = new();
    public List<KetQuaTimHocSinhVM> KetQua { get; set; } = new();
}

public record KetQuaTimHocSinhVM(int Id, string MaHocSinh, string HoTen, string TenLop, DateOnly NgaySinh);

// ---------- Thống kê ----------

public class ThongKeLopVM
{
    public int LopId { get; set; }
    public string TenLop { get; set; } = string.Empty;
    public int Khoi { get; set; }
    public int SiSo { get; set; }
    public int SoTot { get; set; }
    public int SoKha { get; set; }
    public int SoDat { get; set; }
    public int SoChuaDat { get; set; }

    public double TyLeTuDatTroLen => SiSo == 0 ? 0 : Math.Round((SoTot + SoKha + SoDat) * 100.0 / SiSo, 1);
}

public class ThongKeMonVM
{
    public int MonHocId { get; set; }
    public string TenMon { get; set; } = string.Empty;
    public int SoHocSinhCoDiem { get; set; }
    public decimal? DiemTrungBinh { get; set; }
    public decimal? DiemCaoNhat { get; set; }
    public decimal? DiemThapNhat { get; set; }

    public int SoGioi { get; set; }      // >= 8,0
    public int SoKha { get; set; }       // 6,5 - 7,9
    public int SoTrungBinh { get; set; } // 5,0 - 6,4
    public int SoYeu { get; set; }       // 3,5 - 4,9
    public int SoKem { get; set; }       // < 3,5

    public double TyLeTuTrungBinh => SoHocSinhCoDiem == 0
        ? 0
        : Math.Round((SoGioi + SoKha + SoTrungBinh) * 100.0 / SoHocSinhCoDiem, 1);
}

/// <summary>Số liệu tổng quan hiển thị ở trang chủ.</summary>
public class TongQuanVM
{
    public string TenNamHoc { get; set; } = string.Empty;
    public int SoHocSinh { get; set; }
    public int SoGiaoVien { get; set; }
    public int SoLop { get; set; }
    public int SoMonHoc { get; set; }
    public int SoDauDiemDaNhap { get; set; }
    public List<ThongKeLopVM> ThongKeTheoLop { get; set; } = new();
}

/// <summary>Bộ lọc của màn hình thống kê.</summary>
public class ThongKeVM
{
    public int NamHocId { get; set; }
    public string TenNamHoc { get; set; } = string.Empty;
    public HocKy HocKy { get; set; } = HocKy.HocKyI;
    public int? Khoi { get; set; }
    public int? LopId { get; set; }

    public List<LopGonVM> DanhSachLop { get; set; } = new();
    public List<ThongKeLopVM> TheoLop { get; set; } = new();
    public List<ThongKeMonVM> TheoMon { get; set; } = new();
}

/// <summary>Một học sinh trong bảng tổng kết của lớp.</summary>
public class XepHangHocSinhVM
{
    public int ThuHang { get; set; }
    public int HocSinhId { get; set; }
    public string MaHocSinh { get; set; } = string.Empty;
    public string HoTen { get; set; } = string.Empty;
    public string TenLop { get; set; } = string.Empty;
    public MucDanhGia XepLoai { get; set; }
    public int SoMonTinhDiem { get; set; }

    /// <summary>
    /// Trung bình cộng ĐTBm các môn - chỉ dùng để sắp thứ tự hiển thị,
    /// KHÔNG phải căn cứ xếp loại theo Thông tư 22.
    /// </summary>
    public decimal? DiemBinhQuanThamKhao { get; set; }
}

public class ErrorViewModel
{
    public string? RequestId { get; set; }
    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
}
