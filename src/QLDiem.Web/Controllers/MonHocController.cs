using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLDiem.Domain.Entities;
using QLDiem.Infrastructure.Data;

namespace QLDiem.Web.Controllers;

[Authorize(Policy = "QuanTri")]
public class MonHocController : Controller
{
    private readonly QLDiemDbContext _db;

    public MonHocController(QLDiemDbContext db) => _db = db;

    public async Task<IActionResult> Index() =>
        View(await _db.MonHocs.AsNoTracking()
            .OrderBy(m => m.ThuTuHienThi)
            .ToListAsync());

    public IActionResult Create() => View("Form", new MonHoc { SoTietNam = 70, ThuTuHienThi = 99 });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [Bind("MaMon,TenMon,SoTietNam,LoaiDanhGia,BatBuoc,ThuTuHienThi")] MonHoc model)
    {
        await KiemTraAsync(model, 0);
        if (!ModelState.IsValid) return View("Form", model);

        _db.MonHocs.Add(model);
        await _db.SaveChangesAsync();
        TempData["ThanhCong"] = $"Đã thêm môn {model.TenMon}.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var mon = await _db.MonHocs.FindAsync(id);
        return mon is null ? NotFound() : View("Form", mon);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id,
        [Bind("Id,MaMon,TenMon,SoTietNam,LoaiDanhGia,BatBuoc,ThuTuHienThi")] MonHoc model)
    {
        if (id != model.Id) return BadRequest();

        await KiemTraAsync(model, id);
        if (!ModelState.IsValid) return View("Form", model);

        _db.Entry(model).State = EntityState.Modified;
        await _db.SaveChangesAsync();
        TempData["ThanhCong"] = "Đã cập nhật môn học.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var mon = await _db.MonHocs.FindAsync(id);
        if (mon is null) return NotFound();

        if (await _db.Diems.AnyAsync(d => d.MonHocId == id)
            || await _db.PhanCongGiangDays.AnyAsync(p => p.MonHocId == id))
        {
            TempData["Loi"] = "Không xóa được: môn học đã được phân công hoặc đã có điểm.";
            return RedirectToAction(nameof(Index));
        }

        _db.MonHocs.Remove(mon);
        await _db.SaveChangesAsync();
        TempData["ThanhCong"] = "Đã xóa môn học.";
        return RedirectToAction(nameof(Index));
    }

    private async Task KiemTraAsync(MonHoc model, int idHienTai)
    {
        if (await _db.MonHocs.AnyAsync(m => m.MaMon == model.MaMon && m.Id != idHienTai))
            ModelState.AddModelError(nameof(MonHoc.MaMon), "Mã môn đã tồn tại.");

        if (model.SoTietNam <= 0)
            ModelState.AddModelError(nameof(MonHoc.SoTietNam), "Số tiết trong năm phải lớn hơn 0.");
    }
}
