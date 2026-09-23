using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLDiem.Application.Interfaces;
using QLDiem.Domain.Common;
using QLDiem.Domain.Enums;
using QLDiem.Infrastructure.Data;
using QLDiem.Web.Models;

namespace QLDiem.Web.Controllers;

[Authorize(Roles = VaiTro.Admin + "," + VaiTro.GiaoVien)]
public class ThongKeController : Controller
{
    private readonly QLDiemDbContext _db;
    private readonly IThongKeService _thongKe;
    private readonly IKetQuaHocTapService _ketQua;

    public ThongKeController(QLDiemDbContext db, IThongKeService thongKe, IKetQuaHocTapService ketQua)
    {
        _db = db;
        _thongKe = thongKe;
        _ketQua = ketQua;
    }

    /// <summary>Phân bố xếp loại học tập theo lớp.</summary>
    public async Task<IActionResult> Index(HocKy hocKy = HocKy.HocKyI, int? khoi = null)
    {
        var vm = await TaoVmAsync(hocKy);
        vm.Khoi = khoi;
        vm.TheoLop = await _thongKe.ThongKeTheoLopAsync(vm.NamHocId, hocKy, khoi);
        return View(vm);
    }

    /// <summary>Phổ điểm theo môn học.</summary>
    public async Task<IActionResult> TheoMon(HocKy hocKy = HocKy.HocKyI, int? lopId = null)
    {
        var vm = await TaoVmAsync(hocKy);
        vm.LopId = lopId;
        vm.TheoMon = await _thongKe.ThongKeTheoMonAsync(vm.NamHocId, hocKy, lopId);
        return View(vm);
    }

    /// <summary>Bảng tổng kết và xếp hạng của một lớp.</summary>
    public async Task<IActionResult> TongKetLop(int lopId, HocKy hocKy = HocKy.HocKyI)
    {
        var lop = await _db.Lops.AsNoTracking()
            .Include(l => l.GiaoVienChuNhiem)
            .FirstOrDefaultAsync(l => l.Id == lopId);
        if (lop is null) return NotFound();

        ViewBag.Lop = lop;
        ViewBag.HocKy = hocKy;
        return View(await _ketQua.LayBangTongKetLopAsync(lopId, hocKy));
    }

    /// <summary>Tính lại và ghi kết quả học kỳ cho cả lớp.</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "QuanTri")]
    public async Task<IActionResult> TongKet(int lopId, HocKy hocKy)
    {
        var kq = await _ketQua.TongKetLopAsync(lopId, hocKy);

        if (kq.ThanhCong) TempData["ThanhCong"] = kq.ThongBao;
        else TempData["Loi"] = string.Join(" ", kq.Loi);

        return RedirectToAction(nameof(TongKetLop), new { lopId, hocKy });
    }

    private async Task<ThongKeVM> TaoVmAsync(HocKy hocKy)
    {
        var namHoc = await _db.NamHocs.AsNoTracking()
            .FirstOrDefaultAsync(n => n.DangHoatDong)
            ?? await _db.NamHocs.AsNoTracking().OrderByDescending(n => n.Id).FirstOrDefaultAsync();

        var vm = new ThongKeVM
        {
            HocKy = hocKy,
            NamHocId = namHoc?.Id ?? 0,
            TenNamHoc = namHoc?.Ten ?? "Chưa có năm học"
        };

        vm.DanhSachLop = await _db.Lops.AsNoTracking()
            .Where(l => l.NamHocId == vm.NamHocId)
            .OrderBy(l => l.Khoi).ThenBy(l => l.Ten)
            .Select(l => new LopGonVM(l.Id, l.Ten, l.Khoi))
            .ToListAsync();

        return vm;
    }
}
