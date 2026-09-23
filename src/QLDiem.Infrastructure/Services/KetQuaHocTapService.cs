using Microsoft.EntityFrameworkCore;
using QLDiem.Application.DTOs;
using QLDiem.Application.Interfaces;
using QLDiem.Domain.Entities;
using QLDiem.Domain.Enums;
using QLDiem.Domain.QuyChe;
using QLDiem.Infrastructure.Common;
using QLDiem.Infrastructure.Data;

namespace QLDiem.Infrastructure.Services;

public class KetQuaHocTapService : IKetQuaHocTapService
{
    private readonly QLDiemDbContext _db;

    public KetQuaHocTapService(QLDiemDbContext db) => _db = db;

    /// <summary>Kết quả đã tính của một học sinh, dùng nội bộ giữa các hàm trong lớp này.</summary>
    internal sealed record KetQuaTinhToan(
        HocSinh HocSinh,
        List<decimal> DiemTrungBinhCacMon,
        List<KetQuaNhanXet> KetQuaCacMonNhanXet,
        int SoMonTinhDiem,
        bool DuDuLieu)
    {
        public MucDanhGia XepLoai => QuyCheDanhGia.XepLoaiHocTap(DiemTrungBinhCacMon, KetQuaCacMonNhanXet);

        public decimal? BinhQuanThamKhao => DiemTrungBinhCacMon.Count == 0
            ? null
            : QuyCheDanhGia.LamTron(DiemTrungBinhCacMon.Sum() / DiemTrungBinhCacMon.Count);
    }

    public async Task<HocBaDto?> LayHocBaAsync(int hocSinhId, int namHocId)
    {
        var hs = await _db.HocSinhs
            .Include(h => h.Lop).ThenInclude(l => l.NamHoc)
            .Include(h => h.Lop).ThenInclude(l => l.GiaoVienChuNhiem)
            .AsNoTracking()
            .FirstOrDefaultAsync(h => h.Id == hocSinhId);
        if (hs is null) return null;

        var monHocs = await LayMonHocCuaLopAsync(hs.LopId, namHocId);

        var diems = await _db.Diems.AsNoTracking()
            .Where(d => d.HocSinhId == hocSinhId && d.NamHocId == namHocId)
            .ToListAsync();

        var nhanXets = await _db.DanhGiaNhanXets.AsNoTracking()
            .Where(n => n.HocSinhId == hocSinhId && n.NamHocId == namHocId)
            .ToListAsync();

        var hocBa = new HocBaDto
        {
            HocSinhId = hs.Id,
            MaHocSinh = hs.MaHocSinh,
            HoTen = hs.HoTen,
            NgaySinh = hs.NgaySinh,
            TenLop = hs.Lop.Ten,
            TenNamHoc = hs.Lop.NamHoc.Ten,
            GiaoVienChuNhiem = hs.Lop.GiaoVienChuNhiem?.HoTen
        };

        foreach (var mon in monHocs)
        {
            var dong = new DongHocBaDto
            {
                MonHocId = mon.Id,
                TenMon = mon.TenMon,
                LoaiDanhGia = mon.LoaiDanhGia,
                ThuTuHienThi = mon.ThuTuHienThi
            };

            if (mon.LoaiDanhGia == LoaiDanhGia.DiemSo)
            {
                dong.DiemTrungBinhHocKyI = TinhDtbMon(diems, mon.Id, HocKy.HocKyI);
                dong.DiemTrungBinhHocKyII = TinhDtbMon(diems, mon.Id, HocKy.HocKyII);
                dong.DiemTrungBinhCaNam = QuyCheDanhGia.TinhDiemTrungBinhMonCaNam(
                    dong.DiemTrungBinhHocKyI, dong.DiemTrungBinhHocKyII);
            }
            else
            {
                dong.NhanXetHocKyI = nhanXets
                    .FirstOrDefault(n => n.MonHocId == mon.Id && n.HocKy == HocKy.HocKyI)?.KetQua;
                dong.NhanXetHocKyII = nhanXets
                    .FirstOrDefault(n => n.MonHocId == mon.Id && n.HocKy == HocKy.HocKyII)?.KetQua;

                // Thông tư 22: kết quả cả năm của môn nhận xét lấy theo kết quả học kỳ II.
                dong.NhanXetCaNam = dong.NhanXetHocKyII;
            }

            hocBa.CacMon.Add(dong);
        }

        hocBa.XepLoaiHocKyI = XepLoai(hocBa, HocKy.HocKyI);
        hocBa.XepLoaiHocKyII = XepLoai(hocBa, HocKy.HocKyII);
        hocBa.XepLoaiCaNam = XepLoai(hocBa, HocKy.CaNam);

        hocBa.DuDieuKienTongKet = hocBa.CacMon.All(m => m.LoaiDanhGia == LoaiDanhGia.DiemSo
            ? m.DiemTrungBinhCaNam is not null
            : m.NhanXetCaNam is not null);

        var ketQuaCaNam = await _db.KetQuaHocKys.AsNoTracking()
            .FirstOrDefaultAsync(k => k.HocSinhId == hocSinhId
                                      && k.NamHocId == namHocId
                                      && k.HocKy == HocKy.CaNam);

        hocBa.XepLoaiRenLuyen = ketQuaCaNam?.XepLoaiRenLuyen ?? MucDanhGia.Dat;
        hocBa.DanhHieu = QuyCheDanhGia.XetDanhHieu(
            hocBa.XepLoaiCaNam,
            hocBa.XepLoaiRenLuyen,
            hocBa.CacMon.Where(m => m.DiemTrungBinhCaNam is not null)
                        .Select(m => m.DiemTrungBinhCaNam!.Value));

        return hocBa;
    }

    private static MucDanhGia XepLoai(HocBaDto hocBa, HocKy hocKy)
    {
        var diems = hocBa.CacMon
            .Where(m => m.LoaiDanhGia == LoaiDanhGia.DiemSo)
            .Select(m => hocKy switch
            {
                HocKy.HocKyI => m.DiemTrungBinhHocKyI,
                HocKy.HocKyII => m.DiemTrungBinhHocKyII,
                _ => m.DiemTrungBinhCaNam
            })
            .Where(d => d is not null)
            .Select(d => d!.Value)
            .ToList();

        var nhanXets = hocBa.CacMon
            .Where(m => m.LoaiDanhGia == LoaiDanhGia.NhanXet)
            .Select(m => hocKy switch
            {
                HocKy.HocKyI => m.NhanXetHocKyI,
                HocKy.HocKyII => m.NhanXetHocKyII,
                _ => m.NhanXetCaNam
            })
            .Where(k => k is not null)
            .Select(k => k!.Value)
            .ToList();

        return QuyCheDanhGia.XepLoaiHocTap(diems, nhanXets);
    }

    public async Task<List<XepHangHocSinhDto>> LayBangTongKetLopAsync(int lopId, HocKy hocKy)
    {
        var lop = await _db.Lops.AsNoTracking().FirstOrDefaultAsync(l => l.Id == lopId);
        if (lop is null) return new List<XepHangHocSinhDto>();

        var ketQua = await TinhKetQuaLopAsync(lopId, lop.NamHocId, hocKy);

        var danhSach = ketQua
            .OrderByDescending(k => k.BinhQuanThamKhao ?? -1)
            .Select((k, i) => new XepHangHocSinhDto
            {
                ThuHang = i + 1,
                HocSinhId = k.HocSinh.Id,
                MaHocSinh = k.HocSinh.MaHocSinh,
                HoTen = k.HocSinh.HoTen,
                TenLop = lop.Ten,
                XepLoai = k.XepLoai,
                SoMonTinhDiem = k.SoMonTinhDiem,
                DiemBinhQuanThamKhao = k.BinhQuanThamKhao
            })
            .ToList();

        return danhSach;
    }

    public async Task<KetQuaThaoTac> TongKetLopAsync(int lopId, HocKy hocKy)
    {
        var lop = await _db.Lops.AsNoTracking().FirstOrDefaultAsync(l => l.Id == lopId);
        if (lop is null) return KetQuaThaoTac.ThatBai("Không tìm thấy lớp.");

        var ketQua = await TinhKetQuaLopAsync(lopId, lop.NamHocId, hocKy);
        if (ketQua.Count == 0) return KetQuaThaoTac.ThatBai("Lớp chưa có học sinh.");

        var idHocSinh = ketQua.Select(k => k.HocSinh.Id).ToList();
        var hienCo = await _db.KetQuaHocKys
            .Where(k => k.NamHocId == lop.NamHocId
                        && k.HocKy == hocKy
                        && idHocSinh.Contains(k.HocSinhId))
            .ToListAsync();

        int soChuaDuDuLieu = 0;

        foreach (var k in ketQua)
        {
            if (!k.DuDuLieu) soChuaDuDuLieu++;

            var ban = hienCo.FirstOrDefault(x => x.HocSinhId == k.HocSinh.Id);
            if (ban is null)
            {
                ban = new KetQuaHocKy
                {
                    HocSinhId = k.HocSinh.Id,
                    NamHocId = lop.NamHocId,
                    HocKy = hocKy
                };
                _db.KetQuaHocKys.Add(ban);
            }

            ban.XepLoaiHocTap = k.XepLoai;
            ban.SoMonTinhDiem = k.SoMonTinhDiem;
            ban.NgayTongKet = DateTime.Now;

            // Kết quả rèn luyện do GVCN nhập, không bị ghi đè khi tổng kết lại.
            if (hocKy == HocKy.CaNam)
            {
                ban.DanhHieu = QuyCheDanhGia.XetDanhHieu(
                    k.XepLoai, ban.XepLoaiRenLuyen, k.DiemTrungBinhCacMon);
            }
        }

        await _db.SaveChangesAsync();

        var thongBao = $"Đã tổng kết {ketQua.Count} học sinh lớp {lop.Ten} - {QuyCheDanhGia.TenHocKy(hocKy)}.";
        if (soChuaDuDuLieu > 0)
            thongBao += $" Lưu ý: {soChuaDuDuLieu} học sinh chưa đủ đầu điểm ở một số môn.";

        return KetQuaThaoTac.Ok(thongBao);
    }

    /// <summary>Tính kết quả học tập của toàn bộ học sinh trong một lớp, cho một học kỳ.</summary>
    internal async Task<List<KetQuaTinhToan>> TinhKetQuaLopAsync(int lopId, int namHocId, HocKy hocKy)
    {
        var hocSinhs = await _db.HocSinhs.AsNoTracking()
            .Where(h => h.LopId == lopId && h.DangHoc)
            .ToListAsync();
        if (hocSinhs.Count == 0) return new List<KetQuaTinhToan>();

        var monHocs = await LayMonHocCuaLopAsync(lopId, namHocId);
        var monDiemSo = monHocs.Where(m => m.LoaiDanhGia == LoaiDanhGia.DiemSo).ToList();
        var monNhanXet = monHocs.Where(m => m.LoaiDanhGia == LoaiDanhGia.NhanXet).ToList();

        var idHocSinh = hocSinhs.Select(h => h.Id).ToList();

        var diems = await _db.Diems.AsNoTracking()
            .Where(d => idHocSinh.Contains(d.HocSinhId) && d.NamHocId == namHocId)
            .ToListAsync();

        var nhanXets = await _db.DanhGiaNhanXets.AsNoTracking()
            .Where(n => idHocSinh.Contains(n.HocSinhId) && n.NamHocId == namHocId)
            .ToListAsync();

        var ketQua = new List<KetQuaTinhToan>();

        foreach (var hs in hocSinhs.SapTheoTen(h => h.HoTen))
        {
            var diemCuaHs = diems.Where(d => d.HocSinhId == hs.Id).ToList();
            var nxCuaHs = nhanXets.Where(n => n.HocSinhId == hs.Id).ToList();

            var dtbCacMon = new List<decimal>();
            bool duDuLieu = true;

            foreach (var mon in monDiemSo)
            {
                decimal? dtb = hocKy == HocKy.CaNam
                    ? QuyCheDanhGia.TinhDiemTrungBinhMonCaNam(
                        TinhDtbMon(diemCuaHs, mon.Id, HocKy.HocKyI),
                        TinhDtbMon(diemCuaHs, mon.Id, HocKy.HocKyII))
                    : TinhDtbMon(diemCuaHs, mon.Id, hocKy);

                if (dtb is null) duDuLieu = false;
                else dtbCacMon.Add(dtb.Value);
            }

            var kqNhanXet = new List<KetQuaNhanXet>();
            foreach (var mon in monNhanXet)
            {
                // Cả năm lấy theo kết quả học kỳ II.
                var hocKyTra = hocKy == HocKy.CaNam ? HocKy.HocKyII : hocKy;
                var nx = nxCuaHs.FirstOrDefault(n => n.MonHocId == mon.Id && n.HocKy == hocKyTra);
                if (nx is null) duDuLieu = false;
                else kqNhanXet.Add(nx.KetQua);
            }

            ketQua.Add(new KetQuaTinhToan(hs, dtbCacMon, kqNhanXet, dtbCacMon.Count, duDuLieu));
        }

        return ketQua;
    }

    private async Task<List<MonHoc>> LayMonHocCuaLopAsync(int lopId, int namHocId) =>
        await _db.PhanCongGiangDays.AsNoTracking()
            .Where(p => p.LopId == lopId && p.NamHocId == namHocId)
            .Select(p => p.MonHoc)
            .OrderBy(m => m.ThuTuHienThi)
            .ToListAsync();

    private static decimal? TinhDtbMon(IEnumerable<Diem> diems, int monHocId, HocKy hocKy)
    {
        var cuaMon = diems.Where(d => d.MonHocId == monHocId && d.HocKy == hocKy).ToList();
        if (cuaMon.Count == 0) return null;
        return QuyCheDanhGia.TinhDiemTrungBinhMonHocKy(DiemService.ToThanhPhan(cuaMon));
    }
}
