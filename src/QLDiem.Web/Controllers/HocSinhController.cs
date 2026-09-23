using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QLDiem.Domain.Common;
using QLDiem.Domain.Entities;
using QLDiem.Infrastructure.Common;
using QLDiem.Infrastructure.Data;

namespace QLDiem.Web.Controllers;

[Authorize(Roles = VaiTro.Admin + "," + VaiTro.GiaoVien)]
public class HocSinhController : Controller
{
    private const int SoDongMoiTrang = 20;

    private readonly QLDiemDbContext _db;

    public HocSinhController(QLDiemDbContext db) => _db = db;

    public async Task<IActionResult> Index(string? tuKhoa, int? lopId, int trang = 1)
    {
        var truyVan = _db.HocSinhs.AsNoTracking()
            .Include(h => h.Lop)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(tuKhoa))
        {
            var tk = tuKhoa.Trim();
            truyVan = truyVan.Where(h => h.HoTen.Contains(tk) || h.MaHocSinh.Contains(tk));
        }

        if (lopId is not null) truyVan = truyVan.Where(h => h.LopId == lopId);

        var tongSo = await truyVan.CountAsync();
        var danhSach = await truyVan
            .OrderBy(h => h.MaHocSinh)
            .Skip((trang - 1) * SoDongMoiTrang)
            .Take(SoDongMoiTrang)
            .ToListAsync();

        ViewBag.TuKhoa = tuKhoa;
        ViewBag.LopId = lopId;
        ViewBag.Trang = trang;
        ViewBag.TongSoTrang = (int)Math.Ceiling(tongSo / (double)SoDongMoiTrang);
        ViewBag.TongSo = tongSo;
        ViewBag.DanhSachLop = await TaoSelectLopAsync(lopId);

        return View(danhSach);
    }

    public async Task<IActionResult> Create()
    {
        ViewBag.DanhSachLop = await TaoSelectLopAsync(null);
        return View("Form", new HocSinh
        {
            MaHocSinh = await SinhMaMoiAsync(),
            NgaySinh = new DateOnly(DateTime.Today.Year - 16, 1, 1)
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("MaHocSinh,HoTen,NgaySinh,GioiTinh,NoiSinh,DiaChi," +
        "DienThoai,Email,HoTenPhuHuynh,DienThoaiPhuHuynh,LopId,DangHoc")] HocSinh model)
    {
        if (await _db.HocSinhs.AnyAsync(h => h.MaHocSinh == model.MaHocSinh))
            ModelState.AddModelError(nameof(HocSinh.MaHocSinh), "Mã học sinh đã tồn tại.");

        if (!ModelState.IsValid)
        {
            ViewBag.DanhSachLop = await TaoSelectLopAsync(model.LopId);
            return View("Form", model);
        }

        _db.HocSinhs.Add(model);
        await _db.SaveChangesAsync();
        TempData["ThanhCong"] = $"Đã thêm học sinh {model.HoTen}.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var hs = await _db.HocSinhs.FindAsync(id);
        if (hs is null) return NotFound();

        ViewBag.DanhSachLop = await TaoSelectLopAsync(hs.LopId);
        return View("Form", hs);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,MaHocSinh,HoTen,NgaySinh,GioiTinh,NoiSinh," +
        "DiaChi,DienThoai,Email,HoTenPhuHuynh,DienThoaiPhuHuynh,LopId,DangHoc,UserId")] HocSinh model)
    {
        if (id != model.Id) return BadRequest();

        if (await _db.HocSinhs.AnyAsync(h => h.MaHocSinh == model.MaHocSinh && h.Id != id))
            ModelState.AddModelError(nameof(HocSinh.MaHocSinh), "Mã học sinh đã tồn tại.");

        if (!ModelState.IsValid)
        {
            ViewBag.DanhSachLop = await TaoSelectLopAsync(model.LopId);
            return View("Form", model);
        }

        _db.Entry(model).State = EntityState.Modified;
        await _db.SaveChangesAsync();
        TempData["ThanhCong"] = "Đã cập nhật hồ sơ học sinh.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Authorize(Policy = "QuanTri")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var hs = await _db.HocSinhs.FindAsync(id);
        if (hs is null) return NotFound();

        if (await _db.Diems.AnyAsync(d => d.HocSinhId == id))
        {
            // Không xóa cứng học sinh đã có điểm; chuyển sang trạng thái thôi học.
            hs.DangHoc = false;
            await _db.SaveChangesAsync();
            TempData["ThanhCong"] = $"Học sinh {hs.HoTen} đã có điểm nên được chuyển sang trạng thái thôi học.";
            return RedirectToAction(nameof(Index));
        }

        _db.HocSinhs.Remove(hs);
        await _db.SaveChangesAsync();
        TempData["ThanhCong"] = "Đã xóa học sinh.";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>Hồ sơ chi tiết kèm kết quả học tập.</summary>
    public async Task<IActionResult> ChiTiet(int id)
    {
        var hs = await _db.HocSinhs.AsNoTracking()
            .Include(h => h.Lop).ThenInclude(l => l.NamHoc)
            .Include(h => h.Lop).ThenInclude(l => l.GiaoVienChuNhiem)
            .FirstOrDefaultAsync(h => h.Id == id);

        return hs is null ? NotFound() : View(hs);
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

    private async Task<string> SinhMaMoiAsync()
    {
        var maCuoi = await _db.HocSinhs.OrderByDescending(h => h.MaHocSinh)
            .Select(h => h.MaHocSinh)
            .FirstOrDefaultAsync();

        if (maCuoi is null || !int.TryParse(maCuoi.AsSpan(2), out var so)) return "HS0001";
        return $"HS{so + 1:D4}";
    }
}
