using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using QLDiem.Models;

namespace QLDiem.Data;

public class QLDiemDbContext : IdentityDbContext<ApplicationUser>
{
    public QLDiemDbContext(DbContextOptions<QLDiemDbContext> options) : base(options) { }

    public DbSet<NamHoc> NamHocs => Set<NamHoc>();
    public DbSet<Lop> Lops => Set<Lop>();
    public DbSet<HocSinh> HocSinhs => Set<HocSinh>();
    public DbSet<GiaoVien> GiaoViens => Set<GiaoVien>();
    public DbSet<MonHoc> MonHocs => Set<MonHoc>();
    public DbSet<PhanCongGiangDay> PhanCongGiangDays => Set<PhanCongGiangDay>();
    public DbSet<Diem> Diems => Set<Diem>();
    public DbSet<DanhGiaNhanXet> DanhGiaNhanXets => Set<DanhGiaNhanXet>();
    public DbSet<KetQuaHocKy> KetQuaHocKys => Set<KetQuaHocKy>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);

        // Đổi tên bảng Identity sang tiếng Việt cho dễ đọc khi xem trong SSMS.
        b.Entity<ApplicationUser>().ToTable("TaiKhoan");
        b.Entity<IdentityRole>().ToTable("VaiTro");
        b.Entity<IdentityUserRole<string>>().ToTable("TaiKhoan_VaiTro");
        b.Entity<IdentityUserClaim<string>>().ToTable("TaiKhoan_Claim");
        b.Entity<IdentityUserLogin<string>>().ToTable("TaiKhoan_Login");
        b.Entity<IdentityUserToken<string>>().ToTable("TaiKhoan_Token");
        b.Entity<IdentityRoleClaim<string>>().ToTable("VaiTro_Claim");

        b.Entity<NamHoc>(e =>
        {
            e.ToTable("NamHoc");
            e.Property(x => x.Ten).HasMaxLength(20).IsRequired();
            e.HasIndex(x => x.Ten).IsUnique();
        });

        b.Entity<GiaoVien>(e =>
        {
            e.ToTable("GiaoVien");
            e.Property(x => x.MaGiaoVien).HasMaxLength(20).IsRequired();
            e.Property(x => x.HoTen).HasMaxLength(100).IsRequired();
            e.Property(x => x.ChuyenMon).HasMaxLength(100);
            e.Property(x => x.Email).HasMaxLength(100);
            e.Property(x => x.DienThoai).HasMaxLength(20);
            e.HasIndex(x => x.MaGiaoVien).IsUnique();
        });

        b.Entity<Lop>(e =>
        {
            e.ToTable("Lop");
            e.Property(x => x.Ten).HasMaxLength(20).IsRequired();
            e.HasIndex(x => new { x.Ten, x.NamHocId }).IsUnique();
            e.HasOne(x => x.NamHoc).WithMany(x => x.DanhSachLop)
                .HasForeignKey(x => x.NamHocId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.GiaoVienChuNhiem).WithMany(x => x.LopChuNhiem)
                .HasForeignKey(x => x.GiaoVienChuNhiemId).OnDelete(DeleteBehavior.SetNull);
        });

        b.Entity<HocSinh>(e =>
        {
            e.ToTable("HocSinh");
            e.Property(x => x.MaHocSinh).HasMaxLength(20).IsRequired();
            e.Property(x => x.HoTen).HasMaxLength(100).IsRequired();
            e.Property(x => x.NoiSinh).HasMaxLength(100);
            e.Property(x => x.DiaChi).HasMaxLength(200);
            e.Property(x => x.DienThoai).HasMaxLength(20);
            e.Property(x => x.Email).HasMaxLength(100);
            e.Property(x => x.HoTenPhuHuynh).HasMaxLength(100);
            e.Property(x => x.DienThoaiPhuHuynh).HasMaxLength(20);
            e.HasIndex(x => x.MaHocSinh).IsUnique();
            e.HasOne(x => x.Lop).WithMany(x => x.HocSinhs)
                .HasForeignKey(x => x.LopId).OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<MonHoc>(e =>
        {
            e.ToTable("MonHoc");
            e.Property(x => x.MaMon).HasMaxLength(20).IsRequired();
            e.Property(x => x.TenMon).HasMaxLength(100).IsRequired();
            e.HasIndex(x => x.MaMon).IsUnique();
            e.Ignore(x => x.SoDauDiemThuongXuyen); // thuộc tính tính toán, không lưu xuống CSDL
        });

        b.Entity<PhanCongGiangDay>(e =>
        {
            e.ToTable("PhanCongGiangDay");
            e.HasIndex(x => new { x.LopId, x.MonHocId, x.NamHocId }).IsUnique();
            e.HasOne(x => x.GiaoVien).WithMany(x => x.PhanCongs)
                .HasForeignKey(x => x.GiaoVienId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Lop).WithMany(x => x.PhanCongs)
                .HasForeignKey(x => x.LopId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.MonHoc).WithMany(x => x.PhanCongs)
                .HasForeignKey(x => x.MonHocId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.NamHoc).WithMany()
                .HasForeignKey(x => x.NamHocId).OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<Diem>(e =>
        {
            e.ToTable("Diem");
            e.Property(x => x.GiaTri).HasPrecision(4, 1);
            e.Property(x => x.HinhThuc).HasMaxLength(100);

            // Mỗi đầu điểm là duy nhất theo học sinh - môn - năm học - học kỳ - loại - thứ tự.
            e.HasIndex(x => new { x.HocSinhId, x.MonHocId, x.NamHocId, x.HocKy, x.LoaiDiem, x.ThuTu })
                .IsUnique()
                .HasDatabaseName("UX_Diem_DauDiem");

            e.HasOne(x => x.HocSinh).WithMany(x => x.DanhSachDiem)
                .HasForeignKey(x => x.HocSinhId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.MonHoc).WithMany()
                .HasForeignKey(x => x.MonHocId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.NamHoc).WithMany()
                .HasForeignKey(x => x.NamHocId).OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<DanhGiaNhanXet>(e =>
        {
            e.ToTable("DanhGiaNhanXet");
            e.Property(x => x.NoiDungNhanXet).HasMaxLength(500);
            e.HasIndex(x => new { x.HocSinhId, x.MonHocId, x.NamHocId, x.HocKy })
                .IsUnique()
                .HasDatabaseName("UX_DanhGiaNhanXet");
            e.HasOne(x => x.HocSinh).WithMany(x => x.DanhSachNhanXet)
                .HasForeignKey(x => x.HocSinhId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.MonHoc).WithMany()
                .HasForeignKey(x => x.MonHocId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.NamHoc).WithMany()
                .HasForeignKey(x => x.NamHocId).OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<KetQuaHocKy>(e =>
        {
            e.ToTable("KetQuaHocKy");
            e.Property(x => x.NhanXetCuaGVCN).HasMaxLength(500);
            e.HasIndex(x => new { x.HocSinhId, x.NamHocId, x.HocKy })
                .IsUnique()
                .HasDatabaseName("UX_KetQuaHocKy");
            e.HasOne(x => x.HocSinh).WithMany()
                .HasForeignKey(x => x.HocSinhId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.NamHoc).WithMany()
                .HasForeignKey(x => x.NamHocId).OnDelete(DeleteBehavior.Restrict);
        });
    }
}
