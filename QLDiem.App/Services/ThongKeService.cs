using Microsoft.EntityFrameworkCore;
using QLDiem.Data;
using QLDiem.Models;
using QLDiem.Models.ViewModels;

namespace QLDiem.Services;

/// <summary>Thống kê, tổng hợp kết quả học tập.</summary>
public class ThongKeService
{
    private readonly QLDiemDbContext _db;
    private readonly KetQuaHocTapService _ketQua;

    public ThongKeService(QLDiemDbContext db, KetQuaHocTapService ketQua)
    {
        _db = db;
        _ketQua = ketQua;
    }

    /// <summary>Bản rút gọn của một đầu điểm, chỉ chứa các trường cần cho thống kê.</summary>
    private class DiemRutGon
    {
        public int HocSinhId { get; set; }
        public int MonHocId { get; set; }
        public HocKy HocKy { get; set; }
        public LoaiDiem LoaiDiem { get; set; }
        public decimal GiaTri { get; set; }
    }

    /// <summary>Số liệu tổng quan của năm học đang hoạt động, dùng cho trang chủ.</summary>
    public TongQuanVM LayTongQuan(HocKy hocKy)
    {
        var namHoc = LayNamHocHienHanh();
        if (namHoc == null) return new TongQuanVM { TenNamHoc = "Chưa có năm học" };

        var idLop = _db.Lops.AsNoTracking()
            .Where(l => l.NamHocId == namHoc.Id)
            .Select(l => l.Id)
            .ToList();

        return new TongQuanVM
        {
            TenNamHoc = namHoc.Ten,
            SoLop = idLop.Count,
            SoHocSinh = _db.HocSinhs.Count(h => idLop.Contains(h.LopId) && h.DangHoc),
            SoGiaoVien = _db.GiaoViens.Count(g => g.DangCongTac),
            SoMonHoc = _db.MonHocs.Count(),
            SoDauDiemDaNhap = _db.Diems.Count(d => d.NamHocId == namHoc.Id),
            ThongKeTheoLop = ThongKeTheoLop(namHoc.Id, hocKy)
        };
    }

    /// <summary>Phân bố xếp loại học tập của từng lớp trong một học kỳ.</summary>
    public List<ThongKeLopVM> ThongKeTheoLop(int namHocId, HocKy hocKy, int? khoi = null)
    {
        var lops = _db.Lops.AsNoTracking()
            .Where(l => l.NamHocId == namHocId && (khoi == null || l.Khoi == khoi))
            .OrderBy(l => l.Khoi).ThenBy(l => l.Ten)
            .ToList();

        var ketQua = new List<ThongKeLopVM>();

        foreach (var lop in lops)
        {
            var tinhToan = _ketQua.TinhKetQuaLop(lop.Id, namHocId, hocKy);

            ketQua.Add(new ThongKeLopVM
            {
                LopId = lop.Id,
                TenLop = lop.Ten,
                Khoi = lop.Khoi,
                SiSo = tinhToan.Count,
                SoTot = tinhToan.Count(k => k.XepLoai == MucDanhGia.Tot),
                SoKha = tinhToan.Count(k => k.XepLoai == MucDanhGia.Kha),
                SoDat = tinhToan.Count(k => k.XepLoai == MucDanhGia.Dat),
                SoChuaDat = tinhToan.Count(k => k.XepLoai == MucDanhGia.ChuaDat)
            });
        }

        return ketQua;
    }

    /// <summary>Phổ điểm từng môn; truyền lopId để giới hạn trong một lớp.</summary>
    public List<ThongKeMonVM> ThongKeTheoMon(int namHocId, HocKy hocKy, int? lopId = null)
    {
        var monHocs = _db.MonHocs.AsNoTracking()
            .Where(m => m.LoaiDanhGia == LoaiDanhGia.DiemSo)
            .OrderBy(m => m.ThuTuHienThi)
            .ToList();

        var truyVan = _db.Diems.AsNoTracking().Where(d => d.NamHocId == namHocId);

        if (lopId != null)
            truyVan = truyVan.Where(d => d.HocSinh.LopId == lopId);

        // Cả năm cần dữ liệu của cả hai học kỳ nên lấy hết rồi tính trong bộ nhớ.
        if (hocKy != HocKy.CaNam)
            truyVan = truyVan.Where(d => d.HocKy == hocKy);

        var diems = truyVan
            .Select(d => new DiemRutGon
            {
                HocSinhId = d.HocSinhId,
                MonHocId = d.MonHocId,
                HocKy = d.HocKy,
                LoaiDiem = d.LoaiDiem,
                GiaTri = d.GiaTri
            })
            .ToList();

        var ketQua = new List<ThongKeMonVM>();

        foreach (var mon in monHocs)
        {
            var cuaMon = diems.Where(d => d.MonHocId == mon.Id).ToList();
            if (cuaMon.Count == 0) continue;

            var dtbTungHocSinh = new List<decimal>();

            foreach (var nhom in cuaMon.GroupBy(d => d.HocSinhId))
            {
                decimal? dtb = hocKy == HocKy.CaNam
                    ? QuyCheDanhGia.TinhDiemTrungBinhMonCaNam(
                        Dtb(nhom.Where(d => d.HocKy == HocKy.HocKyI)),
                        Dtb(nhom.Where(d => d.HocKy == HocKy.HocKyII)))
                    : Dtb(nhom);

                if (dtb != null) dtbTungHocSinh.Add(dtb.Value);
            }

            if (dtbTungHocSinh.Count == 0) continue;

            ketQua.Add(new ThongKeMonVM
            {
                MonHocId = mon.Id,
                TenMon = mon.TenMon,
                SoHocSinhCoDiem = dtbTungHocSinh.Count,
                DiemTrungBinh = QuyCheDanhGia.LamTron(dtbTungHocSinh.Sum() / dtbTungHocSinh.Count),
                DiemCaoNhat = dtbTungHocSinh.Max(),
                DiemThapNhat = dtbTungHocSinh.Min(),
                SoGioi = dtbTungHocSinh.Count(d => d >= 8.0m),
                SoKha = dtbTungHocSinh.Count(d => d >= 6.5m && d < 8.0m),
                SoTrungBinh = dtbTungHocSinh.Count(d => d >= 5.0m && d < 6.5m),
                SoYeu = dtbTungHocSinh.Count(d => d >= 3.5m && d < 5.0m),
                SoKem = dtbTungHocSinh.Count(d => d < 3.5m)
            });
        }

        return ketQua;
    }

    /// <summary>Năm học đang hoạt động; nếu chưa đặt thì lấy năm học mới nhất.</summary>
    public NamHoc? LayNamHocHienHanh() =>
        _db.NamHocs.AsNoTracking().FirstOrDefault(n => n.DangHoatDong)
        ?? _db.NamHocs.AsNoTracking().OrderByDescending(n => n.Id).FirstOrDefault();

    private static decimal? Dtb(IEnumerable<DiemRutGon> nguon)
    {
        var ds = nguon.Select(d => new DiemThanhPhan(d.LoaiDiem, d.GiaTri)).ToList();
        return ds.Count == 0 ? null : QuyCheDanhGia.TinhDiemTrungBinhMonHocKy(ds);
    }
}
