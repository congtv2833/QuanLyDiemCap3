using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLDiem.Data;
using QLDiem.Models;

namespace QLDiem.Controllers;

[Authorize(Policy = "QuanTri")]
public class GiaoVienController : Controller
{
    private readonly QLDiemDbContext _db;

    public GiaoVienController(QLDiemDbContext db) => _db = db;

    public IActionResult Index(string? tuKhoa)
    {
        var truyVan = _db.GiaoViens.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(tuKhoa))
        {
            var tk = tuKhoa.Trim();
            truyVan = truyVan.Where(g => g.HoTen.Contains(tk)
                                         || g.MaGiaoVien.Contains(tk)
                                         || (g.ChuyenMon != null && g.ChuyenMon.Contains(tk)));
        }

        ViewBag.TuKhoa = tuKhoa;
        return View(truyVan.OrderBy(g => g.MaGiaoVien).ToList());
    }

    public IActionResult Create() => View("Form", new GiaoVien());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(
        [Bind("MaGiaoVien,HoTen,GioiTinh,NgaySinh,ChuyenMon,Email,DienThoai,DangCongTac")] GiaoVien model)
    {
        if (_db.GiaoViens.Any(g => g.MaGiaoVien == model.MaGiaoVien))
            ModelState.AddModelError(nameof(GiaoVien.MaGiaoVien), "Mã giáo viên đã tồn tại.");

        if (!ModelState.IsValid) return View("Form", model);

        _db.GiaoViens.Add(model);
        _db.SaveChanges();
        TempData["ThanhCong"] = $"Đã thêm giáo viên {model.HoTen}.";
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Edit(int id)
    {
        var gv = _db.GiaoViens.Find(id);
        return gv == null ? NotFound() : View("Form", gv);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id,
        [Bind("Id,MaGiaoVien,HoTen,GioiTinh,NgaySinh,ChuyenMon,Email,DienThoai,DangCongTac")] GiaoVien model)
    {
        if (id != model.Id) return BadRequest();

        if (_db.GiaoViens.Any(g => g.MaGiaoVien == model.MaGiaoVien && g.Id != id))
            ModelState.AddModelError(nameof(GiaoVien.MaGiaoVien), "Mã giáo viên đã tồn tại.");

        if (!ModelState.IsValid) return View("Form", model);

        _db.Entry(model).State = EntityState.Modified;
        _db.SaveChanges();
        TempData["ThanhCong"] = "Đã cập nhật hồ sơ giáo viên.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Delete(int id)
    {
        var gv = _db.GiaoViens.Find(id);
        if (gv == null) return NotFound();

        if (_db.PhanCongGiangDays.Any(p => p.GiaoVienId == id))
        {
            TempData["Loi"] = "Không xóa được: giáo viên đang có phân công giảng dạy.";
            return RedirectToAction(nameof(Index));
        }

        _db.GiaoViens.Remove(gv);
        _db.SaveChanges();
        TempData["ThanhCong"] = "Đã xóa giáo viên.";
        return RedirectToAction(nameof(Index));
    }
}
