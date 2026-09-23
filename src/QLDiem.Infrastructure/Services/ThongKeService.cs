using Microsoft.EntityFrameworkCore;
using QLDiem.Application.DTOs;
using QLDiem.Application.Interfaces;
using QLDiem.Domain.Enums;
using QLDiem.Domain.QuyChe;
using QLDiem.Infrastructure.Data;

namespace QLDiem.Infrastructure.Services;

public class ThongKeService : IThongKeService
{
    private readonly QLDiemDbContext _db;
    private readonly KetQuaHocTapService _ketQua;

    public ThongKeService(QLDiemDbContext db, KetQuaHocTapService ketQua)
    {
        _db = db;
        _ketQua = ketQua;
    }

    /// <summary>Bản rút gọn của một đầu điểm, chỉ chứa các trường cần cho thống kê.</summary>
    private sealed record DiemRutGon(int HocSinhId, int MonHocId, HocKy HocKy, LoaiDiem LoaiDiem, decimal GiaTri);

    public async Task<TongQuanDto> LayTongQuanAsync(HocKy hocKy)
    {
        var namHoc = await _db.NamHocs.AsNoTracking()
            .FirstOrDefaultAsync(n => n.DangHoatDong)
            ?? await _db.NamHocs.AsNoTracking().OrderByDescending(n => n.Id).FirstOrDefaultAsync();

        if (namHoc is null) return new TongQuanDto { TenNamHoc = "Chưa có năm học" };

        var idLop = await _db.Lops.AsNoTracking()
            .Where(l => l.NamHocId == namHoc.Id)
            .Select(l => l.Id)
            .ToListAsync();

        return new TongQuanDto
        {
            TenNamHoc = namHoc.Ten,
            SoLop = idLop.Count,
            SoHocSinh = await _db.HocSinhs.CountAsync(h => idLop.Contains(h.LopId) && h.DangHoc),
            SoGiaoVien = await _db.GiaoViens.CountAsync(g => g.DangCongTac),
            SoMonHoc = await _db.MonHocs.CountAsync(),
            SoDauDiemDaNhap = await _db.Diems.CountAsync(d => d.NamHocId == namHoc.Id),
            ThongKeTheoLop = await ThongKeTheoLopAsync(namHoc.Id, hocKy)
        };
    }

    public async Task<List<ThongKeLopDto>> ThongKeTheoLopAsync(int namHocId, HocKy hocKy, int? khoi = null)
    {
        var lops = await _db.Lops.AsNoTracking()
            .Where(l => l.NamHocId == namHocId && (khoi == null || l.Khoi == khoi))
            .OrderBy(l => l.Khoi).ThenBy(l => l.Ten)
            .ToListAsync();

        var ketQua = new List<ThongKeLopDto>();

        foreach (var lop in lops)
        {
            var tinhToan = await _ketQua.TinhKetQuaLopAsync(lop.Id, namHocId, hocKy);

            ketQua.Add(new ThongKeLopDto
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

    public async Task<List<ThongKeMonDto>> ThongKeTheoMonAsync(int namHocId, HocKy hocKy, int? lopId = null)
    {
        var monHocs = await _db.MonHocs.AsNoTracking()
            .Where(m => m.LoaiDanhGia == LoaiDanhGia.DiemSo)
            .OrderBy(m => m.ThuTuHienThi)
            .ToListAsync();

        var truyVan = _db.Diems.AsNoTracking()
            .Where(d => d.NamHocId == namHocId);

        if (lopId is not null)
            truyVan = truyVan.Where(d => d.HocSinh.LopId == lopId);

        // Cả năm cần dữ liệu của cả hai học kỳ nên lấy hết rồi tính trong bộ nhớ.
        if (hocKy != HocKy.CaNam)
            truyVan = truyVan.Where(d => d.HocKy == hocKy);

        var diems = await truyVan
            .Select(d => new DiemRutGon(d.HocSinhId, d.MonHocId, d.HocKy, d.LoaiDiem, d.GiaTri))
            .ToListAsync();

        var ketQua = new List<ThongKeMonDto>();

        foreach (var mon in monHocs)
        {
            var cuaMon = diems.Where(d => d.MonHocId == mon.Id).ToList();
            if (cuaMon.Count == 0) continue;

            var dtbTungHocSinh = new List<decimal>();

            foreach (var nhom in cuaMon.GroupBy(d => d.HocSinhId))
            {
                decimal? dtb;
                if (hocKy == HocKy.CaNam)
                {
                    dtb = QuyCheDanhGia.TinhDiemTrungBinhMonCaNam(
                        Dtb(nhom.Where(d => d.HocKy == HocKy.HocKyI)),
                        Dtb(nhom.Where(d => d.HocKy == HocKy.HocKyII)));
                }
                else
                {
                    dtb = Dtb(nhom);
                }

                if (dtb is not null) dtbTungHocSinh.Add(dtb.Value);
            }

            if (dtbTungHocSinh.Count == 0) continue;

            ketQua.Add(new ThongKeMonDto
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

        static decimal? Dtb(IEnumerable<DiemRutGon> nguon)
        {
            var ds = nguon
                .Select(d => new Domain.Common.DiemThanhPhan(d.LoaiDiem, d.GiaTri))
                .ToList();
            return ds.Count == 0 ? null : QuyCheDanhGia.TinhDiemTrungBinhMonHocKy(ds);
        }
    }
}
