using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLDiem.Data;
using QLDiem.Models;
using QLDiem.Models.ViewModels;
using QLDiem.Services;

namespace QLDiem.Controllers;

[Authorize(Roles = VaiTro.Admin + "," + VaiTro.GiaoVien)]
public class ThongKeController : Controller
{
    private readonly QLDiemDbContext _db;
    private readonly ThongKeService _thongKe;
    private readonly KetQuaHocTapService _ketQua;

    public ThongKeController(QLDiemDbContext db, ThongKeService thongKe, KetQuaHocTapService ketQua)
    {
        _db = db;
        _thongKe = thongKe;
        _ketQua = ketQua;
    }

    /// <summary>Phân bố xếp loại học tập theo lớp.</summary>
    public IActionResult Index(HocKy hocKy = HocKy.HocKyI, int? khoi = null)
    {
        var vm = TaoVm(hocKy);
        vm.Khoi = khoi;
        vm.TheoLop = _thongKe.ThongKeTheoLop(vm.NamHocId, hocKy, khoi);
        return View(vm);
    }

    /// <summary>Phổ điểm theo môn học.</summary>
    public IActionResult TheoMon(HocKy hocKy = HocKy.HocKyI, int? lopId = null)
    {
        var vm = TaoVm(hocKy);
        vm.LopId = lopId;
        vm.TheoMon = _thongKe.ThongKeTheoMon(vm.NamHocId, hocKy, lopId);
        return View(vm);
    }

    /// <summary>Bảng tổng kết và xếp hạng của một lớp.</summary>
    public IActionResult TongKetLop(int lopId, HocKy hocKy = HocKy.HocKyI)
    {
        var lop = _db.Lops.AsNoTracking()
            .Include(l => l.GiaoVienChuNhiem)
            .FirstOrDefault(l => l.Id == lopId);
        if (lop == null) return NotFound();

        ViewBag.Lop = lop;
        ViewBag.HocKy = hocKy;
        return View(_ketQua.LayBangTongKetLop(lopId, hocKy));
    }

    /// <summary>Tính lại và ghi kết quả học kỳ cho cả lớp.</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "QuanTri")]
    public IActionResult TongKet(int lopId, HocKy hocKy)
    {
        var kq = _ketQua.TongKetLop(lopId, hocKy);

        if (kq.ThanhCong) TempData["ThanhCong"] = kq.ThongBao;
        else TempData["Loi"] = string.Join(" ", kq.Loi);

        return RedirectToAction(nameof(TongKetLop), new { lopId, hocKy = (int)hocKy });
    }

    private ThongKeVM TaoVm(HocKy hocKy)
    {
        var namHoc = _thongKe.LayNamHocHienHanh();

        var vm = new ThongKeVM
        {
            HocKy = hocKy,
            NamHocId = namHoc?.Id ?? 0,
            TenNamHoc = namHoc?.Ten ?? "Chưa có năm học"
        };

        vm.DanhSachLop = _db.Lops.AsNoTracking()
            .Where(l => l.NamHocId == vm.NamHocId)
            .OrderBy(l => l.Khoi).ThenBy(l => l.Ten)
            .Select(l => new LopGonVM(l.Id, l.Ten, l.Khoi))
            .ToList();

        return vm;
    }
}
