using System.ComponentModel.DataAnnotations;
using QLDiem.Application.DTOs;
using QLDiem.Domain.Enums;

namespace QLDiem.Web.Models;

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

/// <summary>Bộ lọc chọn lớp - môn - học kỳ ở màn hình nhập điểm.</summary>
public class ChonBangDiemVM
{
    public int? LopId { get; set; }
    public int? MonHocId { get; set; }
    public HocKy HocKy { get; set; } = HocKy.HocKyI;

    public List<LopGonVM> DanhSachLop { get; set; } = new();
    public List<MonHocGonVM> DanhSachMon { get; set; } = new();
    public BangDiemLopDto? BangDiem { get; set; }
}

public record LopGonVM(int Id, string Ten, int Khoi);

public record MonHocGonVM(int Id, string TenMon, LoaiDanhGia LoaiDanhGia, int SoDauDiemThuongXuyen);

/// <summary>Bộ lọc của màn hình thống kê.</summary>
public class ThongKeVM
{
    public int NamHocId { get; set; }
    public string TenNamHoc { get; set; } = string.Empty;
    public HocKy HocKy { get; set; } = HocKy.HocKyI;
    public int? Khoi { get; set; }
    public int? LopId { get; set; }

    public List<LopGonVM> DanhSachLop { get; set; } = new();
    public List<ThongKeLopDto> TheoLop { get; set; } = new();
    public List<ThongKeMonDto> TheoMon { get; set; } = new();
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
