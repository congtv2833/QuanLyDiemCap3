namespace QLDiem.Models;

/// <summary>Học kỳ. CaNam dùng cho các bản ghi tổng kết cả năm.</summary>
public enum HocKy
{
    HocKyI = 1,
    HocKyII = 2,
    CaNam = 3
}

/// <summary>
/// Hình thức đánh giá môn học theo Thông tư 22/2021/TT-BGDĐT, Điều 5.
/// </summary>
public enum LoaiDanhGia
{
    /// <summary>Đánh giá bằng nhận xét kết hợp điểm số (Toán, Văn, Lý, ...).</summary>
    DiemSo = 1,

    /// <summary>Chỉ đánh giá bằng nhận xét: Đạt / Chưa đạt (GDTC, Âm nhạc, HĐTN-HN...).</summary>
    NhanXet = 2
}

/// <summary>Đầu điểm theo Thông tư 22/2021/TT-BGDĐT, Điều 6 và Điều 9.</summary>
public enum LoaiDiem
{
    /// <summary>ĐĐGtx - Đánh giá thường xuyên, hệ số 1.</summary>
    ThuongXuyen = 1,

    /// <summary>ĐĐGgk - Đánh giá giữa kỳ, hệ số 2.</summary>
    GiuaKy = 2,

    /// <summary>ĐĐGck - Đánh giá cuối kỳ, hệ số 3.</summary>
    CuoiKy = 3
}

/// <summary>Mức đánh giá kết quả học tập / rèn luyện theo Thông tư 22, Điều 9.</summary>
public enum MucDanhGia
{
    ChuaDat = 0,
    Dat = 1,
    Kha = 2,
    Tot = 3
}

/// <summary>Kết quả của môn chỉ đánh giá bằng nhận xét.</summary>
public enum KetQuaNhanXet
{
    ChuaDat = 0,
    Dat = 1
}

public enum GioiTinh
{
    Nam = 1,
    Nu = 2,
    Khac = 3
}

/// <summary>Danh hiệu thi đua cuối năm theo Thông tư 22, Điều 15.</summary>
public enum DanhHieu
{
    KhongDat = 0,
    HocSinhGioi = 1,
    HocSinhXuatSac = 2
}

/// <summary>Ba vai trò sử dụng hệ thống.</summary>
public static class VaiTro
{
    public const string Admin = "Admin";
    public const string GiaoVien = "GiaoVien";
    public const string HocSinh = "HocSinh";

    public static readonly string[] TatCa = { Admin, GiaoVien, HocSinh };

    public static string TenHienThi(string vaiTro) => vaiTro switch
    {
        Admin => "Quản trị viên",
        GiaoVien => "Giáo viên",
        HocSinh => "Học sinh",
        _ => vaiTro
    };
}
