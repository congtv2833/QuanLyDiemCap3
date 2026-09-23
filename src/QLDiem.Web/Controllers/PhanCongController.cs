using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QLDiem.Domain.Entities;
using QLDiem.Infrastructure.Data;

namespace QLDiem.Web.Controllers;

[Authorize(Policy = "QuanTri")]
public class PhanCongController : Controller
{
    private readonly QLDiemDbContext _db;

    public PhanCongController(QLDiemDbContext db) => _db = db;

    public async Task<IActionResult> Index(int? lopId)
    {
        ViewBag.DanhSachLop = await TaoSelectLopAsync(lopId);
        ViewBag.LopId = lopId;

        if (lopId is null) return View(new List<PhanCongGiangDay>());

        var danhSach = await _db.PhanCongGiangDays.AsNoTracking()
            .Include(p => p.MonHoc)
            .Include(p => p.GiaoVien)
            .Include(p => p.Lop)
            .Where(p => p.LopId == lopId)
            .OrderBy(p => p.MonHoc.ThuTuHienThi)
            .ToListAsync();

        return View(danhSach);
    }

    public async Task<IActionResult> Create(int? lopId)
    {
        await NapDanhMucAsync(lopId, null, null);
        return View("Form", new PhanCongGiangDay { LopId = lopId ?? 0 });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("LopId,MonHocId,GiaoVienId")] PhanCongGiangDay model)
    {
        var lop = await _db.Lops.AsNoTracking().FirstOrDefaultAsync(l => l.Id == model.LopId);
        if (lop is null)
        {
            ModelState.AddModelError(nameof(PhanCongGiangDay.LopId), "Vui lòng chọn lớp.");
        }
        else
        {
            model.NamHocId = lop.NamHocId;

            if (await _db.PhanCongGiangDays.AnyAsync(p => p.LopId == model.LopId
                                                          && p.MonHocId == model.MonHocId))
            {
                ModelState.AddModelError(nameof(PhanCongGiangDay.MonHocId),
                    "Môn này đã được phân công cho lớp.");
            }
        }

        if (!ModelState.IsValid)
        {
            await NapDanhMucAsync(model.LopId, model.MonHocId, model.GiaoVienId);
            return View("Form", model);
        }

        _db.PhanCongGiangDays.Add(model);
        await _db.SaveChangesAsync();
        TempData["ThanhCong"] = "Đã thêm phân công giảng dạy.";
        return RedirectToAction(nameof(Index), new { lopId = model.LopId });
    }

    public async Task<IActionResult> Edit(int id)
    {
        var pc = await _db.PhanCongGiangDays.FindAsync(id);
        if (pc is null) return NotFound();

        await NapDanhMucAsync(pc.LopId, pc.MonHocId, pc.GiaoVienId);
        return View("Form", pc);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,LopId,MonHocId,GiaoVienId,NamHocId")] PhanCongGiangDay model)
    {
        if (id != model.Id) return BadRequest();

        if (await _db.PhanCongGiangDays.AnyAsync(p => p.LopId == model.LopId
                                                      && p.MonHocId == model.MonHocId
                                                      && p.Id != id))
        {
            ModelState.AddModelError(nameof(PhanCongGiangDay.MonHocId), "Môn này đã được phân công cho lớp.");
        }

        if (!ModelState.IsValid)
        {
            await NapDanhMucAsync(model.LopId, model.MonHocId, model.GiaoVienId);
            return View("Form", model);
        }

        _db.Entry(model).State = EntityState.Modified;
        await _db.SaveChangesAsync();
        TempData["ThanhCong"] = "Đã cập nhật phân công.";
        return RedirectToAction(nameof(Index), new { lopId = model.LopId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var pc = await _db.PhanCongGiangDays.FindAsync(id);
        if (pc is null) return NotFound();

        bool daCoDiem = await _db.Diems.AnyAsync(d => d.MonHocId == pc.MonHocId
                                                      && d.HocSinh.LopId == pc.LopId);
        if (daCoDiem)
        {
            TempData["Loi"] = "Không xóa được: lớp đã có điểm của môn này.";
            return RedirectToAction(nameof(Index), new { lopId = pc.LopId });
        }

        _db.PhanCongGiangDays.Remove(pc);
        await _db.SaveChangesAsync();
        TempData["ThanhCong"] = "Đã xóa phân công.";
        return RedirectToAction(nameof(Index), new { lopId = pc.LopId });
    }

    /// <summary>Gán toàn bộ môn học đang có cho một lớp, bỏ qua những môn đã phân công.</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PhanCongNhanh(int lopId, int giaoVienId)
    {
        var lop = await _db.Lops.AsNoTracking().FirstOrDefaultAsync(l => l.Id == lopId);
        if (lop is null) return NotFound();

        var daCo = await _db.PhanCongGiangDays
            .Where(p => p.LopId == lopId)
            .Select(p => p.MonHocId)
            .ToListAsync();

        var monConLai = await _db.MonHocs
            .Where(m => !daCo.Contains(m.Id))
            .Select(m => m.Id)
            .ToListAsync();

        foreach (var monId in monConLai)
        {
            _db.PhanCongGiangDays.Add(new PhanCongGiangDay
            {
                LopId = lopId,
                MonHocId = monId,
                GiaoVienId = giaoVienId,
                NamHocId = lop.NamHocId
            });
        }

        await _db.SaveChangesAsync();
        TempData["ThanhCong"] = $"Đã phân công thêm {monConLai.Count} môn cho lớp {lop.Ten}.";
        return RedirectToAction(nameof(Index), new { lopId });
    }

    private async Task NapDanhMucAsync(int? lopId, int? monHocId, int? giaoVienId)
    {
        ViewBag.DanhSachLop = await TaoSelectLopAsync(lopId);

        ViewBag.DanhSachMon = new SelectList(
            await _db.MonHocs.AsNoTracking().OrderBy(m => m.ThuTuHienThi)
                .Select(m => new { m.Id, m.TenMon }).ToListAsync(),
            "Id", "TenMon", monHocId);

        ViewBag.DanhSachGiaoVien = new SelectList(
            await _db.GiaoViens.AsNoTracking().Where(g => g.DangCongTac).OrderBy(g => g.MaGiaoVien)
                .Select(g => new { g.Id, MoTa = g.MaGiaoVien + " - " + g.HoTen }).ToListAsync(),
            "Id", "MoTa", giaoVienId);
    }

    private async Task<SelectList> TaoSelectLopAsync(int? chon)
    {
        var lops = await _db.Lops.AsNoTracking()
            .Include(l => l.NamHoc)
            .OrderByDescending(l => l.NamHoc.Ten).ThenBy(l => l.Ten)
            .Select(l => new { l.Id, MoTa = l.Ten + " (" + l.NamHoc.Ten + ")" })
            .ToListAsync();

        return new SelectList(lops, "Id", "MoTa", chon);
    }
}
