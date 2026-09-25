using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QLDiem.Data.Migrations
{
    /// <inheritdoc />
    public partial class KhoiTao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GiaoVien",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaGiaoVien = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    HoTen = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    GioiTinh = table.Column<int>(type: "int", nullable: false),
                    NgaySinh = table.Column<DateOnly>(type: "date", nullable: true),
                    ChuyenMon = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DienThoai = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    DangCongTac = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GiaoVien", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MonHoc",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaMon = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TenMon = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SoTietNam = table.Column<int>(type: "int", nullable: false),
                    LoaiDanhGia = table.Column<int>(type: "int", nullable: false),
                    BatBuoc = table.Column<bool>(type: "bit", nullable: false),
                    ThuTuHienThi = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MonHoc", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NamHoc",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ten = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    NgayBatDau = table.Column<DateOnly>(type: "date", nullable: false),
                    NgayKetThuc = table.Column<DateOnly>(type: "date", nullable: false),
                    DangHoatDong = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NamHoc", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TaiKhoan",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    HoTen = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GiaoVienId = table.Column<int>(type: "int", nullable: true),
                    HocSinhId = table.Column<int>(type: "int", nullable: true),
                    DangHoatDong = table.Column<bool>(type: "bit", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaiKhoan", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VaiTro",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VaiTro", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Lop",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ten = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Khoi = table.Column<int>(type: "int", nullable: false),
                    NamHocId = table.Column<int>(type: "int", nullable: false),
                    GiaoVienChuNhiemId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lop", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Lop_GiaoVien_GiaoVienChuNhiemId",
                        column: x => x.GiaoVienChuNhiemId,
                        principalTable: "GiaoVien",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Lop_NamHoc_NamHocId",
                        column: x => x.NamHocId,
                        principalTable: "NamHoc",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TaiKhoan_Claim",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaiKhoan_Claim", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaiKhoan_Claim_TaiKhoan_UserId",
                        column: x => x.UserId,
                        principalTable: "TaiKhoan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TaiKhoan_Login",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaiKhoan_Login", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_TaiKhoan_Login_TaiKhoan_UserId",
                        column: x => x.UserId,
                        principalTable: "TaiKhoan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TaiKhoan_Token",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaiKhoan_Token", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_TaiKhoan_Token_TaiKhoan_UserId",
                        column: x => x.UserId,
                        principalTable: "TaiKhoan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TaiKhoan_VaiTro",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaiKhoan_VaiTro", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_TaiKhoan_VaiTro_TaiKhoan_UserId",
                        column: x => x.UserId,
                        principalTable: "TaiKhoan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TaiKhoan_VaiTro_VaiTro_RoleId",
                        column: x => x.RoleId,
                        principalTable: "VaiTro",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VaiTro_Claim",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VaiTro_Claim", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VaiTro_Claim_VaiTro_RoleId",
                        column: x => x.RoleId,
                        principalTable: "VaiTro",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HocSinh",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaHocSinh = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    HoTen = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NgaySinh = table.Column<DateOnly>(type: "date", nullable: false),
                    GioiTinh = table.Column<int>(type: "int", nullable: false),
                    NoiSinh = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DiaChi = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    DienThoai = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    HoTenPhuHuynh = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DienThoaiPhuHuynh = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    DangHoc = table.Column<bool>(type: "bit", nullable: false),
                    LopId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HocSinh", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HocSinh_Lop_LopId",
                        column: x => x.LopId,
                        principalTable: "Lop",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PhanCongGiangDay",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GiaoVienId = table.Column<int>(type: "int", nullable: false),
                    LopId = table.Column<int>(type: "int", nullable: false),
                    MonHocId = table.Column<int>(type: "int", nullable: false),
                    NamHocId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhanCongGiangDay", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PhanCongGiangDay_GiaoVien_GiaoVienId",
                        column: x => x.GiaoVienId,
                        principalTable: "GiaoVien",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PhanCongGiangDay_Lop_LopId",
                        column: x => x.LopId,
                        principalTable: "Lop",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PhanCongGiangDay_MonHoc_MonHocId",
                        column: x => x.MonHocId,
                        principalTable: "MonHoc",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PhanCongGiangDay_NamHoc_NamHocId",
                        column: x => x.NamHocId,
                        principalTable: "NamHoc",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DanhGiaNhanXet",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HocSinhId = table.Column<int>(type: "int", nullable: false),
                    MonHocId = table.Column<int>(type: "int", nullable: false),
                    NamHocId = table.Column<int>(type: "int", nullable: false),
                    HocKy = table.Column<int>(type: "int", nullable: false),
                    KetQua = table.Column<int>(type: "int", nullable: false),
                    NoiDungNhanXet = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    NgayNhap = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DanhGiaNhanXet", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DanhGiaNhanXet_HocSinh_HocSinhId",
                        column: x => x.HocSinhId,
                        principalTable: "HocSinh",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DanhGiaNhanXet_MonHoc_MonHocId",
                        column: x => x.MonHocId,
                        principalTable: "MonHoc",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DanhGiaNhanXet_NamHoc_NamHocId",
                        column: x => x.NamHocId,
                        principalTable: "NamHoc",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Diem",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HocSinhId = table.Column<int>(type: "int", nullable: false),
                    MonHocId = table.Column<int>(type: "int", nullable: false),
                    NamHocId = table.Column<int>(type: "int", nullable: false),
                    HocKy = table.Column<int>(type: "int", nullable: false),
                    LoaiDiem = table.Column<int>(type: "int", nullable: false),
                    ThuTu = table.Column<int>(type: "int", nullable: false),
                    GiaTri = table.Column<decimal>(type: "decimal(4,1)", precision: 4, scale: 1, nullable: false),
                    HinhThuc = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    NgayNhap = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NgayCapNhat = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Diem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Diem_HocSinh_HocSinhId",
                        column: x => x.HocSinhId,
                        principalTable: "HocSinh",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Diem_MonHoc_MonHocId",
                        column: x => x.MonHocId,
                        principalTable: "MonHoc",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Diem_NamHoc_NamHocId",
                        column: x => x.NamHocId,
                        principalTable: "NamHoc",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "KetQuaHocKy",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HocSinhId = table.Column<int>(type: "int", nullable: false),
                    NamHocId = table.Column<int>(type: "int", nullable: false),
                    HocKy = table.Column<int>(type: "int", nullable: false),
                    XepLoaiHocTap = table.Column<int>(type: "int", nullable: false),
                    XepLoaiRenLuyen = table.Column<int>(type: "int", nullable: false),
                    DanhHieu = table.Column<int>(type: "int", nullable: false),
                    SoMonTinhDiem = table.Column<int>(type: "int", nullable: false),
                    NhanXetCuaGVCN = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    NgayTongKet = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KetQuaHocKy", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KetQuaHocKy_HocSinh_HocSinhId",
                        column: x => x.HocSinhId,
                        principalTable: "HocSinh",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_KetQuaHocKy_NamHoc_NamHocId",
                        column: x => x.NamHocId,
                        principalTable: "NamHoc",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DanhGiaNhanXet_MonHocId",
                table: "DanhGiaNhanXet",
                column: "MonHocId");

            migrationBuilder.CreateIndex(
                name: "IX_DanhGiaNhanXet_NamHocId",
                table: "DanhGiaNhanXet",
                column: "NamHocId");

            migrationBuilder.CreateIndex(
                name: "UX_DanhGiaNhanXet",
                table: "DanhGiaNhanXet",
                columns: new[] { "HocSinhId", "MonHocId", "NamHocId", "HocKy" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Diem_MonHocId",
                table: "Diem",
                column: "MonHocId");

            migrationBuilder.CreateIndex(
                name: "IX_Diem_NamHocId",
                table: "Diem",
                column: "NamHocId");

            migrationBuilder.CreateIndex(
                name: "UX_Diem_DauDiem",
                table: "Diem",
                columns: new[] { "HocSinhId", "MonHocId", "NamHocId", "HocKy", "LoaiDiem", "ThuTu" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GiaoVien_MaGiaoVien",
                table: "GiaoVien",
                column: "MaGiaoVien",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HocSinh_LopId",
                table: "HocSinh",
                column: "LopId");

            migrationBuilder.CreateIndex(
                name: "IX_HocSinh_MaHocSinh",
                table: "HocSinh",
                column: "MaHocSinh",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KetQuaHocKy_NamHocId",
                table: "KetQuaHocKy",
                column: "NamHocId");

            migrationBuilder.CreateIndex(
                name: "UX_KetQuaHocKy",
                table: "KetQuaHocKy",
                columns: new[] { "HocSinhId", "NamHocId", "HocKy" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Lop_GiaoVienChuNhiemId",
                table: "Lop",
                column: "GiaoVienChuNhiemId");

            migrationBuilder.CreateIndex(
                name: "IX_Lop_NamHocId",
                table: "Lop",
                column: "NamHocId");

            migrationBuilder.CreateIndex(
                name: "IX_Lop_Ten_NamHocId",
                table: "Lop",
                columns: new[] { "Ten", "NamHocId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MonHoc_MaMon",
                table: "MonHoc",
                column: "MaMon",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NamHoc_Ten",
                table: "NamHoc",
                column: "Ten",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PhanCongGiangDay_GiaoVienId",
                table: "PhanCongGiangDay",
                column: "GiaoVienId");

            migrationBuilder.CreateIndex(
                name: "IX_PhanCongGiangDay_LopId_MonHocId_NamHocId",
                table: "PhanCongGiangDay",
                columns: new[] { "LopId", "MonHocId", "NamHocId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PhanCongGiangDay_MonHocId",
                table: "PhanCongGiangDay",
                column: "MonHocId");

            migrationBuilder.CreateIndex(
                name: "IX_PhanCongGiangDay_NamHocId",
                table: "PhanCongGiangDay",
                column: "NamHocId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "TaiKhoan",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "TaiKhoan",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_TaiKhoan_Claim_UserId",
                table: "TaiKhoan_Claim",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_TaiKhoan_Login_UserId",
                table: "TaiKhoan_Login",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_TaiKhoan_VaiTro_RoleId",
                table: "TaiKhoan_VaiTro",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "VaiTro",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_VaiTro_Claim_RoleId",
                table: "VaiTro_Claim",
                column: "RoleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DanhGiaNhanXet");

            migrationBuilder.DropTable(
                name: "Diem");

            migrationBuilder.DropTable(
                name: "KetQuaHocKy");

            migrationBuilder.DropTable(
                name: "PhanCongGiangDay");

            migrationBuilder.DropTable(
                name: "TaiKhoan_Claim");

            migrationBuilder.DropTable(
                name: "TaiKhoan_Login");

            migrationBuilder.DropTable(
                name: "TaiKhoan_Token");

            migrationBuilder.DropTable(
                name: "TaiKhoan_VaiTro");

            migrationBuilder.DropTable(
                name: "VaiTro_Claim");

            migrationBuilder.DropTable(
                name: "HocSinh");

            migrationBuilder.DropTable(
                name: "MonHoc");

            migrationBuilder.DropTable(
                name: "TaiKhoan");

            migrationBuilder.DropTable(
                name: "VaiTro");

            migrationBuilder.DropTable(
                name: "Lop");

            migrationBuilder.DropTable(
                name: "GiaoVien");

            migrationBuilder.DropTable(
                name: "NamHoc");
        }
    }
}
