using Microsoft.EntityFrameworkCore;
using QLDiem.Application.DTOs;
using QLDiem.Application.Interfaces;
using QLDiem.Domain.Common;
using QLDiem.Domain.Entities;
using QLDiem.Domain.Enums;
using QLDiem.Domain.QuyChe;
using QLDiem.Infrastructure.Common;
using QLDiem.Infrastructure.Data;

namespace QLDiem.Infrastructure.Services;

public class DiemService : IDiemService
{
    private readonly QLDiemDbContext _db;

    public DiemService(QLDiemDbContext db) => _db = db;

    public async Task<BangDiemLopDto?> LayBangDiemLopAsync(int lopId, int monHocId, HocKy hocKy)
    {
        var lop = await _db.Lops.Include(l => l.NamHoc)
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.Id == lopId);
        var mon = await _db.MonHocs.AsNoTracking().FirstOrDefaultAsync(m => m.Id == monHocId);
        if (lop is null || mon is null) return null;

        var hocSinhs = await _db.HocSinhs.AsNoTracking()
            .Where(h => h.LopId == lopId && h.DangHoc)
            .ToListAsync();

        var diems = await _db.Diems.AsNoTracking()
            .Where(d => d.MonHocId == monHocId
                        && d.NamHocId == lop.NamHocId
                        && d.HocKy == hocKy
                        && d.HocSinh.LopId == lopId)
            .ToListAsync();

        var nhanXets = await _db.DanhGiaNhanXets.AsNoTracking()
            .Where(n => n.MonHocId == monHocId
                        && n.NamHocId == lop.NamHocId
                        && n.HocKy == hocKy
                        && n.HocSinh.LopId == lopId)
            .ToListAsync();

        int soTx = mon.SoDauDiemThuongXuyen;

        var bang = new BangDiemLopDto
        {
            NamHocId = lop.NamHocId,
            TenNamHoc = lop.NamHoc.Ten,
            LopId = lop.Id,
            TenLop = lop.Ten,
            MonHocId = mon.Id,
            TenMon = mon.TenMon,
            HocKy = hocKy,
            LoaiDanhGia = mon.LoaiDanhGia,
            SoDauDiemThuongXuyen = soTx
        };

        foreach (var hs in hocSinhs.SapTheoTen(h => h.HoTen))
        {
            var cuaHs = diems.Where(d => d.HocSinhId == hs.Id).ToList();

            var tx = new decimal?[soTx];
            foreach (var d in cuaHs.Where(d => d.LoaiDiem == LoaiDiem.ThuongXuyen))
            {
                if (d.ThuTu >= 1 && d.ThuTu <= soTx) tx[d.ThuTu - 1] = d.GiaTri;
            }

            var gk = cuaHs.FirstOrDefault(d => d.LoaiDiem == LoaiDiem.GiuaKy)?.GiaTri;
            var ck = cuaHs.FirstOrDefault(d => d.LoaiDiem == LoaiDiem.CuoiKy)?.GiaTri;

            var thanhPhan = ToThanhPhan(cuaHs);
            var nx = nhanXets.FirstOrDefault(n => n.HocSinhId == hs.Id);

            bang.DanhSach.Add(new DongBangDiemDto
            {
                HocSinhId = hs.Id,
                MaHocSinh = hs.MaHocSinh,
                HoTen = hs.HoTen,
                DiemThuongXuyen = tx,
                DiemGiuaKy = gk,
                DiemCuoiKy = ck,
                DiemTrungBinh = QuyCheDanhGia.TinhDiemTrungBinhMonHocKy(thanhPhan),
                DaDuDiem = QuyCheDanhGia.DaDuDauDiem(thanhPhan, soTx),
                KetQuaNhanXet = nx?.KetQua,
                NoiDungNhanXet = nx?.NoiDungNhanXet
            });
        }

        return bang;
    }

    public async Task<KetQuaThaoTac> LuuBangDiemAsync(LuuBangDiemDto duLieu)
    {
        var mon = await _db.MonHocs.FirstOrDefaultAsync(m => m.Id == duLieu.MonHocId);
        if (mon is null) return KetQuaThaoTac.ThatBai("Không tìm thấy môn học.");

        if (duLieu.HocKy == HocKy.CaNam)
            return KetQuaThaoTac.ThatBai("Chỉ nhập điểm cho Học kỳ I hoặc Học kỳ II.");

        var idHocSinh = duLieu.DanhSach.Select(x => x.HocSinhId).ToList();
        var tenHocSinh = await _db.HocSinhs
            .Where(h => idHocSinh.Contains(h.Id) && h.LopId == duLieu.LopId)
            .ToDictionaryAsync(h => h.Id, h => h.HoTen);
        var hocSinhHopLe = tenHocSinh.Keys.ToList();

        // Kiểm tra dữ liệu trước khi ghi: chỉ ghi khi toàn bộ bảng hợp lệ.
        var loi = new List<string>();
        int soTx = mon.SoDauDiemThuongXuyen;

        foreach (var dong in duLieu.DanhSach)
        {
            if (!tenHocSinh.TryGetValue(dong.HocSinhId, out var hoTen))
            {
                loi.Add($"Học sinh #{dong.HocSinhId} không thuộc lớp được chọn.");
                continue;
            }

            if (mon.LoaiDanhGia == LoaiDanhGia.NhanXet) continue;

            foreach (var (giaTri, i) in dong.DiemThuongXuyen.Select((g, i) => (g, i)))
            {
                if (i >= soTx && giaTri is not null)
                    loi.Add($"Môn {mon.TenMon} chỉ có {soTx} đầu điểm thường xuyên.");
                if (giaTri is not null && !QuyCheDanhGia.LaDiemHopLe(giaTri.Value))
                    loi.Add($"ĐĐGtx{i + 1} của {hoTen} phải nằm trong khoảng 0 - 10.");
            }

            if (dong.DiemGiuaKy is not null && !QuyCheDanhGia.LaDiemHopLe(dong.DiemGiuaKy.Value))
                loi.Add($"ĐĐGgk của {hoTen} phải nằm trong khoảng 0 - 10.");

            if (dong.DiemCuoiKy is not null && !QuyCheDanhGia.LaDiemHopLe(dong.DiemCuoiKy.Value))
                loi.Add($"ĐĐGck của {hoTen} phải nằm trong khoảng 0 - 10.");
        }

        if (loi.Count > 0) return KetQuaThaoTac.ThatBai(loi.Distinct().ToArray());

        if (mon.LoaiDanhGia == LoaiDanhGia.NhanXet)
            return await LuuNhanXetAsync(duLieu, hocSinhHopLe);

        var hienCo = await _db.Diems
            .Where(d => d.MonHocId == duLieu.MonHocId
                        && d.NamHocId == duLieu.NamHocId
                        && d.HocKy == duLieu.HocKy
                        && idHocSinh.Contains(d.HocSinhId))
            .ToListAsync();

        foreach (var dong in duLieu.DanhSach)
        {
            for (int i = 0; i < soTx; i++)
            {
                decimal? giaTri = i < dong.DiemThuongXuyen.Length ? dong.DiemThuongXuyen[i] : null;
                GhiDauDiem(hienCo, duLieu, dong.HocSinhId, LoaiDiem.ThuongXuyen, i + 1, giaTri);
            }

            GhiDauDiem(hienCo, duLieu, dong.HocSinhId, LoaiDiem.GiuaKy, 1, dong.DiemGiuaKy);
            GhiDauDiem(hienCo, duLieu, dong.HocSinhId, LoaiDiem.CuoiKy, 1, dong.DiemCuoiKy);
        }

        await _db.SaveChangesAsync();
        return KetQuaThaoTac.Ok($"Đã lưu bảng điểm môn {mon.TenMon}.");
    }

    /// <summary>Ghi một đầu điểm: thêm mới, cập nhật, hoặc xóa khi người dùng để trống ô.</summary>
    private void GhiDauDiem(
        List<Diem> hienCo, LuuBangDiemDto duLieu, int hocSinhId,
        LoaiDiem loaiDiem, int thuTu, decimal? giaTri)
    {
        var cu = hienCo.FirstOrDefault(d => d.HocSinhId == hocSinhId
                                            && d.LoaiDiem == loaiDiem
                                            && d.ThuTu == thuTu);

        if (giaTri is null)
        {
            if (cu is not null) _db.Diems.Remove(cu);
            return;
        }

        var giaTriLamTron = QuyCheDanhGia.LamTron(giaTri.Value);

        if (cu is null)
        {
            _db.Diems.Add(new Diem
            {
                HocSinhId = hocSinhId,
                MonHocId = duLieu.MonHocId,
                NamHocId = duLieu.NamHocId,
                HocKy = duLieu.HocKy,
                LoaiDiem = loaiDiem,
                ThuTu = thuTu,
                GiaTri = giaTriLamTron,
                NgayNhap = DateTime.Now
            });
        }
        else if (cu.GiaTri != giaTriLamTron)
        {
            cu.GiaTri = giaTriLamTron;
            cu.NgayCapNhat = DateTime.Now;
        }
    }

    private async Task<KetQuaThaoTac> LuuNhanXetAsync(LuuBangDiemDto duLieu, List<int> hocSinhHopLe)
    {
        var idHocSinh = duLieu.DanhSach.Select(x => x.HocSinhId).ToList();

        var hienCo = await _db.DanhGiaNhanXets
            .Where(n => n.MonHocId == duLieu.MonHocId
                        && n.NamHocId == duLieu.NamHocId
                        && n.HocKy == duLieu.HocKy
                        && idHocSinh.Contains(n.HocSinhId))
            .ToListAsync();

        foreach (var dong in duLieu.DanhSach.Where(d => hocSinhHopLe.Contains(d.HocSinhId)))
        {
            var cu = hienCo.FirstOrDefault(n => n.HocSinhId == dong.HocSinhId);

            if (dong.KetQuaNhanXet is null)
            {
                if (cu is not null) _db.DanhGiaNhanXets.Remove(cu);
                continue;
            }

            if (cu is null)
            {
                _db.DanhGiaNhanXets.Add(new DanhGiaNhanXet
                {
                    HocSinhId = dong.HocSinhId,
                    MonHocId = duLieu.MonHocId,
                    NamHocId = duLieu.NamHocId,
                    HocKy = duLieu.HocKy,
                    KetQua = dong.KetQuaNhanXet.Value,
                    NoiDungNhanXet = dong.NoiDungNhanXet
                });
            }
            else
            {
                cu.KetQua = dong.KetQuaNhanXet.Value;
                cu.NoiDungNhanXet = dong.NoiDungNhanXet;
            }
        }

        await _db.SaveChangesAsync();
        return KetQuaThaoTac.Ok("Đã lưu kết quả đánh giá.");
    }

    public async Task<List<ChiTietDiemMonDto>> LayChiTietDiemAsync(int hocSinhId, HocKy hocKy)
    {
        var hs = await _db.HocSinhs.Include(h => h.Lop).AsNoTracking()
            .FirstOrDefaultAsync(h => h.Id == hocSinhId);
        if (hs is null) return new List<ChiTietDiemMonDto>();

        var diems = await _db.Diems.Include(d => d.MonHoc).AsNoTracking()
            .Where(d => d.HocSinhId == hocSinhId
                        && d.NamHocId == hs.Lop.NamHocId
                        && d.HocKy == hocKy)
            .ToListAsync();

        // Gom theo MonHocId, không gom theo đối tượng MonHoc: truy vấn AsNoTracking
        // tạo một instance MonHoc riêng cho mỗi dòng điểm nên so sánh tham chiếu sẽ sai.
        return diems
            .GroupBy(d => d.MonHocId)
            .Select(g => new { Mon = g.First().MonHoc, Diems = g.ToList() })
            .OrderBy(x => x.Mon.ThuTuHienThi)
            .Select(x => new ChiTietDiemMonDto
            {
                TenMon = x.Mon.TenMon,
                HocKy = hocKy,
                DiemThuongXuyen = x.Diems.Where(d => d.LoaiDiem == LoaiDiem.ThuongXuyen)
                                         .OrderBy(d => d.ThuTu)
                                         .Select(d => d.GiaTri).ToList(),
                DiemGiuaKy = x.Diems.FirstOrDefault(d => d.LoaiDiem == LoaiDiem.GiuaKy)?.GiaTri,
                DiemCuoiKy = x.Diems.FirstOrDefault(d => d.LoaiDiem == LoaiDiem.CuoiKy)?.GiaTri,
                DiemTrungBinh = QuyCheDanhGia.TinhDiemTrungBinhMonHocKy(ToThanhPhan(x.Diems))
            })
            .ToList();
    }

    internal static List<DiemThanhPhan> ToThanhPhan(IEnumerable<Diem> diems) =>
        diems.Select(d => new DiemThanhPhan(d.LoaiDiem, d.GiaTri)).ToList();
}
