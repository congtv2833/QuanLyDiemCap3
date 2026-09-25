using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QLDiem.Data;
using QLDiem.Models;

namespace QLDiem.Controllers;

[Authorize(Policy = "QuanTri")]
public class PhanCongController : Controller
{
    private readonly QLDiemDbContext _db;

    public PhanCongController(QLDiemDbContext db) => _db = db;

    public IActionResult Index(int? lopId)
    {
        ViewBag.DanhSachLop = TaoSelectLop(lopId);
        ViewBag.LopId = lopId;

        if (lopId == null) return View(new List<PhanCongGiangDay>());

        var danhSach = _db.PhanCongGiangDays.AsNoTracking()
            .Include(p => p.MonHoc)
            .Include(p => p.GiaoVien)
            .Include(p => p.Lop)
            .Where(p => p.LopId == lopId)
            .OrderBy(p => p.MonHoc.ThuTuHienThi)
            .ToList();

        return View(danhSach);
    }

    public IActionResult Create(int? lopId)
    {
        NapDanhMuc(lopId, null, null);
        return View("Form", new PhanCongGiangDay { LopId = lopId ?? 0 });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create([Bind("LopId,MonHocId,GiaoVienId")] PhanCongGiangDay model)
    {
        var lop = _db.Lops.AsNoTracking().FirstOrDefault(l => l.Id == model.LopId);
        if (lop == null)
        {
            ModelState.AddModelError(nameof(PhanCongGiangDay.LopId), "Vui lòng chọn lớp.");
        }
        else
        {
            model.NamHocId = lop.NamHocId;

            if (_db.PhanCongGiangDays.Any(p => p.LopId == model.LopId && p.MonHocId == model.MonHocId))
                ModelState.AddModelError(nameof(PhanCongGiangDay.MonHocId),
                    "Môn này đã được phân công cho lớp.");
        }

        if (!ModelState.IsValid)
        {
            NapDanhMuc(model.LopId, model.MonHocId, model.GiaoVienId);
            return View("Form", model);
        }

        _db.PhanCongGiangDays.Add(model);
        _db.SaveChanges();
        TempData["ThanhCong"] = "Đã thêm phân công giảng dạy.";
        return RedirectToAction(nameof(Index), new { lopId = model.LopId });
    }

    public IActionResult Edit(int id)
    {
        var pc = _db.PhanCongGiangDays.Find(id);
        if (pc == null) return NotFound();

        NapDanhMuc(pc.LopId, pc.MonHocId, pc.GiaoVienId);
        return View("Form", pc);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id, [Bind("Id,LopId,MonHocId,GiaoVienId,NamHocId")] PhanCongGiangDay model)
    {
        if (id != model.Id) return BadRequest();

        if (_db.PhanCongGiangDays.Any(p => p.LopId == model.LopId
                                           && p.MonHocId == model.MonHocId
                                           && p.Id != id))
        {
            ModelState.AddModelError(nameof(PhanCongGiangDay.MonHocId), "Môn này đã được phân công cho lớp.");
        }

        if (!ModelState.IsValid)
        {
            NapDanhMuc(model.LopId, model.MonHocId, model.GiaoVienId);
            return View("Form", model);
        }

        _db.Entry(model).State = EntityState.Modified;
        _db.SaveChanges();
        TempData["ThanhCong"] = "Đã cập nhật phân công.";
        return RedirectToAction(nameof(Index), new { lopId = model.LopId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Delete(int id)
    {
        var pc = _db.PhanCongGiangDays.Find(id);
        if (pc == null) return NotFound();

        if (_db.Diems.Any(d => d.MonHocId == pc.MonHocId && d.HocSinh.LopId == pc.LopId))
        {
            TempData["Loi"] = "Không xóa được: lớp đã có điểm của môn này.";
            return RedirectToAction(nameof(Index), new { lopId = pc.LopId });
        }

        _db.PhanCongGiangDays.Remove(pc);
        _db.SaveChanges();
        TempData["ThanhCong"] = "Đã xóa phân công.";
        return RedirectToAction(nameof(Index), new { lopId = pc.LopId });
    }

    private void NapDanhMuc(int? lopId, int? monHocId, int? giaoVienId)
    {
        ViewBag.DanhSachLop = TaoSelectLop(lopId);

        ViewBag.DanhSachMon = new SelectList(
            _db.MonHocs.AsNoTracking().OrderBy(m => m.ThuTuHienThi)
                .Select(m => new { m.Id, m.TenMon }).ToList(),
            "Id", "TenMon", monHocId);

        ViewBag.DanhSachGiaoVien = new SelectList(
            _db.GiaoViens.AsNoTracking().Where(g => g.DangCongTac).OrderBy(g => g.MaGiaoVien)
                .Select(g => new { g.Id, MoTa = g.MaGiaoVien + " - " + g.HoTen }).ToList(),
            "Id", "MoTa", giaoVienId);
    }

    private SelectList TaoSelectLop(int? chon)
    {
        var lops = _db.Lops.AsNoTracking()
            .Include(l => l.NamHoc)
            .OrderByDescending(l => l.NamHoc.Ten).ThenBy(l => l.Ten)
            .Select(l => new { l.Id, MoTa = l.Ten + " (" + l.NamHoc.Ten + ")" })
            .ToList();

        return new SelectList(lops, "Id", "MoTa", chon);
    }
}
