using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using QLDiem.Infrastructure.Identity;
using QLDiem.Web.Models;

namespace QLDiem.Web.Controllers;

public class AccountController : Controller
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public AccountController(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager)
    {
        _signInManager = signInManager;
        _userManager = userManager;
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true) return RedirectToAction("Index", "Home");
        return View(new DangNhapVM { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [AllowAnonymous]
    public async Task<IActionResult> Login(DangNhapVM vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var user = await _userManager.FindByNameAsync(vm.TenDangNhap);
        if (user is null || !user.DangHoatDong)
        {
            ModelState.AddModelError(string.Empty, "Tên đăng nhập hoặc mật khẩu không đúng.");
            return View(vm);
        }

        var kq = await _signInManager.PasswordSignInAsync(
            user, vm.MatKhau, vm.GhiNho, lockoutOnFailure: true);

        if (kq.IsLockedOut)
        {
            ModelState.AddModelError(string.Empty, "Tài khoản đang bị khóa tạm thời do đăng nhập sai nhiều lần.");
            return View(vm);
        }

        if (!kq.Succeeded)
        {
            ModelState.AddModelError(string.Empty, "Tên đăng nhập hoặc mật khẩu không đúng.");
            return View(vm);
        }

        if (!string.IsNullOrEmpty(vm.ReturnUrl) && Url.IsLocalUrl(vm.ReturnUrl))
            return Redirect(vm.ReturnUrl);

        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction(nameof(Login));
    }

    [AllowAnonymous]
    public IActionResult AccessDenied() => View();

    [HttpGet]
    [Authorize]
    public IActionResult DoiMatKhau() => View(new DoiMatKhauVM());

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DoiMatKhau(DoiMatKhauVM vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var user = await _userManager.GetUserAsync(User);
        if (user is null) return RedirectToAction(nameof(Login));

        var kq = await _userManager.ChangePasswordAsync(user, vm.MatKhauCu, vm.MatKhauMoi);
        if (!kq.Succeeded)
        {
            foreach (var loi in kq.Errors) ModelState.AddModelError(string.Empty, loi.Description);
            return View(vm);
        }

        await _signInManager.RefreshSignInAsync(user);
        TempData["ThanhCong"] = "Đã đổi mật khẩu.";
        return RedirectToAction("Index", "Home");
    }
}
