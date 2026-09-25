using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QLDiem.Data;
using QLDiem.Models;

namespace QLDiem.Controllers;

[Authorize(Policy = "QuanTri")]
public class LopController : Controller
{
    private readonly QLDiemDbContext _db;

    public LopController(QLDiemDbContext db) => _db = db;

    public IActionResult Index(int? namHocId)
    {
        var nam = namHocId ?? LayNamHocMacDinh();
        ViewBag.NamHocId = nam;
        ViewBag.DanhSachNamHoc = TaoSelectNamHoc(nam);

        var lops = _db.Lops.AsNoTracking()
            .Include(l => l.GiaoVienChuNhiem)
            .Where(l => l.NamHocId == nam)
            .OrderBy(l => l.Khoi).ThenBy(l => l.Ten)
            .ToList();

        ViewBag.SiSo = _db.HocSinhs.AsNoTracking()
            .Where(h => h.DangHoc && h.Lop.NamHocId == nam)
            .GroupBy(h => h.LopId)
            .Select(g => new { LopId = g.Key, SiSo = g.Count() })
            .ToDictionary(x => x.LopId, x => x.SiSo);

        return View(lops);
    }

    public IActionResult Create()
    {
        NapDanhMuc(null);
        return View("Form", new Lop { Khoi = 10, NamHocId = LayNamHocMacDinh() });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create([Bind("Ten,Khoi,NamHocId,GiaoVienChuNhiemId")] Lop model)
    {
        KiemTra(model, 0);
        if (!ModelState.IsValid)
        {
            NapDanhMuc(model.GiaoVienChuNhiemId);
            return View("Form", model);
        }

        _db.Lops.Add(model);
        _db.SaveChanges();
        TempData["ThanhCong"] = $"Đã thêm lớp {model.Ten}.";
        return RedirectToAction(nameof(Index), new { namHocId = model.NamHocId });
    }

    public IActionResult Edit(int id)
    {
        var lop = _db.Lops.Find(id);
        if (lop == null) return NotFound();

        NapDanhMuc(lop.GiaoVienChuNhiemId);
        return View("Form", lop);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id, [Bind("Id,Ten,Khoi,NamHocId,GiaoVienChuNhiemId")] Lop model)
    {
        if (id != model.Id) return BadRequest();

        KiemTra(model, id);
        if (!ModelState.IsValid)
        {
            NapDanhMuc(model.GiaoVienChuNhiemId);
            return View("Form", model);
        }

        _db.Entry(model).State = EntityState.Modified;
        _db.SaveChanges();
        TempData["ThanhCong"] = "Đã cập nhật lớp.";
        return RedirectToAction(nameof(Index), new { namHocId = model.NamHocId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Delete(int id)
    {
        var lop = _db.Lops.Find(id);
        if (lop == null) return NotFound();

        if (_db.HocSinhs.Any(h => h.LopId == id))
        {
            TempData["Loi"] = "Không xóa được: lớp đang có học sinh.";
            return RedirectToAction(nameof(Index), new { namHocId = lop.NamHocId });
        }

        _db.PhanCongGiangDays.RemoveRange(_db.PhanCongGiangDays.Where(p => p.LopId == id));
        _db.Lops.Remove(lop);
        _db.SaveChanges();

        TempData["ThanhCong"] = "Đã xóa lớp.";
        return RedirectToAction(nameof(Index), new { namHocId = lop.NamHocId });
    }

    private void KiemTra(Lop model, int idHienTai)
    {
        if (_db.Lops.Any(l => l.Ten == model.Ten && l.NamHocId == model.NamHocId && l.Id != idHienTai))
            ModelState.AddModelError(nameof(Lop.Ten), "Lớp này đã tồn tại trong năm học đã chọn.");

        if (model.Khoi is < 10 or > 12)
            ModelState.AddModelError(nameof(Lop.Khoi), "Khối phải là 10, 11 hoặc 12.");
    }

    private void NapDanhMuc(int? giaoVienId)
    {
        ViewBag.DanhSachNamHoc = TaoSelectNamHoc(null);
        ViewBag.DanhSachGiaoVien = new SelectList(
            _db.GiaoViens.AsNoTracking()
                .Where(g => g.DangCongTac)
                .OrderBy(g => g.MaGiaoVien)
                .Select(g => new { g.Id, MoTa = g.MaGiaoVien + " - " + g.HoTen })
                .ToList(),
            "Id", "MoTa", giaoVienId);
    }

    private SelectList TaoSelectNamHoc(int? chon) =>
        new(_db.NamHocs.AsNoTracking().OrderByDescending(n => n.Ten).ToList(), "Id", "Ten", chon);

    private int LayNamHocMacDinh() =>
        _db.NamHocs.Where(n => n.DangHoatDong).Select(n => n.Id).FirstOrDefault();
}
