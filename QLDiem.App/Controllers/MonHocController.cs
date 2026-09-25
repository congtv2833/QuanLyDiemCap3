using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLDiem.Data;
using QLDiem.Models;

namespace QLDiem.Controllers;

[Authorize(Policy = "QuanTri")]
public class MonHocController : Controller
{
    private readonly QLDiemDbContext _db;

    public MonHocController(QLDiemDbContext db) => _db = db;

    public IActionResult Index() =>
        View(_db.MonHocs.AsNoTracking().OrderBy(m => m.ThuTuHienThi).ToList());

    public IActionResult Create() => View("Form", new MonHoc { SoTietNam = 70, ThuTuHienThi = 99 });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(
        [Bind("MaMon,TenMon,SoTietNam,LoaiDanhGia,BatBuoc,ThuTuHienThi")] MonHoc model)
    {
        KiemTra(model, 0);
        if (!ModelState.IsValid) return View("Form", model);

        _db.MonHocs.Add(model);
        _db.SaveChanges();
        TempData["ThanhCong"] = $"Đã thêm môn {model.TenMon}.";
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Edit(int id)
    {
        var mon = _db.MonHocs.Find(id);
        return mon == null ? NotFound() : View("Form", mon);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id,
        [Bind("Id,MaMon,TenMon,SoTietNam,LoaiDanhGia,BatBuoc,ThuTuHienThi")] MonHoc model)
    {
        if (id != model.Id) return BadRequest();

        KiemTra(model, id);
        KiemTraThayDoiAnhHuongDenDiem(model, id);
        if (!ModelState.IsValid) return View("Form", model);

        _db.Entry(model).State = EntityState.Modified;
        _db.SaveChanges();
        TempData["ThanhCong"] = "Đã cập nhật môn học.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Delete(int id)
    {
        var mon = _db.MonHocs.Find(id);
        if (mon == null) return NotFound();

        if (_db.Diems.Any(d => d.MonHocId == id) || _db.PhanCongGiangDays.Any(p => p.MonHocId == id))
        {
            TempData["Loi"] = "Không xóa được: môn học đã được phân công hoặc đã có điểm.";
            return RedirectToAction(nameof(Index));
        }

        _db.MonHocs.Remove(mon);
        _db.SaveChanges();
        TempData["ThanhCong"] = "Đã xóa môn học.";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Chặn những thay đổi làm hỏng dữ liệu điểm đã nhập:
    /// đổi hình thức đánh giá, hoặc giảm số tiết khiến số ĐĐGtx ít đi so với điểm đang có.
    /// </summary>
    private void KiemTraThayDoiAnhHuongDenDiem(MonHoc model, int id)
    {
        var goc = _db.MonHocs.AsNoTracking().FirstOrDefault(m => m.Id == id);
        if (goc == null) return;

        bool daCoDiem = _db.Diems.Any(d => d.MonHocId == id);
        bool daCoNhanXet = _db.DanhGiaNhanXets.Any(n => n.MonHocId == id);

        if (goc.LoaiDanhGia != model.LoaiDanhGia && (daCoDiem || daCoNhanXet))
        {
            ModelState.AddModelError(nameof(MonHoc.LoaiDanhGia),
                "Không đổi được hình thức đánh giá vì môn này đã có dữ liệu. Hãy xóa hết điểm hoặc nhận xét trước.");
        }

        if (!daCoDiem) return;

        int soTxMoi = model.SoDauDiemThuongXuyen;
        int thuTuLonNhat = _db.Diems
            .Where(d => d.MonHocId == id && d.LoaiDiem == LoaiDiem.ThuongXuyen)
            .Max(d => (int?)d.ThuTu) ?? 0;

        if (thuTuLonNhat > soTxMoi)
        {
            ModelState.AddModelError(nameof(MonHoc.SoTietNam),
                $"Số tiết mới chỉ cho phép {soTxMoi} đầu điểm thường xuyên, " +
                $"nhưng môn này đã có điểm ở cột thứ {thuTuLonNhat}. Hãy xóa các cột điểm thừa trước.");
        }
    }

    private void KiemTra(MonHoc model, int idHienTai)
    {
        if (_db.MonHocs.Any(m => m.MaMon == model.MaMon && m.Id != idHienTai))
            ModelState.AddModelError(nameof(MonHoc.MaMon), "Mã môn đã tồn tại.");

        if (model.SoTietNam <= 0)
            ModelState.AddModelError(nameof(MonHoc.SoTietNam), "Số tiết trong năm phải lớn hơn 0.");
    }
}
