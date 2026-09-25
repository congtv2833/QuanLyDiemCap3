using QLDiem.Models;


namespace QLDiem.Services;

/// <summary>
/// Toàn bộ quy tắc đánh giá học sinh THPT theo Thông tư 22/2021/TT-BGDĐT.
/// Các hàm ở đây là hàm thuần: không chạm cơ sở dữ liệu, không phụ thuộc hạ tầng,
/// nhờ vậy có thể kiểm thử đơn vị trực tiếp (xem project QLDiem.Tests).
/// </summary>
public static class QuyCheDanhGia
{
    public const decimal DiemToiThieu = 0m;
    public const decimal DiemToiDa = 10m;

    /// <summary>Số môn tối thiểu phải đạt ngưỡng điểm khi xếp loại (Điều 9).</summary>
    public const int SoMonToiThieuDatNguong = 6;

    /// <summary>Hệ số của từng đầu điểm: ĐĐGtx = 1, ĐĐGgk = 2, ĐĐGck = 3 (Điều 9 khoản 1).</summary>
    public static int HeSo(LoaiDiem loaiDiem) => loaiDiem switch
    {
        LoaiDiem.ThuongXuyen => 1,
        LoaiDiem.GiuaKy => 2,
        LoaiDiem.CuoiKy => 3,
        _ => throw new ArgumentOutOfRangeException(nameof(loaiDiem))
    };

    /// <summary>
    /// Số ĐĐGtx trong một học kỳ, xác định theo tổng số tiết của môn trong năm học
    /// (Điều 6 khoản 1 điểm b):
    /// tối đa 35 tiết thì 2 đầu điểm; 36-70 tiết thì 3 đầu điểm; trên 70 tiết thì 4 đầu điểm.
    /// </summary>
    public static int SoDauDiemThuongXuyen(int soTietNam) => soTietNam switch
    {
        <= 0 => 0,
        <= 35 => 2,
        <= 70 => 3,
        _ => 4
    };

    /// <summary>Điểm hợp lệ khi nằm trong đoạn [0; 10].</summary>
    public static bool LaDiemHopLe(decimal giaTri) => giaTri >= DiemToiThieu && giaTri <= DiemToiDa;

    /// <summary>Làm tròn đến chữ số thập phân thứ nhất (Điều 9 khoản 1).</summary>
    public static decimal LamTron(decimal giaTri) =>
        Math.Round(giaTri, 1, MidpointRounding.AwayFromZero);

    /// <summary>
    /// Kiểm tra một học kỳ đã có đủ đầu điểm để tổng kết chưa:
    /// đủ số ĐĐGtx theo quy định, có 01 ĐĐGgk và 01 ĐĐGck.
    /// </summary>
    public static bool DaDuDauDiem(IEnumerable<DiemThanhPhan> diems, int soDauDiemTxYeuCau)
    {
        var ds = diems as ICollection<DiemThanhPhan> ?? diems.ToList();
        return ds.Count(d => d.LoaiDiem == LoaiDiem.ThuongXuyen) >= soDauDiemTxYeuCau
               && ds.Count(d => d.LoaiDiem == LoaiDiem.GiuaKy) >= 1
               && ds.Count(d => d.LoaiDiem == LoaiDiem.CuoiKy) >= 1;
    }

    /// <summary>
    /// ĐTBmhk = (Tổng ĐĐGtx + 2 x ĐĐGgk + 3 x ĐĐGck) / (Số ĐĐGtx + 5).
    /// Trả về null khi thiếu ĐĐGgk hoặc ĐĐGck - lúc đó môn chưa đủ điều kiện tổng kết.
    /// </summary>
    public static decimal? TinhDiemTrungBinhMonHocKy(IEnumerable<DiemThanhPhan> diems)
    {
        var ds = diems as ICollection<DiemThanhPhan> ?? diems.ToList();

        var dsGiuaKy = ds.Where(d => d.LoaiDiem == LoaiDiem.GiuaKy).ToList();
        var dsCuoiKy = ds.Where(d => d.LoaiDiem == LoaiDiem.CuoiKy).ToList();
        if (dsGiuaKy.Count == 0 || dsCuoiKy.Count == 0) return null;

        var thuongXuyen = ds.Where(d => d.LoaiDiem == LoaiDiem.ThuongXuyen).ToList();

        decimal tuSo = thuongXuyen.Sum(d => d.GiaTri)
                       + HeSo(LoaiDiem.GiuaKy) * dsGiuaKy[0].GiaTri
                       + HeSo(LoaiDiem.CuoiKy) * dsCuoiKy[0].GiaTri;

        int mauSo = thuongXuyen.Count + HeSo(LoaiDiem.GiuaKy) + HeSo(LoaiDiem.CuoiKy);

        return LamTron(tuSo / mauSo);
    }

    /// <summary>
    /// ĐTBmcn = (ĐTBmhkI + 2 x ĐTBmhkII) / 3 - học kỳ II tính hệ số 2 (Điều 9 khoản 2).
    /// Trả về null khi thiếu điểm trung bình của một trong hai học kỳ.
    /// </summary>
    public static decimal? TinhDiemTrungBinhMonCaNam(decimal? dtbHocKyI, decimal? dtbHocKyII)
    {
        if (dtbHocKyI is null || dtbHocKyII is null) return null;
        return LamTron((dtbHocKyI.Value + 2 * dtbHocKyII.Value) / 3);
    }

    /// <summary>
    /// Xếp loại kết quả học tập theo Thông tư 22, Điều 9 khoản 3.
    /// Lưu ý: từ Thông tư 22 KHÔNG còn khái niệm điểm trung bình chung tất cả các môn;
    /// việc xếp loại dựa trên bộ tiêu chí dưới đây.
    /// </summary>
    /// <param name="dtbCacMon">ĐTBmhk (hoặc ĐTBmcn) của các môn đánh giá bằng điểm số.</param>
    /// <param name="ketQuaCacMonNhanXet">Kết quả Đạt / Chưa đạt của các môn chỉ nhận xét.</param>
    public static MucDanhGia XepLoaiHocTap(
        IEnumerable<decimal> dtbCacMon,
        IEnumerable<KetQuaNhanXet> ketQuaCacMonNhanXet)
    {
        var diems = dtbCacMon as IReadOnlyCollection<decimal> ?? dtbCacMon.ToList();
        var nhanXets = ketQuaCacMonNhanXet as IReadOnlyCollection<KetQuaNhanXet> ?? ketQuaCacMonNhanXet.ToList();

        if (diems.Count == 0) return MucDanhGia.ChuaDat;

        int soMonChuaDatNhanXet = nhanXets.Count(k => k == KetQuaNhanXet.ChuaDat);
        bool tatCaNhanXetDat = soMonChuaDatNhanXet == 0;

        // Mức Tốt: mọi môn nhận xét Đạt; mọi ĐTBm >= 6,5; ít nhất 6 môn >= 8,0.
        if (tatCaNhanXetDat
            && diems.All(d => d >= 6.5m)
            && diems.Count(d => d >= 8.0m) >= SoMonToiThieuDatNguong)
        {
            return MucDanhGia.Tot;
        }

        // Mức Khá: mọi môn nhận xét Đạt; mọi ĐTBm >= 5,0; ít nhất 6 môn >= 6,5.
        if (tatCaNhanXetDat
            && diems.All(d => d >= 5.0m)
            && diems.Count(d => d >= 6.5m) >= SoMonToiThieuDatNguong)
        {
            return MucDanhGia.Kha;
        }

        // Mức Đạt: nhiều nhất 1 môn nhận xét Chưa đạt; ít nhất 6 môn ĐTBm >= 5,0;
        // không môn nào dưới 3,5.
        if (soMonChuaDatNhanXet <= 1
            && diems.Count(d => d >= 5.0m) >= SoMonToiThieuDatNguong
            && diems.All(d => d >= 3.5m))
        {
            return MucDanhGia.Dat;
        }

        return MucDanhGia.ChuaDat;
    }

    /// <summary>
    /// Danh hiệu thi đua cuối năm theo Thông tư 22, Điều 15:
    /// - Học sinh Xuất sắc: rèn luyện Tốt, học tập Tốt và có ít nhất 06 môn ĐTBmcn >= 9,0.
    /// - Học sinh Giỏi: rèn luyện Tốt và học tập Tốt.
    /// </summary>
    public static DanhHieu XetDanhHieu(
        MucDanhGia xepLoaiHocTap,
        MucDanhGia xepLoaiRenLuyen,
        IEnumerable<decimal> dtbCacMonCaNam)
    {
        if (xepLoaiHocTap != MucDanhGia.Tot || xepLoaiRenLuyen != MucDanhGia.Tot)
            return DanhHieu.KhongDat;

        int soMonTu9 = dtbCacMonCaNam.Count(d => d >= 9.0m);
        return soMonTu9 >= SoMonToiThieuDatNguong ? DanhHieu.HocSinhXuatSac : DanhHieu.HocSinhGioi;
    }

    /// <summary>Tên hiển thị tiếng Việt của mức đánh giá.</summary>
    public static string TenMucDanhGia(MucDanhGia muc) => muc switch
    {
        MucDanhGia.Tot => "Tốt",
        MucDanhGia.Kha => "Khá",
        MucDanhGia.Dat => "Đạt",
        _ => "Chưa đạt"
    };

    /// <summary>Tên viết tắt của đầu điểm, dùng làm tiêu đề cột bảng điểm.</summary>
    public static string TenLoaiDiem(LoaiDiem loai) => loai switch
    {
        LoaiDiem.ThuongXuyen => "ĐĐGtx",
        LoaiDiem.GiuaKy => "ĐĐGgk",
        LoaiDiem.CuoiKy => "ĐĐGck",
        _ => "?"
    };

    /// <summary>Tên đầy đủ của đầu điểm, dùng cho chú thích trên giao diện.</summary>
    public static string TenDayDuLoaiDiem(LoaiDiem loai) => loai switch
    {
        LoaiDiem.ThuongXuyen => "Đánh giá thường xuyên (hệ số 1)",
        LoaiDiem.GiuaKy => "Đánh giá giữa kỳ (hệ số 2)",
        LoaiDiem.CuoiKy => "Đánh giá cuối kỳ (hệ số 3)",
        _ => "?"
    };

    public static string TenHocKy(HocKy hocKy) => hocKy switch
    {
        HocKy.HocKyI => "Học kỳ I",
        HocKy.HocKyII => "Học kỳ II",
        _ => "Cả năm"
    };

    public static string TenDanhHieu(DanhHieu danhHieu) => danhHieu switch
    {
        DanhHieu.HocSinhXuatSac => "Học sinh Xuất sắc",
        DanhHieu.HocSinhGioi => "Học sinh Giỏi",
        _ => "-"
    };
}
