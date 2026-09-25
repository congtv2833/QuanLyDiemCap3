using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLDiem.Data;
using QLDiem.Models;
using QLDiem.Models.ViewModels;
using QLDiem.Services;

namespace QLDiem.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly QLDiemDbContext _db;
    private readonly ThongKeService _thongKe;
    private readonly KetQuaHocTapService _ketQua;

    public HomeController(QLDiemDbContext db, ThongKeService thongKe, KetQuaHocTapService ketQua)
    {
        _db = db;
        _thongKe = thongKe;
        _ketQua = ketQua;
    }

    public IActionResult Index(HocKy hocKy = HocKy.HocKyI)
    {
        // Học sinh chỉ thấy kết quả của chính mình, không thấy số liệu toàn trường.
        if (User.IsInRole(VaiTro.HocSinh)) return TrangChuHocSinh();

        ViewBag.HocKy = hocKy;

        // Giáo viên chỉ thấy các lớp mình dạy hoặc chủ nhiệm; quản trị viên thấy tất cả.
        var giaoVienId = User.IsInRole(VaiTro.Admin) ? null : User.LayGiaoVienId();
        ViewBag.PhamVi = giaoVienId == null ? "Toàn trường" : "Các lớp tôi phụ trách";

        return View(_thongKe.LayTongQuan(hocKy, giaoVienId));
    }

    private IActionResult TrangChuHocSinh()
    {
        var hocSinhId = User.LayHocSinhId();
        if (hocSinhId == null)
        {
            TempData["Loi"] = "Tài khoản chưa được gắn với hồ sơ học sinh. Vui lòng liên hệ quản trị viên.";
            return View("HocSinhChuaCoHoSo");
        }

        var hs = _db.HocSinhs.AsNoTracking()
            .Include(h => h.Lop)
            .FirstOrDefault(h => h.Id == hocSinhId);
        if (hs == null) return View("HocSinhChuaCoHoSo");

        var hocBa = _ketQua.LayHocBa(hs.Id, hs.Lop.NamHocId);
        return hocBa == null ? View("HocSinhChuaCoHoSo") : View("TrangChuHocSinh", hocBa);
    }

    [AllowAnonymous]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() =>
        View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
}
