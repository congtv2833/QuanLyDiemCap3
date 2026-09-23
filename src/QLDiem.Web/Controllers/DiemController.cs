using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLDiem.Application.DTOs;
using QLDiem.Application.Interfaces;
using QLDiem.Domain.Common;
using QLDiem.Domain.Enums;
using QLDiem.Infrastructure.Data;
using QLDiem.Infrastructure.Identity;
using QLDiem.Web.Models;

namespace QLDiem.Web.Controllers;

/// <summary>Nhập và sửa điểm theo lớp - môn - học kỳ.</summary>
[Authorize(Policy = "NhapDiem")]
public class DiemController : Controller
{
    private readonly QLDiemDbContext _db;
    private readonly IDiemService _diem;
    private readonly UserManager<ApplicationUser> _userManager;

    public DiemController(QLDiemDbContext db, IDiemService diem, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _diem = diem;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index(int? lopId, int? monHocId, HocKy hocKy = HocKy.HocKyI)
    {
        var vm = new ChonBangDiemVM { LopId = lopId, MonHocId = monHocId, HocKy = hocKy };
        var giaoVienId = await LayGiaoVienIdAsync();

        vm.DanhSachLop = await LayDanhSachLopAsync(giaoVienId);

        if (lopId is not null)
            vm.DanhSachMon = await LayDanhSachMonAsync(lopId.Value, giaoVienId);

        if (lopId is not null && monHocId is not null)
        {
            if (!await DuocPhepAsync(lopId.Value, monHocId.Value, giaoVienId))
                return Forbid();

            vm.BangDiem = await _diem.LayBangDiemLopAsync(lopId.Value, monHocId.Value, hocKy);
        }

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Luu(LuuBangDiemDto duLieu)
    {
        var giaoVienId = await LayGiaoVienIdAsync();
        if (!await DuocPhepAsync(duLieu.LopId, duLieu.MonHocId, giaoVienId)) return Forbid();

        var kq = await _diem.LuuBangDiemAsync(duLieu);

        if (kq.ThanhCong) TempData["ThanhCong"] = kq.ThongBao;
        else TempData["Loi"] = string.Join(" ", kq.Loi.Take(5));

        return RedirectToAction(nameof(Index), new
        {
            lopId = duLieu.LopId,
            monHocId = duLieu.MonHocId,
            hocKy = duLieu.HocKy
        });
    }

    /// <summary>Id hồ sơ giáo viên của người đang đăng nhập; null nếu là quản trị viên.</summary>
    private async Task<int?> LayGiaoVienIdAsync()
    {
        if (User.IsInRole(VaiTro.Admin)) return null;
        var user = await _userManager.GetUserAsync(User);
        return user?.GiaoVienId;
    }

    /// <summary>Quản trị viên vào được mọi lớp; giáo viên chỉ vào lớp - môn mình được phân công.</summary>
    private async Task<bool> DuocPhepAsync(int lopId, int monHocId, int? giaoVienId)
    {
        if (giaoVienId is null) return User.IsInRole(VaiTro.Admin);

        return await _db.PhanCongGiangDays.AnyAsync(p => p.LopId == lopId
                                                         && p.MonHocId == monHocId
                                                         && p.GiaoVienId == giaoVienId);
    }

    private async Task<List<LopGonVM>> LayDanhSachLopAsync(int? giaoVienId)
    {
        var truyVan = _db.Lops.AsNoTracking()
            .Where(l => l.NamHoc.DangHoatDong);

        if (giaoVienId is not null)
        {
            truyVan = truyVan.Where(l => l.PhanCongs.Any(p => p.GiaoVienId == giaoVienId));
        }

        return await truyVan
            .OrderBy(l => l.Khoi).ThenBy(l => l.Ten)
            .Select(l => new LopGonVM(l.Id, l.Ten, l.Khoi))
            .ToListAsync();
    }

    private async Task<List<MonHocGonVM>> LayDanhSachMonAsync(int lopId, int? giaoVienId)
    {
        var truyVan = _db.PhanCongGiangDays.AsNoTracking()
            .Where(p => p.LopId == lopId);

        if (giaoVienId is not null)
            truyVan = truyVan.Where(p => p.GiaoVienId == giaoVienId);

        var mons = await truyVan
            .Select(p => p.MonHoc)
            .OrderBy(m => m.ThuTuHienThi)
            .ToListAsync();

        return mons
            .Select(m => new MonHocGonVM(m.Id, m.TenMon, m.LoaiDanhGia, m.SoDauDiemThuongXuyen))
            .ToList();
    }
}
