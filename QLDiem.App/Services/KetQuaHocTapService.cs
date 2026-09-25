using Microsoft.EntityFrameworkCore;
using QLDiem.Data;
using QLDiem.Models;
using QLDiem.Models.ViewModels;

namespace QLDiem.Services;

/// <summary>Tổng kết điểm, xếp loại học tập và lập học bạ.</summary>
public class KetQuaHocTapService
{
    private readonly QLDiemDbContext _db;

    public KetQuaHocTapService(QLDiemDbContext db) => _db = db;

    /// <summary>Kết quả đã tính của một học sinh, dùng nội bộ giữa các hàm trong lớp này.</summary>
    public class KetQuaTinhToan
    {
        public HocSinh HocSinh { get; set; } = null!;
        public List<decimal> DiemTrungBinhCacMon { get; set; } = new();
        public List<KetQuaNhanXet> KetQuaCacMonNhanXet { get; set; } = new();
        public bool DuDuLieu { get; set; }

        public int SoMonTinhDiem => DiemTrungBinhCacMon.Count;

        public MucDanhGia XepLoai =>
            QuyCheDanhGia.XepLoaiHocTap(DiemTrungBinhCacMon, KetQuaCacMonNhanXet);

        public decimal? BinhQuanThamKhao => DiemTrungBinhCacMon.Count == 0
            ? null
            : QuyCheDanhGia.LamTron(DiemTrungBinhCacMon.Sum() / DiemTrungBinhCacMon.Count);
    }

    /// <summary>Học bạ của một học sinh trong năm học.</summary>
    public HocBaVM? LayHocBa(int hocSinhId, int namHocId)
    {
        var hs = _db.HocSinhs
            .Include(h => h.Lop).ThenInclude(l => l.NamHoc)
            .Include(h => h.Lop).ThenInclude(l => l.GiaoVienChuNhiem)
            .AsNoTracking()
            .FirstOrDefault(h => h.Id == hocSinhId);
        if (hs == null) return null;

        var monHocs = LayMonHocCuaLop(hs.LopId, namHocId);

        var diems = _db.Diems.AsNoTracking()
            .Where(d => d.HocSinhId == hocSinhId && d.NamHocId == namHocId)
            .ToList();

        var nhanXets = _db.DanhGiaNhanXets.AsNoTracking()
            .Where(n => n.HocSinhId == hocSinhId && n.NamHocId == namHocId)
            .ToList();

        var hocBa = new HocBaVM
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
            var dong = new DongHocBaVM
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
            ? m.DiemTrungBinhCaNam != null
            : m.NhanXetCaNam != null);

        var ketQuaCaNam = _db.KetQuaHocKys.AsNoTracking()
            .FirstOrDefault(k => k.HocSinhId == hocSinhId
                                 && k.NamHocId == namHocId
                                 && k.HocKy == HocKy.CaNam);

        hocBa.XepLoaiRenLuyen = ketQuaCaNam?.XepLoaiRenLuyen ?? MucDanhGia.Dat;
        hocBa.DanhHieu = QuyCheDanhGia.XetDanhHieu(
            hocBa.XepLoaiCaNam,
            hocBa.XepLoaiRenLuyen,
            hocBa.CacMon.Where(m => m.DiemTrungBinhCaNam != null)
                        .Select(m => m.DiemTrungBinhCaNam!.Value));

        return hocBa;
    }

    private static MucDanhGia XepLoai(HocBaVM hocBa, HocKy hocKy)
    {
        var diems = hocBa.CacMon
            .Where(m => m.LoaiDanhGia == LoaiDanhGia.DiemSo)
            .Select(m => hocKy switch
            {
                HocKy.HocKyI => m.DiemTrungBinhHocKyI,
                HocKy.HocKyII => m.DiemTrungBinhHocKyII,
                _ => m.DiemTrungBinhCaNam
            })
            .Where(d => d != null)
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
            .Where(k => k != null)
            .Select(k => k!.Value)
            .ToList();

        return QuyCheDanhGia.XepLoaiHocTap(diems, nhanXets);
    }

    /// <summary>Bảng tổng kết của cả lớp trong một học kỳ, sắp theo kết quả.</summary>
    public List<XepHangHocSinhVM> LayBangTongKetLop(int lopId, HocKy hocKy)
    {
        var lop = _db.Lops.AsNoTracking().FirstOrDefault(l => l.Id == lopId);
        if (lop == null) return new List<XepHangHocSinhVM>();

        var ketQua = TinhKetQuaLop(lopId, lop.NamHocId, hocKy);

        return ketQua
            .OrderByDescending(k => k.BinhQuanThamKhao ?? -1)
            .Select((k, i) => new XepHangHocSinhVM
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
    }

    /// <summary>Tính lại và ghi kết quả học kỳ cho toàn bộ học sinh của lớp.</summary>
    public KetQuaThaoTac TongKetLop(int lopId, HocKy hocKy)
    {
        var lop = _db.Lops.AsNoTracking().FirstOrDefault(l => l.Id == lopId);
        if (lop == null) return KetQuaThaoTac.ThatBai("Không tìm thấy lớp.");

        var ketQua = TinhKetQuaLop(lopId, lop.NamHocId, hocKy);
        if (ketQua.Count == 0) return KetQuaThaoTac.ThatBai("Lớp chưa có học sinh.");

        var idHocSinh = ketQua.Select(k => k.HocSinh.Id).ToList();
        var hienCo = _db.KetQuaHocKys
            .Where(k => k.NamHocId == lop.NamHocId
                        && k.HocKy == hocKy
                        && idHocSinh.Contains(k.HocSinhId))
            .ToList();

        int soChuaDuDuLieu = 0;

        foreach (var k in ketQua)
        {
            if (!k.DuDuLieu) soChuaDuDuLieu++;

            var ban = hienCo.FirstOrDefault(x => x.HocSinhId == k.HocSinh.Id);
            if (ban == null)
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

        _db.SaveChanges();

        var thongBao = $"Đã tổng kết {ketQua.Count} học sinh lớp {lop.Ten} - {QuyCheDanhGia.TenHocKy(hocKy)}.";
        if (soChuaDuDuLieu > 0)
            thongBao += $" Lưu ý: {soChuaDuDuLieu} học sinh chưa đủ đầu điểm ở một số môn.";

        return KetQuaThaoTac.Ok(thongBao);
    }

    /// <summary>Tính kết quả học tập của toàn bộ học sinh trong một lớp, cho một học kỳ.</summary>
    public List<KetQuaTinhToan> TinhKetQuaLop(int lopId, int namHocId, HocKy hocKy)
    {
        var hocSinhs = _db.HocSinhs.AsNoTracking()
            .Where(h => h.LopId == lopId && h.DangHoc)
            .OrderBy(h => h.MaHocSinh)
            .ToList();
        if (hocSinhs.Count == 0) return new List<KetQuaTinhToan>();

        var monHocs = LayMonHocCuaLop(lopId, namHocId);
        var monDiemSo = monHocs.Where(m => m.LoaiDanhGia == LoaiDanhGia.DiemSo).ToList();
        var monNhanXet = monHocs.Where(m => m.LoaiDanhGia == LoaiDanhGia.NhanXet).ToList();

        var idHocSinh = hocSinhs.Select(h => h.Id).ToList();

        var diems = _db.Diems.AsNoTracking()
            .Where(d => idHocSinh.Contains(d.HocSinhId) && d.NamHocId == namHocId)
            .ToList();

        var nhanXets = _db.DanhGiaNhanXets.AsNoTracking()
            .Where(n => idHocSinh.Contains(n.HocSinhId) && n.NamHocId == namHocId)
            .ToList();

        var ketQua = new List<KetQuaTinhToan>();

        foreach (var hs in hocSinhs)
        {
            var diemCuaHs = diems.Where(d => d.HocSinhId == hs.Id).ToList();
            var nxCuaHs = nhanXets.Where(n => n.HocSinhId == hs.Id).ToList();

            var mucTinh = new KetQuaTinhToan { HocSinh = hs, DuDuLieu = true };

            foreach (var mon in monDiemSo)
            {
                decimal? dtb = hocKy == HocKy.CaNam
                    ? QuyCheDanhGia.TinhDiemTrungBinhMonCaNam(
                        TinhDtbMon(diemCuaHs, mon.Id, HocKy.HocKyI),
                        TinhDtbMon(diemCuaHs, mon.Id, HocKy.HocKyII))
                    : TinhDtbMon(diemCuaHs, mon.Id, hocKy);

                if (dtb == null) mucTinh.DuDuLieu = false;
                else mucTinh.DiemTrungBinhCacMon.Add(dtb.Value);
            }

            foreach (var mon in monNhanXet)
            {
                // Cả năm lấy theo kết quả học kỳ II.
                var hocKyTra = hocKy == HocKy.CaNam ? HocKy.HocKyII : hocKy;
                var nx = nxCuaHs.FirstOrDefault(n => n.MonHocId == mon.Id && n.HocKy == hocKyTra);
                if (nx == null) mucTinh.DuDuLieu = false;
                else mucTinh.KetQuaCacMonNhanXet.Add(nx.KetQua);
            }

            ketQua.Add(mucTinh);
        }

        return ketQua;
    }

    private List<MonHoc> LayMonHocCuaLop(int lopId, int namHocId) =>
        _db.PhanCongGiangDays.AsNoTracking()
            .Where(p => p.LopId == lopId && p.NamHocId == namHocId)
            .Select(p => p.MonHoc)
            .OrderBy(m => m.ThuTuHienThi)
            .ToList();

    private static decimal? TinhDtbMon(IEnumerable<Diem> diems, int monHocId, HocKy hocKy)
    {
        var cuaMon = diems.Where(d => d.MonHocId == monHocId && d.HocKy == hocKy).ToList();
        if (cuaMon.Count == 0) return null;
        return QuyCheDanhGia.TinhDiemTrungBinhMonHocKy(DiemService.ToThanhPhan(cuaMon));
    }
}
