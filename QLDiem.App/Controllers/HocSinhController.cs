using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QLDiem.Data;
using QLDiem.Models;

namespace QLDiem.Controllers;

[Authorize(Roles = VaiTro.Admin + "," + VaiTro.GiaoVien)]
public class HocSinhController : Controller
{
    private const int SoDongMoiTrang = 20;

    private readonly QLDiemDbContext _db;

    public HocSinhController(QLDiemDbContext db) => _db = db;

    public IActionResult Index(string? tuKhoa, int? lopId, int trang = 1)
    {
        var truyVan = _db.HocSinhs.AsNoTracking().Include(h => h.Lop).AsQueryable();

        if (!string.IsNullOrWhiteSpace(tuKhoa))
        {
            var tk = tuKhoa.Trim();
            truyVan = truyVan.Where(h => h.HoTen.Contains(tk) || h.MaHocSinh.Contains(tk));
        }

        if (lopId != null) truyVan = truyVan.Where(h => h.LopId == lopId);

        var tongSo = truyVan.Count();
        var danhSach = truyVan
            .OrderBy(h => h.MaHocSinh)
            .Skip((trang - 1) * SoDongMoiTrang)
            .Take(SoDongMoiTrang)
            .ToList();

        ViewBag.TuKhoa = tuKhoa;
        ViewBag.LopId = lopId;
        ViewBag.Trang = trang;
        ViewBag.TongSoTrang = (int)Math.Ceiling(tongSo / (double)SoDongMoiTrang);
        ViewBag.TongSo = tongSo;
        ViewBag.DanhSachLop = TaoSelectLop(lopId);

        return View(danhSach);
    }

    public IActionResult Create()
    {
        ViewBag.DanhSachLop = TaoSelectLop(null);
        return View("Form", new HocSinh
        {
            MaHocSinh = SinhMaMoi(),
            NgaySinh = new DateOnly(DateTime.Today.Year - 16, 1, 1)
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create([Bind("MaHocSinh,HoTen,NgaySinh,GioiTinh,NoiSinh,DiaChi," +
        "DienThoai,Email,HoTenPhuHuynh,DienThoaiPhuHuynh,LopId,DangHoc")] HocSinh model)
    {
        if (_db.HocSinhs.Any(h => h.MaHocSinh == model.MaHocSinh))
            ModelState.AddModelError(nameof(HocSinh.MaHocSinh), "Mã học sinh đã tồn tại.");

        if (!ModelState.IsValid)
        {
            ViewBag.DanhSachLop = TaoSelectLop(model.LopId);
            return View("Form", model);
        }

        _db.HocSinhs.Add(model);
        _db.SaveChanges();
        TempData["ThanhCong"] = $"Đã thêm học sinh {model.HoTen}.";
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Edit(int id)
    {
        var hs = _db.HocSinhs.Find(id);
        if (hs == null) return NotFound();

        ViewBag.DanhSachLop = TaoSelectLop(hs.LopId);
        return View("Form", hs);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id, [Bind("Id,MaHocSinh,HoTen,NgaySinh,GioiTinh,NoiSinh," +
        "DiaChi,DienThoai,Email,HoTenPhuHuynh,DienThoaiPhuHuynh,LopId,DangHoc")] HocSinh model)
    {
        if (id != model.Id) return BadRequest();

        if (_db.HocSinhs.Any(h => h.MaHocSinh == model.MaHocSinh && h.Id != id))
            ModelState.AddModelError(nameof(HocSinh.MaHocSinh), "Mã học sinh đã tồn tại.");

        if (!ModelState.IsValid)
        {
            ViewBag.DanhSachLop = TaoSelectLop(model.LopId);
            return View("Form", model);
        }

        _db.Entry(model).State = EntityState.Modified;
        _db.SaveChanges();
        TempData["ThanhCong"] = "Đã cập nhật hồ sơ học sinh.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Authorize(Policy = "QuanTri")]
    [ValidateAntiForgeryToken]
    public IActionResult Delete(int id)
    {
        var hs = _db.HocSinhs.Find(id);
        if (hs == null) return NotFound();

        if (_db.Diems.Any(d => d.HocSinhId == id))
        {
            // Không xóa cứng học sinh đã có điểm; chuyển sang trạng thái thôi học.
            hs.DangHoc = false;
            _db.SaveChanges();
            TempData["ThanhCong"] = $"Học sinh {hs.HoTen} đã có điểm nên được chuyển sang trạng thái thôi học.";
            return RedirectToAction(nameof(Index));
        }

        _db.HocSinhs.Remove(hs);
        _db.SaveChanges();
        TempData["ThanhCong"] = "Đã xóa học sinh.";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>Hồ sơ chi tiết của một học sinh.</summary>
    public IActionResult ChiTiet(int id)
    {
        var hs = _db.HocSinhs.AsNoTracking()
            .Include(h => h.Lop).ThenInclude(l => l.NamHoc)
            .Include(h => h.Lop).ThenInclude(l => l.GiaoVienChuNhiem)
            .FirstOrDefault(h => h.Id == id);

        return hs == null ? NotFound() : View(hs);
    }

    /// <summary>Danh sách lớp cho ô chọn trên form, kèm năm học để phân biệt.</summary>
    private SelectList TaoSelectLop(int? chon)
    {
        var lops = _db.Lops.AsNoTracking()
            .Include(l => l.NamHoc)
            .OrderByDescending(l => l.NamHoc.Ten).ThenBy(l => l.Ten)
            .Select(l => new { l.Id, MoTa = l.Ten + " (" + l.NamHoc.Ten + ")" })
            .ToList();

        return new SelectList(lops, "Id", "MoTa", chon);
    }

    /// <summary>Sinh mã học sinh kế tiếp, ví dụ HS031 sau HS030.</summary>
    private string SinhMaMoi()
    {
        var maCuoi = _db.HocSinhs.OrderByDescending(h => h.MaHocSinh)
            .Select(h => h.MaHocSinh)
            .FirstOrDefault();

        if (maCuoi == null || !int.TryParse(maCuoi.Substring(2), out var so)) return "HS001";
        return $"HS{so + 1:D3}";
    }
}
