using Microsoft.EntityFrameworkCore;
using QLDiem.Data;
using QLDiem.Models;
using QLDiem.Models.ViewModels;

namespace QLDiem.Services;

/// <summary>Nhập, sửa và tra cứu các đầu điểm.</summary>
public class DiemService
{
    private readonly QLDiemDbContext _db;

    public DiemService(QLDiemDbContext db) => _db = db;

    /// <summary>Lấy bảng điểm của một lớp theo môn và học kỳ, kèm ĐTBmhk đã tính sẵn.</summary>
    public BangDiemLopVM? LayBangDiemLop(int lopId, int monHocId, HocKy hocKy)
    {
        var lop = _db.Lops.Include(l => l.NamHoc).AsNoTracking()
            .FirstOrDefault(l => l.Id == lopId);
        var mon = _db.MonHocs.AsNoTracking().FirstOrDefault(m => m.Id == monHocId);
        if (lop == null || mon == null) return null;

        var hocSinhs = _db.HocSinhs.AsNoTracking()
            .Where(h => h.LopId == lopId && h.DangHoc)
            .OrderBy(h => h.MaHocSinh)
            .ToList();

        var diems = _db.Diems.AsNoTracking()
            .Where(d => d.MonHocId == monHocId
                        && d.NamHocId == lop.NamHocId
                        && d.HocKy == hocKy
                        && d.HocSinh.LopId == lopId)
            .ToList();

        var nhanXets = _db.DanhGiaNhanXets.AsNoTracking()
            .Where(n => n.MonHocId == monHocId
                        && n.NamHocId == lop.NamHocId
                        && n.HocKy == hocKy
                        && n.HocSinh.LopId == lopId)
            .ToList();

        int soTx = mon.SoDauDiemThuongXuyen;

        var bang = new BangDiemLopVM
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

        foreach (var hs in hocSinhs)
        {
            var cuaHs = diems.Where(d => d.HocSinhId == hs.Id).ToList();

            var tx = new decimal?[soTx];
            foreach (var d in cuaHs.Where(d => d.LoaiDiem == LoaiDiem.ThuongXuyen))
            {
                if (d.ThuTu >= 1 && d.ThuTu <= soTx) tx[d.ThuTu - 1] = d.GiaTri;
            }

            var thanhPhan = ToThanhPhan(cuaHs);
            var nx = nhanXets.FirstOrDefault(n => n.HocSinhId == hs.Id);

            bang.DanhSach.Add(new DongBangDiemVM
            {
                HocSinhId = hs.Id,
                MaHocSinh = hs.MaHocSinh,
                HoTen = hs.HoTen,
                DiemThuongXuyen = tx,
                DiemGiuaKy = cuaHs.FirstOrDefault(d => d.LoaiDiem == LoaiDiem.GiuaKy)?.GiaTri,
                DiemCuoiKy = cuaHs.FirstOrDefault(d => d.LoaiDiem == LoaiDiem.CuoiKy)?.GiaTri,
                DiemTrungBinh = QuyCheDanhGia.TinhDiemTrungBinhMonHocKy(thanhPhan),
                DaDuDiem = QuyCheDanhGia.DaDuDauDiem(thanhPhan, soTx),
                KetQuaNhanXet = nx?.KetQua,
                NoiDungNhanXet = nx?.NoiDungNhanXet
            });
        }

        return bang;
    }

    /// <summary>Lưu toàn bộ bảng điểm vừa nhập. Ô để trống nghĩa là xóa đầu điểm đó.</summary>
    public KetQuaThaoTac LuuBangDiem(LuuBangDiemVM duLieu)
    {
        var mon = _db.MonHocs.FirstOrDefault(m => m.Id == duLieu.MonHocId);
        if (mon == null) return KetQuaThaoTac.ThatBai("Không tìm thấy môn học.");

        if (duLieu.HocKy == HocKy.CaNam)
            return KetQuaThaoTac.ThatBai("Chỉ nhập điểm cho Học kỳ I hoặc Học kỳ II.");

        var idHocSinh = duLieu.DanhSach.Select(x => x.HocSinhId).ToList();
        var tenHocSinh = _db.HocSinhs
            .Where(h => idHocSinh.Contains(h.Id) && h.LopId == duLieu.LopId)
            .ToDictionary(h => h.Id, h => h.HoTen);

        // Kiểm tra toàn bộ bảng trước khi ghi: chỉ lưu khi mọi ô đều hợp lệ.
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

            for (int i = 0; i < dong.DiemThuongXuyen.Length; i++)
            {
                var giaTri = dong.DiemThuongXuyen[i];
                if (giaTri == null) continue;

                if (i >= soTx)
                    loi.Add($"Môn {mon.TenMon} chỉ có {soTx} đầu điểm thường xuyên.");
                else if (!QuyCheDanhGia.LaDiemHopLe(giaTri.Value))
                    loi.Add($"ĐĐGtx{i + 1} của {hoTen} phải nằm trong khoảng 0 - 10.");
            }

            if (dong.DiemGiuaKy != null && !QuyCheDanhGia.LaDiemHopLe(dong.DiemGiuaKy.Value))
                loi.Add($"ĐĐGgk của {hoTen} phải nằm trong khoảng 0 - 10.");

            if (dong.DiemCuoiKy != null && !QuyCheDanhGia.LaDiemHopLe(dong.DiemCuoiKy.Value))
                loi.Add($"ĐĐGck của {hoTen} phải nằm trong khoảng 0 - 10.");
        }

        if (loi.Count > 0) return KetQuaThaoTac.ThatBai(loi.Distinct().ToArray());

        if (mon.LoaiDanhGia == LoaiDanhGia.NhanXet)
            return LuuNhanXet(duLieu, tenHocSinh.Keys.ToList());

        var hienCo = _db.Diems
            .Where(d => d.MonHocId == duLieu.MonHocId
                        && d.NamHocId == duLieu.NamHocId
                        && d.HocKy == duLieu.HocKy
                        && idHocSinh.Contains(d.HocSinhId))
            .ToList();

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

        _db.SaveChanges();
        return KetQuaThaoTac.Ok($"Đã lưu bảng điểm môn {mon.TenMon}.");
    }

    /// <summary>Ghi một đầu điểm: thêm mới, cập nhật, hoặc xóa khi người dùng để trống ô.</summary>
    private void GhiDauDiem(
        List<Diem> hienCo, LuuBangDiemVM duLieu, int hocSinhId,
        LoaiDiem loaiDiem, int thuTu, decimal? giaTri)
    {
        var cu = hienCo.FirstOrDefault(d => d.HocSinhId == hocSinhId
                                            && d.LoaiDiem == loaiDiem
                                            && d.ThuTu == thuTu);

        if (giaTri == null)
        {
            if (cu != null) _db.Diems.Remove(cu);
            return;
        }

        var giaTriLamTron = QuyCheDanhGia.LamTron(giaTri.Value);

        if (cu == null)
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

    private KetQuaThaoTac LuuNhanXet(LuuBangDiemVM duLieu, List<int> hocSinhHopLe)
    {
        var idHocSinh = duLieu.DanhSach.Select(x => x.HocSinhId).ToList();

        var hienCo = _db.DanhGiaNhanXets
            .Where(n => n.MonHocId == duLieu.MonHocId
                        && n.NamHocId == duLieu.NamHocId
                        && n.HocKy == duLieu.HocKy
                        && idHocSinh.Contains(n.HocSinhId))
            .ToList();

        foreach (var dong in duLieu.DanhSach.Where(d => hocSinhHopLe.Contains(d.HocSinhId)))
        {
            var cu = hienCo.FirstOrDefault(n => n.HocSinhId == dong.HocSinhId);

            if (dong.KetQuaNhanXet == null)
            {
                if (cu != null) _db.DanhGiaNhanXets.Remove(cu);
                continue;
            }

            if (cu == null)
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

        _db.SaveChanges();
        return KetQuaThaoTac.Ok("Đã lưu kết quả đánh giá.");
    }

    /// <summary>Chi tiết các đầu điểm của một học sinh trong một học kỳ.</summary>
    public List<ChiTietDiemMonVM> LayChiTietDiem(int hocSinhId, HocKy hocKy)
    {
        var hs = _db.HocSinhs.Include(h => h.Lop).AsNoTracking()
            .FirstOrDefault(h => h.Id == hocSinhId);
        if (hs == null) return new List<ChiTietDiemMonVM>();

        var diems = _db.Diems.Include(d => d.MonHoc).AsNoTracking()
            .Where(d => d.HocSinhId == hocSinhId
                        && d.NamHocId == hs.Lop.NamHocId
                        && d.HocKy == hocKy)
            .ToList();

        // Gom theo MonHocId, không gom theo đối tượng MonHoc: truy vấn AsNoTracking
        // tạo một instance MonHoc riêng cho mỗi dòng điểm nên so sánh tham chiếu sẽ sai.
        return diems
            .GroupBy(d => d.MonHocId)
            .Select(g => new { Mon = g.First().MonHoc, Diems = g.ToList() })
            .OrderBy(x => x.Mon.ThuTuHienThi)
            .Select(x => new ChiTietDiemMonVM
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
