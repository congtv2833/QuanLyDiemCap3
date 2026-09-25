using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLDiem.Data;
using QLDiem.Models;
using QLDiem.Models.ViewModels;
using QLDiem.Services;

namespace QLDiem.Controllers;

/// <summary>Nhập và sửa điểm theo lớp - môn - học kỳ.</summary>
[Authorize(Policy = "NhapDiem")]
public class DiemController : Controller
{
    private readonly QLDiemDbContext _db;
    private readonly DiemService _diem;

    public DiemController(QLDiemDbContext db, DiemService diem)
    {
        _db = db;
        _diem = diem;
    }

    public IActionResult Index(int? lopId, int? monHocId, HocKy hocKy = HocKy.HocKyI)
    {
        var vm = new ChonBangDiemVM { LopId = lopId, MonHocId = monHocId, HocKy = hocKy };

        // Quản trị viên xem mọi lớp; giáo viên chỉ thấy lớp mình được phân công.
        var giaoVienId = User.IsInRole(VaiTro.Admin) ? null : User.LayGiaoVienId();

        vm.DanhSachLop = LayDanhSachLop(giaoVienId);

        if (lopId != null)
            vm.DanhSachMon = LayDanhSachMon(lopId.Value, giaoVienId);

        if (lopId != null && monHocId != null)
        {
            if (!DuocPhep(lopId.Value, monHocId.Value, giaoVienId)) return Forbid();

            vm.BangDiem = _diem.LayBangDiemLop(lopId.Value, monHocId.Value, hocKy);
        }

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Luu(LuuBangDiemVM duLieu)
    {
        var giaoVienId = User.IsInRole(VaiTro.Admin) ? null : User.LayGiaoVienId();
        if (!DuocPhep(duLieu.LopId, duLieu.MonHocId, giaoVienId)) return Forbid();

        var kq = _diem.LuuBangDiem(duLieu);

        if (kq.ThanhCong) TempData["ThanhCong"] = kq.ThongBao;
        else TempData["Loi"] = string.Join(" ", kq.Loi.Take(5));

        return RedirectToAction(nameof(Index), new
        {
            lopId = duLieu.LopId,
            monHocId = duLieu.MonHocId,
            hocKy = (int)duLieu.HocKy
        });
    }

    /// <summary>Quản trị viên vào được mọi lớp; giáo viên chỉ vào lớp - môn mình được phân công.</summary>
    private bool DuocPhep(int lopId, int monHocId, int? giaoVienId)
    {
        if (giaoVienId == null) return User.IsInRole(VaiTro.Admin);

        return _db.PhanCongGiangDays.Any(p => p.LopId == lopId
                                              && p.MonHocId == monHocId
                                              && p.GiaoVienId == giaoVienId);
    }

    private List<LopGonVM> LayDanhSachLop(int? giaoVienId)
    {
        var truyVan = _db.Lops.AsNoTracking().Where(l => l.NamHoc.DangHoatDong);

        if (giaoVienId != null)
            truyVan = truyVan.Where(l => l.PhanCongs.Any(p => p.GiaoVienId == giaoVienId));

        return truyVan
            .OrderBy(l => l.Khoi).ThenBy(l => l.Ten)
            .Select(l => new LopGonVM(l.Id, l.Ten, l.Khoi))
            .ToList();
    }

    private List<MonHocGonVM> LayDanhSachMon(int lopId, int? giaoVienId)
    {
        var truyVan = _db.PhanCongGiangDays.AsNoTracking().Where(p => p.LopId == lopId);

        if (giaoVienId != null)
            truyVan = truyVan.Where(p => p.GiaoVienId == giaoVienId);

        return truyVan
            .Select(p => p.MonHoc)
            .OrderBy(m => m.ThuTuHienThi)
            .ToList()
            .Select(m => new MonHocGonVM(m.Id, m.TenMon, m.LoaiDanhGia, m.SoDauDiemThuongXuyen))
            .ToList();
    }
}
