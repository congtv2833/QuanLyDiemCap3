using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLDiem.Data;
using QLDiem.Models;

namespace QLDiem.Controllers;

[Authorize(Policy = "QuanTri")]
public class NamHocController : Controller
{
    private readonly QLDiemDbContext _db;

    public NamHocController(QLDiemDbContext db) => _db = db;

    public IActionResult Index() =>
        View(_db.NamHocs.AsNoTracking().OrderByDescending(n => n.Ten).ToList());

    public IActionResult Create() => View("Form", new NamHoc
    {
        NgayBatDau = new DateOnly(DateTime.Today.Year, 9, 5),
        NgayKetThuc = new DateOnly(DateTime.Today.Year + 1, 5, 31)
    });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create([Bind("Ten,NgayBatDau,NgayKetThuc,DangHoatDong")] NamHoc model)
    {
        if (_db.NamHocs.Any(n => n.Ten == model.Ten))
            ModelState.AddModelError(nameof(NamHoc.Ten), "Năm học này đã tồn tại.");

        if (!ModelState.IsValid) return View("Form", model);

        if (model.DangHoatDong) TatCacNamHocKhac(0);

        _db.NamHocs.Add(model);
        _db.SaveChanges();
        TempData["ThanhCong"] = $"Đã thêm năm học {model.Ten}.";
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Edit(int id)
    {
        var nam = _db.NamHocs.Find(id);
        return nam == null ? NotFound() : View("Form", nam);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id, [Bind("Id,Ten,NgayBatDau,NgayKetThuc,DangHoatDong")] NamHoc model)
    {
        if (id != model.Id) return BadRequest();

        if (_db.NamHocs.Any(n => n.Ten == model.Ten && n.Id != id))
            ModelState.AddModelError(nameof(NamHoc.Ten), "Năm học này đã tồn tại.");

        if (!ModelState.IsValid) return View("Form", model);

        if (model.DangHoatDong) TatCacNamHocKhac(id);

        _db.Entry(model).State = EntityState.Modified;
        _db.SaveChanges();
        TempData["ThanhCong"] = "Đã cập nhật năm học.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Delete(int id)
    {
        var nam = _db.NamHocs.Find(id);
        if (nam == null) return NotFound();

        if (_db.Lops.Any(l => l.NamHocId == id))
        {
            TempData["Loi"] = "Không xóa được: năm học đã có lớp.";
            return RedirectToAction(nameof(Index));
        }

        _db.NamHocs.Remove(nam);
        _db.SaveChanges();
        TempData["ThanhCong"] = "Đã xóa năm học.";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>Chỉ một năm học được đánh dấu đang hoạt động tại một thời điểm.</summary>
    private void TatCacNamHocKhac(int giuLaiId)
    {
        foreach (var n in _db.NamHocs.Where(n => n.DangHoatDong && n.Id != giuLaiId).ToList())
            n.DangHoatDong = false;
    }
}
