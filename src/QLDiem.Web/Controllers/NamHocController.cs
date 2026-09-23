using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLDiem.Domain.Entities;
using QLDiem.Infrastructure.Data;

namespace QLDiem.Web.Controllers;

[Authorize(Policy = "QuanTri")]
public class NamHocController : Controller
{
    private readonly QLDiemDbContext _db;

    public NamHocController(QLDiemDbContext db) => _db = db;

    public async Task<IActionResult> Index() =>
        View(await _db.NamHocs.AsNoTracking()
            .OrderByDescending(n => n.Ten)
            .ToListAsync());

    public IActionResult Create() => View("Form", new NamHoc
    {
        NgayBatDau = new DateOnly(DateTime.Today.Year, 9, 5),
        NgayKetThuc = new DateOnly(DateTime.Today.Year + 1, 5, 31)
    });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Ten,NgayBatDau,NgayKetThuc,DangHoatDong")] NamHoc model)
    {
        if (await _db.NamHocs.AnyAsync(n => n.Ten == model.Ten))
            ModelState.AddModelError(nameof(NamHoc.Ten), "Năm học này đã tồn tại.");

        if (!ModelState.IsValid) return View("Form", model);

        if (model.DangHoatDong) await TatCacNamHocKhacAsync(0);

        _db.NamHocs.Add(model);
        await _db.SaveChangesAsync();
        TempData["ThanhCong"] = $"Đã thêm năm học {model.Ten}.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var nam = await _db.NamHocs.FindAsync(id);
        return nam is null ? NotFound() : View("Form", nam);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Ten,NgayBatDau,NgayKetThuc,DangHoatDong")] NamHoc model)
    {
        if (id != model.Id) return BadRequest();

        if (await _db.NamHocs.AnyAsync(n => n.Ten == model.Ten && n.Id != id))
            ModelState.AddModelError(nameof(NamHoc.Ten), "Năm học này đã tồn tại.");

        if (!ModelState.IsValid) return View("Form", model);

        if (model.DangHoatDong) await TatCacNamHocKhacAsync(id);

        _db.Entry(model).State = EntityState.Modified;
        await _db.SaveChangesAsync();
        TempData["ThanhCong"] = "Đã cập nhật năm học.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var nam = await _db.NamHocs.FindAsync(id);
        if (nam is null) return NotFound();

        if (await _db.Lops.AnyAsync(l => l.NamHocId == id))
        {
            TempData["Loi"] = "Không xóa được: năm học đã có lớp.";
            return RedirectToAction(nameof(Index));
        }

        _db.NamHocs.Remove(nam);
        await _db.SaveChangesAsync();
        TempData["ThanhCong"] = "Đã xóa năm học.";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>Chỉ một năm học được đánh dấu đang hoạt động tại một thời điểm.</summary>
    private async Task TatCacNamHocKhacAsync(int giuLaiId)
    {
        var khac = await _db.NamHocs.Where(n => n.DangHoatDong && n.Id != giuLaiId).ToListAsync();
        foreach (var n in khac) n.DangHoatDong = false;
    }
}
