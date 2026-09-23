using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QLDiem.Domain.Common;
using QLDiem.Domain.Entities;
using QLDiem.Domain.Enums;
using QLDiem.Infrastructure.Identity;

namespace QLDiem.Infrastructure.Data;

/// <summary>
/// Tạo dữ liệu mẫu để chạy thử và demo: vai trò, tài khoản, năm học,
/// môn học theo Chương trình GDPT 2018, lớp, học sinh và bảng điểm hai học kỳ.
/// Chỉ chạy khi cơ sở dữ liệu còn trống nên có thể gọi mỗi lần khởi động.
/// </summary>
public static class DuLieuMau
{
    public const string MatKhauMacDinh = "Abc@123";

    private static readonly string[] HoDem =
    [
        "Nguyễn Văn", "Trần Thị", "Lê Hoàng", "Phạm Minh", "Hoàng Thu", "Vũ Đức",
        "Đặng Ngọc", "Bùi Quang", "Đỗ Thị", "Ngô Gia", "Dương Khánh", "Lý Thanh",
        "Phan Anh", "Võ Hải", "Đinh Bảo", "Trương Mỹ", "Mai Tuấn", "Chu Diệu"
    ];

    private static readonly string[] Ten =
    [
        "An", "Bình", "Chi", "Dũng", "Duyên", "Giang", "Hà", "Hải", "Hiếu", "Hoa",
        "Huy", "Khoa", "Lan", "Linh", "Long", "Mai", "Minh", "Nam", "Ngân", "Nhung",
        "Phúc", "Quân", "Quỳnh", "Sơn", "Thảo", "Thắng", "Trang", "Tú", "Vy", "Yến"
    ];

    private record MonMau(string Ma, string Ten, int SoTiet, LoaiDanhGia Loai, bool BatBuoc);

    private static readonly MonMau[] DanhSachMon =
    [
        new("NV",     "Ngữ văn",                              105, LoaiDanhGia.DiemSo,  true),
        new("TOAN",   "Toán",                                 105, LoaiDanhGia.DiemSo,  true),
        new("ANH",    "Tiếng Anh",                            105, LoaiDanhGia.DiemSo,  true),
        new("SU",     "Lịch sử",                               52, LoaiDanhGia.DiemSo,  true),
        new("LY",     "Vật lí",                                70, LoaiDanhGia.DiemSo,  false),
        new("HOA",    "Hóa học",                               70, LoaiDanhGia.DiemSo,  false),
        new("SINH",   "Sinh học",                              70, LoaiDanhGia.DiemSo,  false),
        new("DIA",    "Địa lí",                                70, LoaiDanhGia.DiemSo,  false),
        new("GDKTPL", "Giáo dục kinh tế và pháp luật",         70, LoaiDanhGia.DiemSo,  false),
        new("TIN",    "Tin học",                               70, LoaiDanhGia.DiemSo,  false),
        new("CN",     "Công nghệ",                             70, LoaiDanhGia.DiemSo,  false),
        new("GDQP",   "Giáo dục quốc phòng và an ninh",        35, LoaiDanhGia.DiemSo,  true),
        new("GDTC",   "Giáo dục thể chất",                     70, LoaiDanhGia.NhanXet, true),
        new("NHAC",   "Âm nhạc",                               70, LoaiDanhGia.NhanXet, false),
        new("HDTN",   "Hoạt động trải nghiệm, hướng nghiệp",  105, LoaiDanhGia.NhanXet, true),
        new("GDDP",   "Nội dung giáo dục của địa phương",      35, LoaiDanhGia.NhanXet, true)
    ];

    public static async Task KhoiTaoAsync(
        QLDiemDbContext db,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        await db.Database.MigrateAsync();

        await TaoVaiTroAsync(roleManager);
        await TaoQuanTriVienAsync(userManager);

        if (await db.NamHocs.AnyAsync()) return; // đã có dữ liệu, không tạo lại

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

        await db.SaveChangesAsync();

        var lops = new List<Lop>
        {
            new() { Ten = "10A1", Khoi = 10, NamHocId = namHoc.Id, GiaoVienChuNhiemId = giaoViens[0].Id },
            new() { Ten = "10A2", Khoi = 10, NamHocId = namHoc.Id, GiaoVienChuNhiemId = giaoViens[1].Id },
            new() { Ten = "11A1", Khoi = 11, NamHocId = namHoc.Id, GiaoVienChuNhiemId = giaoViens[2].Id },
            new() { Ten = "12A1", Khoi = 12, NamHocId = namHoc.Id, GiaoVienChuNhiemId = giaoViens[3].Id }
        };
        db.Lops.AddRange(lops);
        await db.SaveChangesAsync();

        // Mỗi lớp học toàn bộ môn trong danh sách; mỗi môn do một giáo viên phụ trách.
        var phanCongs = new List<PhanCongGiangDay>();
        foreach (var lop in lops)
        {
            for (int i = 0; i < monHocs.Count; i++)
            {
                phanCongs.Add(new PhanCongGiangDay
                {
                    LopId = lop.Id,
                    MonHocId = monHocs[i].Id,
                    GiaoVienId = giaoViens[i].Id,
                    NamHocId = namHoc.Id
                });
            }
        }
        db.PhanCongGiangDays.AddRange(phanCongs);

        var hocSinhs = new List<HocSinh>();
        int stt = 1;
        foreach (var lop in lops)
        {
            for (int i = 0; i < 30; i++)
            {
                hocSinhs.Add(new HocSinh
                {
                    MaHocSinh = $"HS{stt:D4}",
                    HoTen = TaoHoTen(rnd),
                    NgaySinh = new DateOnly(2026 - lop.Khoi + 4 - 12, rnd.Next(1, 13), rnd.Next(1, 28)),
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
        await db.SaveChangesAsync();

        await TaoDiemMauAsync(db, namHoc, monHocs, hocSinhs, rnd);
        await TaoTaiKhoanAsync(userManager, giaoViens, hocSinhs);
    }

    private static string TaoHoTen(Random rnd) =>
        $"{HoDem[rnd.Next(HoDem.Length)]} {Ten[rnd.Next(Ten.Length)]}";

    private static async Task TaoVaiTroAsync(RoleManager<IdentityRole> roleManager)
    {
        foreach (var vaiTro in VaiTro.TatCa)
        {
            if (!await roleManager.RoleExistsAsync(vaiTro))
                await roleManager.CreateAsync(new IdentityRole(vaiTro));
        }
    }

    private static async Task TaoQuanTriVienAsync(UserManager<ApplicationUser> userManager)
    {
        const string tenDangNhap = "admin";
        if (await userManager.FindByNameAsync(tenDangNhap) is not null) return;

        var admin = new ApplicationUser
        {
            UserName = tenDangNhap,
            Email = "admin@thpt.edu.vn",
            EmailConfirmed = true,
            HoTen = "Quản trị hệ thống"
        };

        var kq = await userManager.CreateAsync(admin, "Admin@123");
        if (kq.Succeeded) await userManager.AddToRoleAsync(admin, VaiTro.Admin);
    }

    private static async Task TaoTaiKhoanAsync(
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
            var kq = await userManager.CreateAsync(user, MatKhauMacDinh);
            if (kq.Succeeded) await userManager.AddToRoleAsync(user, VaiTro.GiaoVien);
        }

        // Tạo sẵn vài tài khoản học sinh để demo; số còn lại admin cấp sau khi cần.
        foreach (var hs in hocSinhs.Take(5))
        {
            var user = new ApplicationUser
            {
                UserName = hs.MaHocSinh.ToLowerInvariant(),
                EmailConfirmed = true,
                HoTen = hs.HoTen,
                HocSinhId = hs.Id
            };
            var kq = await userManager.CreateAsync(user, MatKhauMacDinh);
            if (kq.Succeeded) await userManager.AddToRoleAsync(user, VaiTro.HocSinh);
        }
    }

    /// <summary>Sinh điểm cho cả hai học kỳ, đúng số đầu điểm mà Thông tư 22 quy định cho từng môn.</summary>
    private static async Task TaoDiemMauAsync(
        QLDiemDbContext db, NamHoc namHoc, List<MonHoc> monHocs, List<HocSinh> hocSinhs, Random rnd)
    {
        var diems = new List<Diem>();
        var nhanXets = new List<DanhGiaNhanXet>();

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
                        nhanXets.Add(new DanhGiaNhanXet
                        {
                            HocSinhId = hs.Id,
                            MonHocId = mon.Id,
                            NamHocId = namHoc.Id,
                            HocKy = hocKy,
                            KetQua = rnd.Next(100) < 97 ? KetQuaNhanXet.Dat : KetQuaNhanXet.ChuaDat,
                            NoiDungNhanXet = "Hoàn thành các yêu cầu của môn học."
                        });
                        continue;
                    }

                    for (int i = 1; i <= mon.SoDauDiemThuongXuyen; i++)
                        diems.Add(TaoDiem(hs, mon, namHoc, hocKy, LoaiDiem.ThuongXuyen, i, nenTang, rnd));

                    diems.Add(TaoDiem(hs, mon, namHoc, hocKy, LoaiDiem.GiuaKy, 1, nenTang, rnd));
                    diems.Add(TaoDiem(hs, mon, namHoc, hocKy, LoaiDiem.CuoiKy, 1, nenTang, rnd));
                }
            }
        }

        db.ChangeTracker.AutoDetectChangesEnabled = false;
        db.Diems.AddRange(diems);
        db.DanhGiaNhanXets.AddRange(nhanXets);
        await db.SaveChangesAsync();
        db.ChangeTracker.AutoDetectChangesEnabled = true;
    }

    private static Diem TaoDiem(
        HocSinh hs, MonHoc mon, NamHoc namHoc, HocKy hocKy,
        LoaiDiem loaiDiem, int thuTu, double nenTang, Random rnd)
    {
        double giaTri = nenTang + (rnd.NextDouble() - 0.5) * 3.0;
        giaTri = Math.Clamp(giaTri, 1.0, 10.0);

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
