using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QLDiem.Models;

namespace QLDiem.Data;

/// <summary>
/// Tạo dữ liệu mẫu để chạy thử và demo: vai trò, tài khoản, năm học,
/// môn học, lớp, học sinh và bảng điểm hai học kỳ.
/// Chỉ chạy khi cơ sở dữ liệu còn trống nên có thể gọi mỗi lần khởi động.
/// </summary>
public static class DuLieuMau
{
    public const string MatKhauMacDinh = "Abc@123";

    private const int SoHocSinhMoiLop = 15;

    private static readonly string[] HoDem =
    {
        "Nguyễn Văn", "Trần Thị", "Lê Hoàng", "Phạm Minh", "Hoàng Thu", "Vũ Đức",
        "Đặng Ngọc", "Bùi Quang", "Đỗ Thị", "Ngô Gia", "Dương Khánh", "Lý Thanh"
    };

    private static readonly string[] Ten =
    {
        "An", "Bình", "Chi", "Dũng", "Giang", "Hà", "Hiếu", "Huy", "Lan", "Linh",
        "Mai", "Minh", "Nam", "Phúc", "Quân", "Thảo", "Trang", "Tú", "Vy", "Yến"
    };

    /// <summary>
    /// Danh mục môn học. Giữ đủ 6 môn tính điểm vì Thông tư 22 yêu cầu
    /// ít nhất 6 môn đạt ngưỡng mới xếp được mức Tốt hoặc Khá.
    /// Số tiết mỗi năm quyết định số ĐĐGtx: tối đa 35 tiết thì 2 đầu điểm,
    /// 36-70 tiết thì 3, trên 70 tiết thì 4.
    /// </summary>
    private static readonly (string Ma, string Ten, int SoTiet, LoaiDanhGia Loai, bool BatBuoc)[] DanhSachMon =
    {
        ("NV",   "Ngữ văn",                            105, LoaiDanhGia.DiemSo,  true),
        ("TOAN", "Toán",                               105, LoaiDanhGia.DiemSo,  true),
        ("ANH",  "Tiếng Anh",                          105, LoaiDanhGia.DiemSo,  true),
        ("LY",   "Vật lí",                              70, LoaiDanhGia.DiemSo,  false),
        ("HOA",  "Hóa học",                             70, LoaiDanhGia.DiemSo,  false),
        ("SU",   "Lịch sử",                             52, LoaiDanhGia.DiemSo,  true),
        ("GDTC", "Giáo dục thể chất",                   70, LoaiDanhGia.NhanXet, true),
        ("HDTN", "Hoạt động trải nghiệm, hướng nghiệp", 105, LoaiDanhGia.NhanXet, true)
    };

    public static void KhoiTao(
        QLDiemDbContext db,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        db.Database.Migrate();

        TaoVaiTro(roleManager);
        TaoQuanTriVien(userManager);

        if (db.NamHocs.Any()) return; // đã có dữ liệu, không tạo lại

        var namHoc = new NamHoc
        {
            Ten = "2025-2026",
            NgayBatDau = new DateOnly(2025, 9, 5),
            NgayKetThuc = new DateOnly(2026, 5, 31),
            DangHoatDong = true
        };
        db.NamHocs.Add(namHoc);

        var monHocs = DanhSachMon.Select((m, i) => new MonHoc
        {
            MaMon = m.Ma,
            TenMon = m.Ten,
            SoTietNam = m.SoTiet,
            LoaiDanhGia = m.Loai,
            BatBuoc = m.BatBuoc,
            ThuTuHienThi = i + 1
        }).ToList();
        db.MonHocs.AddRange(monHocs);

        var rnd = new Random(2026); // cố định seed để dữ liệu mẫu tái lập được

        // Mỗi môn một giáo viên phụ trách.
        var giaoViens = monHocs.Select((m, i) => new GiaoVien
        {
            MaGiaoVien = $"GV{i + 1:D3}",
            HoTen = TaoHoTen(rnd),
            GioiTinh = i % 2 == 0 ? GioiTinh.Nam : GioiTinh.Nu,
            NgaySinh = new DateOnly(1980 + rnd.Next(0, 15), rnd.Next(1, 13), rnd.Next(1, 28)),
            ChuyenMon = m.TenMon,
            Email = $"gv{i + 1:D3}@thpt.edu.vn",
            DienThoai = $"09{rnd.Next(10000000, 99999999)}"
        }).ToList();
        db.GiaoViens.AddRange(giaoViens);

        db.SaveChanges();

        var lops = new List<Lop>
        {
            new() { Ten = "10A1", Khoi = 10, NamHocId = namHoc.Id, GiaoVienChuNhiemId = giaoViens[0].Id },
            new() { Ten = "10A2", Khoi = 10, NamHocId = namHoc.Id, GiaoVienChuNhiemId = giaoViens[1].Id }
        };
        db.Lops.AddRange(lops);
        db.SaveChanges();

        // Mỗi lớp học toàn bộ môn trong danh sách.
        foreach (var lop in lops)
        {
            for (int i = 0; i < monHocs.Count; i++)
            {
                db.PhanCongGiangDays.Add(new PhanCongGiangDay
                {
                    LopId = lop.Id,
                    MonHocId = monHocs[i].Id,
                    GiaoVienId = giaoViens[i].Id,
                    NamHocId = namHoc.Id
                });
            }
        }

        var hocSinhs = new List<HocSinh>();
        int stt = 1;
        foreach (var lop in lops)
        {
            for (int i = 0; i < SoHocSinhMoiLop; i++)
            {
                hocSinhs.Add(new HocSinh
                {
                    MaHocSinh = $"HS{stt:D3}",
                    HoTen = TaoHoTen(rnd),
                    NgaySinh = new DateOnly(2010, rnd.Next(1, 13), rnd.Next(1, 28)),
                    GioiTinh = rnd.Next(2) == 0 ? GioiTinh.Nam : GioiTinh.Nu,
                    NoiSinh = "Hà Nội",
                    DiaChi = $"Số {rnd.Next(1, 200)}, phường {rnd.Next(1, 20)}, Hà Nội",
                    HoTenPhuHuynh = TaoHoTen(rnd),
                    DienThoaiPhuHuynh = $"09{rnd.Next(10000000, 99999999)}",
                    LopId = lop.Id
                });
                stt++;
            }
        }
        db.HocSinhs.AddRange(hocSinhs);
        db.SaveChanges();

        TaoDiemMau(db, namHoc, monHocs, hocSinhs, rnd);
        TaoTaiKhoan(userManager, giaoViens, hocSinhs);
    }

    private static string TaoHoTen(Random rnd) =>
        $"{HoDem[rnd.Next(HoDem.Length)]} {Ten[rnd.Next(Ten.Length)]}";

    private static void TaoVaiTro(RoleManager<IdentityRole> roleManager)
    {
        foreach (var vaiTro in VaiTro.TatCa)
        {
            if (!roleManager.RoleExistsAsync(vaiTro).GetAwaiter().GetResult())
                roleManager.CreateAsync(new IdentityRole(vaiTro)).GetAwaiter().GetResult();
        }
    }

    private static void TaoQuanTriVien(UserManager<ApplicationUser> userManager)
    {
        const string tenDangNhap = "admin";
        if (userManager.FindByNameAsync(tenDangNhap).GetAwaiter().GetResult() != null) return;

        var admin = new ApplicationUser
        {
            UserName = tenDangNhap,
            Email = "admin@thpt.edu.vn",
            EmailConfirmed = true,
            HoTen = "Quản trị hệ thống"
        };

        var kq = userManager.CreateAsync(admin, "Admin@123").GetAwaiter().GetResult();
        if (kq.Succeeded)
            userManager.AddToRoleAsync(admin, VaiTro.Admin).GetAwaiter().GetResult();
    }

    private static void TaoTaiKhoan(
        UserManager<ApplicationUser> userManager,
        List<GiaoVien> giaoViens,
        List<HocSinh> hocSinhs)
    {
        foreach (var gv in giaoViens)
        {
            var user = new ApplicationUser
            {
                UserName = gv.MaGiaoVien.ToLowerInvariant(),
                Email = gv.Email,
                EmailConfirmed = true,
                HoTen = gv.HoTen,
                GiaoVienId = gv.Id
            };
            var kq = userManager.CreateAsync(user, MatKhauMacDinh).GetAwaiter().GetResult();
            if (kq.Succeeded)
                userManager.AddToRoleAsync(user, VaiTro.GiaoVien).GetAwaiter().GetResult();
        }

        // Tạo sẵn vài tài khoản học sinh để demo; số còn lại admin cấp khi cần.
        foreach (var hs in hocSinhs.Take(3))
        {
            var user = new ApplicationUser
            {
                UserName = hs.MaHocSinh.ToLowerInvariant(),
                EmailConfirmed = true,
                HoTen = hs.HoTen,
                HocSinhId = hs.Id
            };
            var kq = userManager.CreateAsync(user, MatKhauMacDinh).GetAwaiter().GetResult();
            if (kq.Succeeded)
                userManager.AddToRoleAsync(user, VaiTro.HocSinh).GetAwaiter().GetResult();
        }
    }

    /// <summary>Sinh điểm cho cả hai học kỳ, đúng số đầu điểm mà Thông tư 22 quy định cho từng môn.</summary>
    private static void TaoDiemMau(
        QLDiemDbContext db, NamHoc namHoc, List<MonHoc> monHocs, List<HocSinh> hocSinhs, Random rnd)
    {
        foreach (var hs in hocSinhs)
        {
            // Mỗi học sinh có một "mức học lực nền" để phổ điểm trông tự nhiên.
            double nenTang = 5.5 + rnd.NextDouble() * 3.5;

            foreach (var mon in monHocs)
            {
                foreach (var hocKy in new[] { HocKy.HocKyI, HocKy.HocKyII })
                {
                    if (mon.LoaiDanhGia == LoaiDanhGia.NhanXet)
                    {
                        db.DanhGiaNhanXets.Add(new DanhGiaNhanXet
                        {
                            HocSinhId = hs.Id,
                            MonHocId = mon.Id,
                            NamHocId = namHoc.Id,
                            HocKy = hocKy,
                            KetQua = rnd.Next(100) < 95 ? KetQuaNhanXet.Dat : KetQuaNhanXet.ChuaDat,
                            NoiDungNhanXet = "Hoàn thành các yêu cầu của môn học."
                        });
                        continue;
                    }

                    for (int i = 1; i <= mon.SoDauDiemThuongXuyen; i++)
                        db.Diems.Add(TaoDiem(hs, mon, namHoc, hocKy, LoaiDiem.ThuongXuyen, i, nenTang, rnd));

                    db.Diems.Add(TaoDiem(hs, mon, namHoc, hocKy, LoaiDiem.GiuaKy, 1, nenTang, rnd));
                    db.Diems.Add(TaoDiem(hs, mon, namHoc, hocKy, LoaiDiem.CuoiKy, 1, nenTang, rnd));
                }
            }
        }

        db.SaveChanges();
    }

    private static Diem TaoDiem(
        HocSinh hs, MonHoc mon, NamHoc namHoc, HocKy hocKy,
        LoaiDiem loaiDiem, int thuTu, double nenTang, Random rnd)
    {
        double giaTri = Math.Clamp(nenTang + (rnd.NextDouble() - 0.5) * 3.0, 1.0, 10.0);

        return new Diem
        {
            HocSinhId = hs.Id,
            MonHocId = mon.Id,
            NamHocId = namHoc.Id,
            HocKy = hocKy,
            LoaiDiem = loaiDiem,
            ThuTu = thuTu,
            GiaTri = Math.Round((decimal)giaTri, 1, MidpointRounding.AwayFromZero),
            HinhThuc = loaiDiem switch
            {
                LoaiDiem.ThuongXuyen => thuTu % 2 == 0 ? "Viết ngắn" : "Hỏi đáp",
                LoaiDiem.GiuaKy => "Kiểm tra viết giữa kỳ",
                _ => "Kiểm tra viết cuối kỳ"
            }
        };
    }
}
