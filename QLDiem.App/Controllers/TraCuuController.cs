using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLDiem.Data;
using QLDiem.Models;
using QLDiem.Models.ViewModels;
using QLDiem.Services;

namespace QLDiem.Controllers;

/// <summary>Tra cứu điểm và kết quả học tập.</summary>
[Authorize]
public class TraCuuController : Controller
{
    private readonly QLDiemDbContext _db;
    private readonly KetQuaHocTapService _ketQua;
    private readonly DiemService _diem;

    public TraCuuController(QLDiemDbContext db, KetQuaHocTapService ketQua, DiemService diem)
    {
        _db = db;
        _ketQua = ketQua;
        _diem = diem;
    }

    public IActionResult Index(string? tuKhoa, int? lopId)
    {
        // Học sinh chỉ xem được kết quả của chính mình.
        if (User.IsInRole(VaiTro.HocSinh))
        {
            var hocSinhId = User.LayHocSinhId();
            if (hocSinhId == null)
            {
                TempData["Loi"] = "Tài khoản chưa được gắn với hồ sơ học sinh.";
                return RedirectToAction("Index", "Home");
            }

            return RedirectToAction(nameof(HocBa), new { id = hocSinhId });
        }

        var vm = new TraCuuVM
        {
            TuKhoa = tuKhoa,
            LopId = lopId,
            DanhSachLop = _db.Lops.AsNoTracking()
                .Where(l => l.NamHoc.DangHoatDong)
                .OrderBy(l => l.Khoi).ThenBy(l => l.Ten)
                .Select(l => new LopGonVM(l.Id, l.Ten, l.Khoi))
                .ToList()
        };

        if (string.IsNullOrWhiteSpace(tuKhoa) && lopId == null) return View(vm);

        var truyVan = _db.HocSinhs.AsNoTracking().Include(h => h.Lop).AsQueryable();

        if (!string.IsNullOrWhiteSpace(tuKhoa))
        {
            var tk = tuKhoa.Trim();
            truyVan = truyVan.Where(h => h.HoTen.Contains(tk) || h.MaHocSinh.Contains(tk));
        }

        if (lopId != null) truyVan = truyVan.Where(h => h.LopId == lopId);

        vm.KetQua = truyVan
            .OrderBy(h => h.MaHocSinh)
            .Take(100)
            .Select(h => new KetQuaTimHocSinhVM(h.Id, h.MaHocSinh, h.HoTen, h.Lop.Ten, h.NgaySinh))
            .ToList();

        return View(vm);
    }

    /// <summary>Học bạ: ĐTBm từng môn theo học kỳ, cả năm và xếp loại.</summary>
    public IActionResult HocBa(int id)
    {
        if (!DuocXem(id)) return Forbid();

        var hs = _db.HocSinhs.AsNoTracking().Include(h => h.Lop).FirstOrDefault(h => h.Id == id);
        if (hs == null) return NotFound();

        var hocBa = _ketQua.LayHocBa(id, hs.Lop.NamHocId);
        return hocBa == null ? NotFound() : View(hocBa);
    }

    /// <summary>Chi tiết từng đầu điểm của một học kỳ.</summary>
    public IActionResult ChiTiet(int id, HocKy hocKy = HocKy.HocKyI)
    {
        if (!DuocXem(id)) return Forbid();

        var hs = _db.HocSinhs.AsNoTracking().Include(h => h.Lop).FirstOrDefault(h => h.Id == id);
        if (hs == null) return NotFound();

        ViewBag.HocSinh = hs;
        ViewBag.HocKy = hocKy;
        return View(_diem.LayChiTietDiem(id, hocKy));
    }

    private bool DuocXem(int hocSinhId)
    {
        if (User.IsInRole(VaiTro.Admin) || User.IsInRole(VaiTro.GiaoVien)) return true;
        return User.LayHocSinhId() == hocSinhId;
    }
}
