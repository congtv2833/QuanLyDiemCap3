using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLDiem.Domain.Entities;
using QLDiem.Infrastructure.Common;
using QLDiem.Infrastructure.Data;

namespace QLDiem.Web.Controllers;

[Authorize(Policy = "QuanTri")]
public class GiaoVienController : Controller
{
    private readonly QLDiemDbContext _db;

    public GiaoVienController(QLDiemDbContext db) => _db = db;

    public async Task<IActionResult> Index(string? tuKhoa)
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
        var danhSach = await truyVan.ToListAsync();
        return View(danhSach.SapTheoTen(g => g.HoTen).ToList());
    }

    public IActionResult Create() => View("Form", new GiaoVien());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [Bind("MaGiaoVien,HoTen,GioiTinh,NgaySinh,ChuyenMon,Email,DienThoai,DangCongTac")] GiaoVien model)
    {
        if (await _db.GiaoViens.AnyAsync(g => g.MaGiaoVien == model.MaGiaoVien))
            ModelState.AddModelError(nameof(GiaoVien.MaGiaoVien), "Mã giáo viên đã tồn tại.");

        if (!ModelState.IsValid) return View("Form", model);

        _db.GiaoViens.Add(model);
        await _db.SaveChangesAsync();
        TempData["ThanhCong"] = $"Đã thêm giáo viên {model.HoTen}.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var gv = await _db.GiaoViens.FindAsync(id);
        return gv is null ? NotFound() : View("Form", gv);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id,
        [Bind("Id,MaGiaoVien,HoTen,GioiTinh,NgaySinh,ChuyenMon,Email,DienThoai,DangCongTac,UserId")] GiaoVien model)
    {
        if (id != model.Id) return BadRequest();

        if (await _db.GiaoViens.AnyAsync(g => g.MaGiaoVien == model.MaGiaoVien && g.Id != id))
            ModelState.AddModelError(nameof(GiaoVien.MaGiaoVien), "Mã giáo viên đã tồn tại.");

        if (!ModelState.IsValid) return View("Form", model);

        _db.Entry(model).State = EntityState.Modified;
        await _db.SaveChangesAsync();
        TempData["ThanhCong"] = "Đã cập nhật hồ sơ giáo viên.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var gv = await _db.GiaoViens.FindAsync(id);
        if (gv is null) return NotFound();

        if (await _db.PhanCongGiangDays.AnyAsync(p => p.GiaoVienId == id))
        {
            TempData["Loi"] = "Không xóa được: giáo viên đang có phân công giảng dạy.";
            return RedirectToAction(nameof(Index));
        }

        _db.GiaoViens.Remove(gv);
        await _db.SaveChangesAsync();
        TempData["ThanhCong"] = "Đã xóa giáo viên.";
        return RedirectToAction(nameof(Index));
    }
}
