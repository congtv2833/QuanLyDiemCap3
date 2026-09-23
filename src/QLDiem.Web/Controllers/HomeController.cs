using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QLDiem.Application.Interfaces;
using QLDiem.Domain.Enums;
using QLDiem.Web.Models;

namespace QLDiem.Web.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly IThongKeService _thongKe;

    public HomeController(IThongKeService thongKe) => _thongKe = thongKe;

    public async Task<IActionResult> Index(HocKy hocKy = HocKy.HocKyI)
    {
        ViewBag.HocKy = hocKy;
        var tongQuan = await _thongKe.LayTongQuanAsync(hocKy);
        return View(tongQuan);
    }

    [AllowAnonymous]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() =>
        View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
}
