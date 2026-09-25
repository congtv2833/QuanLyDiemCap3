using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QLDiem.Models;
using QLDiem.Models.ViewModels;
using QLDiem.Services;

namespace QLDiem.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly ThongKeService _thongKe;

    public HomeController(ThongKeService thongKe) => _thongKe = thongKe;

    public IActionResult Index(HocKy hocKy = HocKy.HocKyI)
    {
        ViewBag.HocKy = hocKy;
        return View(_thongKe.LayTongQuan(hocKy));
    }

    [AllowAnonymous]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() =>
        View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
}
