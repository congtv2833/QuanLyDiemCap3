using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLDiem.Application.Interfaces;
using QLDiem.Domain.Common;
using QLDiem.Domain.Enums;
using QLDiem.Infrastructure.Data;
using QLDiem.Infrastructure.Identity;
using QLDiem.Web.Models;

namespace QLDiem.Web.Controllers;

/// <summary>Tra cứu điểm và kết quả học tập.</summary>
[Authorize]
public class TraCuuController : Controller
{
    private readonly QLDiemDbContext _db;
    private readonly IKetQuaHocTapService _ketQua;
    private readonly IDiemService _diem;
    private readonly UserManager<ApplicationUser> _userManager;

    public TraCuuController(
        QLDiemDbContext db,
        IKetQuaHocTapService ketQua,
        IDiemService diem,
        UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _ketQua = ketQua;
        _diem = diem;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index(string? tuKhoa, int? lopId)
    {
        // Học sinh chỉ xem được học bạ của chính mình.
        if (User.IsInRole(VaiTro.HocSinh))
        {
            var user = await _userManager.GetUserAsync(User);
            if (user?.HocSinhId is null)
            {
                TempData["Loi"] = "Tài khoản chưa được gắn với hồ sơ học sinh.";
                return RedirectToAction("Index", "Home");
            }

            return RedirectToAction(nameof(HocBa), new { id = user.HocSinhId });
        }

        var vm = new TraCuuVM
        {
            TuKhoa = tuKhoa,
            LopId = lopId,
            DanhSachLop = await _db.Lops.AsNoTracking()
                .Where(l => l.NamHoc.DangHoatDong)
                .OrderBy(l => l.Khoi).ThenBy(l => l.Ten)
                .Select(l => new LopGonVM(l.Id, l.Ten, l.Khoi))
                .ToListAsync()
        };

        if (string.IsNullOrWhiteSpace(tuKhoa) && lopId is null) return View(vm);

        var truyVan = _db.HocSinhs.AsNoTracking().Include(h => h.Lop).AsQueryable();

        if (!string.IsNullOrWhiteSpace(tuKhoa))
        {
            var tk = tuKhoa.Trim();
            truyVan = truyVan.Where(h => h.HoTen.Contains(tk) || h.MaHocSinh.Contains(tk));
        }

        if (lopId is not null) truyVan = truyVan.Where(h => h.LopId == lopId);

        vm.KetQua = await truyVan
            .OrderBy(h => h.MaHocSinh)
            .Take(100)
            .Select(h => new KetQuaTimHocSinhVM(h.Id, h.MaHocSinh, h.HoTen, h.Lop.Ten, h.NgaySinh))
            .ToListAsync();

        return View(vm);
    }

    /// <summary>Học bạ: ĐTBm từng môn theo học kỳ, cả năm và xếp loại.</summary>
    public async Task<IActionResult> HocBa(int id)
    {
        if (!await DuocXemAsync(id)) return Forbid();

        var hs = await _db.HocSinhs.AsNoTracking()
            .Include(h => h.Lop)
            .FirstOrDefaultAsync(h => h.Id == id);
        if (hs is null) return NotFound();

        var hocBa = await _ketQua.LayHocBaAsync(id, hs.Lop.NamHocId);
        return hocBa is null ? NotFound() : View(hocBa);
    }

    /// <summary>Chi tiết từng đầu điểm của một học kỳ.</summary>
    public async Task<IActionResult> ChiTiet(int id, HocKy hocKy = HocKy.HocKyI)
    {
        if (!await DuocXemAsync(id)) return Forbid();

        var hs = await _db.HocSinhs.AsNoTracking()
            .Include(h => h.Lop)
            .FirstOrDefaultAsync(h => h.Id == id);
        if (hs is null) return NotFound();

        ViewBag.HocSinh = hs;
        ViewBag.HocKy = hocKy;
        return View(await _diem.LayChiTietDiemAsync(id, hocKy));
    }

    private async Task<bool> DuocXemAsync(int hocSinhId)
    {
        if (User.IsInRole(VaiTro.Admin) || User.IsInRole(VaiTro.GiaoVien)) return true;

        var user = await _userManager.GetUserAsync(User);
        return user?.HocSinhId == hocSinhId;
    }
}
