using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QLDiem.Domain.Entities;
using QLDiem.Infrastructure.Data;

namespace QLDiem.Web.Controllers;

[Authorize(Policy = "QuanTri")]
public class LopController : Controller
{
    private readonly QLDiemDbContext _db;

    public LopController(QLDiemDbContext db) => _db = db;

    public async Task<IActionResult> Index(int? namHocId)
    {
        var nam = namHocId ?? await LayNamHocMacDinhAsync();
        ViewBag.NamHocId = nam;
        ViewBag.DanhSachNamHoc = await TaoSelectNamHocAsync(nam);

        var lops = await _db.Lops.AsNoTracking()
            .Include(l => l.GiaoVienChuNhiem)
            .Where(l => l.NamHocId == nam)
            .OrderBy(l => l.Khoi).ThenBy(l => l.Ten)
            .ToListAsync();

        ViewBag.SiSo = await _db.HocSinhs.AsNoTracking()
            .Where(h => h.DangHoc && h.Lop.NamHocId == nam)
            .GroupBy(h => h.LopId)
            .Select(g => new { LopId = g.Key, SiSo = g.Count() })
            .ToDictionaryAsync(x => x.LopId, x => x.SiSo);

        return View(lops);
    }

    public async Task<IActionResult> Create()
    {
        await NapDanhMucAsync();
        return View("Form", new Lop { Khoi = 10, NamHocId = await LayNamHocMacDinhAsync() });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Ten,Khoi,NamHocId,GiaoVienChuNhiemId")] Lop model)
    {
        await KiemTraAsync(model, 0);
        if (!ModelState.IsValid)
        {
            await NapDanhMucAsync();
            return View("Form", model);
        }

        _db.Lops.Add(model);
        await _db.SaveChangesAsync();
        TempData["ThanhCong"] = $"Đã thêm lớp {model.Ten}.";
        return RedirectToAction(nameof(Index), new { namHocId = model.NamHocId });
    }

    public async Task<IActionResult> Edit(int id)
    {
        var lop = await _db.Lops.FindAsync(id);
        if (lop is null) return NotFound();

        await NapDanhMucAsync();
        return View("Form", lop);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Ten,Khoi,NamHocId,GiaoVienChuNhiemId")] Lop model)
    {
        if (id != model.Id) return BadRequest();

        await KiemTraAsync(model, id);
        if (!ModelState.IsValid)
        {
            await NapDanhMucAsync();
            return View("Form", model);
        }

        _db.Entry(model).State = EntityState.Modified;
        await _db.SaveChangesAsync();
        TempData["ThanhCong"] = "Đã cập nhật lớp.";
        return RedirectToAction(nameof(Index), new { namHocId = model.NamHocId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var lop = await _db.Lops.FindAsync(id);
        if (lop is null) return NotFound();

        if (await _db.HocSinhs.AnyAsync(h => h.LopId == id))
        {
            TempData["Loi"] = "Không xóa được: lớp đang có học sinh.";
            return RedirectToAction(nameof(Index), new { namHocId = lop.NamHocId });
        }

        var phanCongs = await _db.PhanCongGiangDays.Where(p => p.LopId == id).ToListAsync();
        _db.PhanCongGiangDays.RemoveRange(phanCongs);
        _db.Lops.Remove(lop);
        await _db.SaveChangesAsync();

        TempData["ThanhCong"] = "Đã xóa lớp.";
        return RedirectToAction(nameof(Index), new { namHocId = lop.NamHocId });
    }

    private async Task KiemTraAsync(Lop model, int idHienTai)
    {
        if (await _db.Lops.AnyAsync(l => l.Ten == model.Ten
                                         && l.NamHocId == model.NamHocId
                                         && l.Id != idHienTai))
        {
            ModelState.AddModelError(nameof(Lop.Ten), "Lớp này đã tồn tại trong năm học đã chọn.");
        }

        if (model.Khoi is < 10 or > 12)
            ModelState.AddModelError(nameof(Lop.Khoi), "Khối phải là 10, 11 hoặc 12.");
    }

    private async Task NapDanhMucAsync()
    {
        ViewBag.DanhSachNamHoc = await TaoSelectNamHocAsync(null);
        ViewBag.DanhSachGiaoVien = new SelectList(
            await _db.GiaoViens.AsNoTracking()
                .Where(g => g.DangCongTac)
                .OrderBy(g => g.MaGiaoVien)
                .Select(g => new { g.Id, MoTa = g.MaGiaoVien + " - " + g.HoTen })
                .ToListAsync(),
            "Id", "MoTa");
    }

    private async Task<SelectList> TaoSelectNamHocAsync(int? chon) =>
        new(await _db.NamHocs.AsNoTracking().OrderByDescending(n => n.Ten).ToListAsync(),
            "Id", "Ten", chon);

    private async Task<int> LayNamHocMacDinhAsync() =>
        await _db.NamHocs.Where(n => n.DangHoatDong).Select(n => n.Id).FirstOrDefaultAsync();
}
