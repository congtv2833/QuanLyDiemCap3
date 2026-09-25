using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QLDiem.Data;
using QLDiem.Models;
using QLDiem.Models.ViewModels;

namespace QLDiem.Controllers;

/// <summary>
/// Cấp và quản lý tài khoản đăng nhập cho giáo viên, học sinh.
/// Controller này dùng async vì ASP.NET Core Identity chỉ cung cấp API bất đồng bộ.
/// </summary>
[Authorize(Policy = "QuanTri")]
public class TaiKhoanController : Controller
{
    private readonly QLDiemDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public TaiKhoanController(QLDiemDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index(string? tuKhoa, string? vaiTro)
    {
        var users = await _userManager.Users.AsNoTracking().OrderBy(u => u.UserName).ToListAsync();

        var tenGiaoVien = await _db.GiaoViens.AsNoTracking()
            .ToDictionaryAsync(g => g.Id, g => g.MaGiaoVien + " - " + g.HoTen);
        var tenHocSinh = await _db.HocSinhs.AsNoTracking()
            .ToDictionaryAsync(h => h.Id, h => h.MaHocSinh + " - " + h.HoTen);

        var danhSach = new List<TaiKhoanVM>();

        foreach (var u in users)
        {
            var cacVaiTro = await _userManager.GetRolesAsync(u);

            danhSach.Add(new TaiKhoanVM
            {
                Id = u.Id,
                TenDangNhap = u.UserName ?? string.Empty,
                HoTen = u.HoTen,
                VaiTro = cacVaiTro.FirstOrDefault() ?? "(chưa gán)",
                HoSoLienKet = u.GiaoVienId != null && tenGiaoVien.TryGetValue(u.GiaoVienId.Value, out var gv)
                    ? gv
                    : u.HocSinhId != null && tenHocSinh.TryGetValue(u.HocSinhId.Value, out var hs)
                        ? hs
                        : null,
                DangHoatDong = u.DangHoatDong,
                DangBiKhoa = u.LockoutEnd != null && u.LockoutEnd > DateTimeOffset.Now
            });
        }

        if (!string.IsNullOrWhiteSpace(tuKhoa))
        {
            var tk = tuKhoa.Trim();
            danhSach = danhSach
                .Where(t => t.TenDangNhap.Contains(tk, StringComparison.OrdinalIgnoreCase)
                            || t.HoTen.Contains(tk, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        if (!string.IsNullOrWhiteSpace(vaiTro))
            danhSach = danhSach.Where(t => t.VaiTro == vaiTro).ToList();

        ViewBag.TuKhoa = tuKhoa;
        ViewBag.VaiTro = vaiTro;
        ViewBag.MatKhauMacDinh = DuLieuMau.MatKhauMacDinh;

        return View(danhSach);
    }

    public async Task<IActionResult> Create()
    {
        await NapDanhMucAsync(null, null);
        return View("Form", new TaoTaiKhoanVM { MatKhau = DuLieuMau.MatKhauMacDinh, XacNhanMatKhau = DuLieuMau.MatKhauMacDinh });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TaoTaiKhoanVM vm)
    {
        await KiemTraAsync(vm);

        if (!ModelState.IsValid)
        {
            await NapDanhMucAsync(vm.GiaoVienId, vm.HocSinhId);
            return View("Form", vm);
        }

        var user = new ApplicationUser
        {
            UserName = vm.TenDangNhap.Trim().ToLowerInvariant(),
            Email = vm.Email,
            EmailConfirmed = true,
            HoTen = vm.HoTen.Trim(),
            GiaoVienId = vm.VaiTro == VaiTro.GiaoVien ? vm.GiaoVienId : null,
            HocSinhId = vm.VaiTro == VaiTro.HocSinh ? vm.HocSinhId : null
        };

        var ketQua = await _userManager.CreateAsync(user, vm.MatKhau);
        if (!ketQua.Succeeded)
        {
            foreach (var loi in ketQua.Errors) ModelState.AddModelError(string.Empty, loi.Description);
            await NapDanhMucAsync(vm.GiaoVienId, vm.HocSinhId);
            return View("Form", vm);
        }

        await _userManager.AddToRoleAsync(user, vm.VaiTro);

        TempData["ThanhCong"] = $"Đã tạo tài khoản {user.UserName} cho {VaiTro.TenHienThi(vm.VaiTro).ToLower()}.";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>Đặt lại mật khẩu về mặc định, dùng khi người dùng quên mật khẩu.</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DatLaiMatKhau(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var ketQua = await _userManager.ResetPasswordAsync(user, token, DuLieuMau.MatKhauMacDinh);

        if (ketQua.Succeeded)
        {
            await _userManager.SetLockoutEndDateAsync(user, null);
            await _userManager.ResetAccessFailedCountAsync(user);
            TempData["ThanhCong"] =
                $"Đã đặt lại mật khẩu của {user.UserName} về {DuLieuMau.MatKhauMacDinh}. Nhắc người dùng đổi lại sau khi đăng nhập.";
        }
        else
        {
            TempData["Loi"] = string.Join(" ", ketQua.Errors.Select(e => e.Description));
        }

        return RedirectToAction(nameof(Index));
    }

    /// <summary>Khóa hoặc mở khóa tài khoản. Tài khoản bị khóa không đăng nhập được.</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DoiTrangThai(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();

        if (user.UserName == User.Identity?.Name)
        {
            TempData["Loi"] = "Không thể tự khóa tài khoản đang đăng nhập.";
            return RedirectToAction(nameof(Index));
        }

        user.DangHoatDong = !user.DangHoatDong;
        await _userManager.UpdateAsync(user);

        TempData["ThanhCong"] = user.DangHoatDong
            ? $"Đã mở khóa tài khoản {user.UserName}."
            : $"Đã khóa tài khoản {user.UserName}.";

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();

        if (user.UserName == User.Identity?.Name)
        {
            TempData["Loi"] = "Không thể xóa tài khoản đang đăng nhập.";
            return RedirectToAction(nameof(Index));
        }

        if (await LaQuanTriVienCuoiCungAsync(user))
        {
            TempData["Loi"] = "Không thể xóa: đây là quản trị viên cuối cùng của hệ thống.";
            return RedirectToAction(nameof(Index));
        }

        await _userManager.DeleteAsync(user);
        TempData["ThanhCong"] = $"Đã xóa tài khoản {user.UserName}.";
        return RedirectToAction(nameof(Index));
    }

    private async Task KiemTraAsync(TaoTaiKhoanVM vm)
    {
        var tenDangNhap = vm.TenDangNhap.Trim().ToLowerInvariant();

        if (await _userManager.FindByNameAsync(tenDangNhap) != null)
            ModelState.AddModelError(nameof(vm.TenDangNhap), "Tên đăng nhập đã tồn tại.");

        if (!VaiTro.TatCa.Contains(vm.VaiTro))
        {
            ModelState.AddModelError(nameof(vm.VaiTro), "Vai trò không hợp lệ.");
            return;
        }

        if (vm.VaiTro == VaiTro.GiaoVien)
        {
            if (vm.GiaoVienId == null)
            {
                ModelState.AddModelError(nameof(vm.GiaoVienId), "Vui lòng chọn hồ sơ giáo viên.");
            }
            else if (await _userManager.Users.AnyAsync(u => u.GiaoVienId == vm.GiaoVienId))
            {
                ModelState.AddModelError(nameof(vm.GiaoVienId), "Giáo viên này đã có tài khoản.");
            }
        }

        if (vm.VaiTro == VaiTro.HocSinh)
        {
            if (vm.HocSinhId == null)
            {
                ModelState.AddModelError(nameof(vm.HocSinhId), "Vui lòng chọn hồ sơ học sinh.");
            }
            else if (await _userManager.Users.AnyAsync(u => u.HocSinhId == vm.HocSinhId))
            {
                ModelState.AddModelError(nameof(vm.HocSinhId), "Học sinh này đã có tài khoản.");
            }
        }
    }

    /// <summary>Danh sách hồ sơ chưa có tài khoản, để không gán trùng.</summary>
    private async Task NapDanhMucAsync(int? giaoVienId, int? hocSinhId)
    {
        var gvDaCo = await _userManager.Users.Where(u => u.GiaoVienId != null)
            .Select(u => u.GiaoVienId!.Value).ToListAsync();
        var hsDaCo = await _userManager.Users.Where(u => u.HocSinhId != null)
            .Select(u => u.HocSinhId!.Value).ToListAsync();

        ViewBag.DanhSachGiaoVien = new SelectList(
            await _db.GiaoViens.AsNoTracking()
                .Where(g => g.DangCongTac && (!gvDaCo.Contains(g.Id) || g.Id == giaoVienId))
                .OrderBy(g => g.MaGiaoVien)
                .Select(g => new { g.Id, MoTa = g.MaGiaoVien + " - " + g.HoTen })
                .ToListAsync(),
            "Id", "MoTa", giaoVienId);

        ViewBag.DanhSachHocSinh = new SelectList(
            await _db.HocSinhs.AsNoTracking()
                .Where(h => h.DangHoc && (!hsDaCo.Contains(h.Id) || h.Id == hocSinhId))
                .OrderBy(h => h.MaHocSinh)
                .Select(h => new { h.Id, MoTa = h.MaHocSinh + " - " + h.HoTen })
                .ToListAsync(),
            "Id", "MoTa", hocSinhId);
    }

    private async Task<bool> LaQuanTriVienCuoiCungAsync(ApplicationUser user)
    {
        if (!await _userManager.IsInRoleAsync(user, VaiTro.Admin)) return false;
        var admins = await _userManager.GetUsersInRoleAsync(VaiTro.Admin);
        return admins.Count <= 1;
    }
}
