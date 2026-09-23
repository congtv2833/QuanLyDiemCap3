using QLDiem.Domain.Enums;

namespace QLDiem.Application.DTOs;

/// <summary>Bảng điểm của một lớp, cho một môn học, trong một học kỳ.</summary>
public class BangDiemLopDto
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

    public List<DongBangDiemDto> DanhSach { get; set; } = new();
}

/// <summary>Một dòng trong bảng điểm - tương ứng một học sinh.</summary>
public class DongBangDiemDto
{
    public int HocSinhId { get; set; }
    public string MaHocSinh { get; set; } = string.Empty;
    public string HoTen { get; set; } = string.Empty;

    /// <summary>Các ĐĐGtx theo thứ tự 1..n; phần tử null nghĩa là chưa nhập.</summary>
    public decimal?[] DiemThuongXuyen { get; set; } = [];

    public decimal? DiemGiuaKy { get; set; }
    public decimal? DiemCuoiKy { get; set; }

    /// <summary>ĐTBmhk được tính lại mỗi lần tải bảng điểm; null khi chưa đủ điểm.</summary>
    public decimal? DiemTrungBinh { get; set; }

    /// <summary>Đã nhập đủ số đầu điểm theo quy định hay chưa.</summary>
    public bool DaDuDiem { get; set; }

    // Dành cho môn chỉ đánh giá bằng nhận xét
    public KetQuaNhanXet? KetQuaNhanXet { get; set; }
    public string? NoiDungNhanXet { get; set; }
}

/// <summary>Dữ liệu người dùng gửi lên khi lưu một bảng điểm.</summary>
public class LuuBangDiemDto
{
    public int NamHocId { get; set; }
    public int LopId { get; set; }
    public int MonHocId { get; set; }
    public HocKy HocKy { get; set; }
    public List<LuuDiemHocSinhDto> DanhSach { get; set; } = new();
}

public class LuuDiemHocSinhDto
{
    public int HocSinhId { get; set; }
    public decimal?[] DiemThuongXuyen { get; set; } = [];
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
