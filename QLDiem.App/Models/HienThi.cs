using QLDiem.Services;

namespace QLDiem.Models;

/// <summary>Hàm hỗ trợ hiển thị dùng chung cho các view.</summary>
public static class HienThi
{
    /// <summary>Lớp CSS cho nhãn xếp loại.</summary>
    public static string CssXepLoai(MucDanhGia muc) => muc switch
    {
        MucDanhGia.Tot => "nhan-xl xl-tot",
        MucDanhGia.Kha => "nhan-xl xl-kha",
        MucDanhGia.Dat => "nhan-xl xl-dat",
        _ => "nhan-xl xl-chua-dat"
    };

    public static string TenXepLoai(MucDanhGia muc) => QuyCheDanhGia.TenMucDanhGia(muc);

    /// <summary>Hiển thị điểm theo kiểu Việt Nam (dấu phẩy thập phân); dấu gạch khi chưa có điểm.</summary>
    public static string Diem(decimal? giaTri) =>
        giaTri == null ? "-" : giaTri.Value.ToString("0.0").Replace('.', ',');

    /// <summary>Giá trị đưa vào ô input: dùng dấu chấm để trình duyệt hiểu là số.</summary>
    public static string DiemInput(decimal? giaTri) =>
        giaTri == null ? string.Empty : giaTri.Value.ToString("0.#");

    public static string KetQuaNhanXet(KetQuaNhanXet? kq) => kq switch
    {
        Models.KetQuaNhanXet.Dat => "Đạt",
        Models.KetQuaNhanXet.ChuaDat => "Chưa đạt",
        _ => "-"
    };

    public static string CssKetQuaNhanXet(KetQuaNhanXet? kq) => kq switch
    {
        Models.KetQuaNhanXet.Dat => "nhan-xl xl-tot",
        Models.KetQuaNhanXet.ChuaDat => "nhan-xl xl-chua-dat",
        _ => "text-muted"
    };

    public static string TenGioiTinh(GioiTinh gt) => gt switch
    {
        GioiTinh.Nam => "Nam",
        GioiTinh.Nu => "Nữ",
        _ => "Khác"
    };

    /// <summary>Màu thanh tiến độ theo tỷ lệ đạt.</summary>
    public static string MauTyLe(double tyLe) => tyLe switch
    {
        >= 80 => "bg-success",
        >= 60 => "bg-primary",
        >= 40 => "bg-warning",
        _ => "bg-danger"
    };
}
