# Hệ thống quản lý điểm học sinh THPT

Ứng dụng web quản lý học sinh, lớp học, môn học và kết quả học tập cấp THPT,
xây dựng theo **Thông tư 22/2021/TT-BGDĐT** của Bộ Giáo dục và Đào tạo.

## 1. Công nghệ

| Thành phần | Phiên bản | Vai trò |
|---|---|---|
| .NET | 8.0 (LTS) | Nền tảng |
| ASP.NET Core MVC | 8.0 | Controller + Razor Views |
| Entity Framework Core | 8.0.11 | ORM, Code First + Migrations |
| SQL Server LocalDB | — | Cơ sở dữ liệu |
| ASP.NET Core Identity | 8.0.11 | Đăng nhập, phân quyền 3 vai trò |
| Bootstrap | 5.3 | Giao diện |
| xUnit | 2.9 | 39 unit test cho quy chế tính điểm |

## 2. Cấu trúc dự án

Một project ASP.NET Core MVC duy nhất, tổ chức theo thư mục chức năng:

```
QLDiem/
├── QLDiem.slnx
├── QLDiem.App/                    ← project chính
│   ├── Controllers/               (8 controller)
│   ├── Models/
│   │   ├── Enums.cs               HocKy, LoaiDiem, LoaiDanhGia, MucDanhGia, VaiTro...
│   │   ├── ThucThe.cs             9 lớp thực thể ánh xạ xuống bảng CSDL
│   │   ├── ApplicationUser.cs     tài khoản đăng nhập
│   │   ├── NguoiDungClaims.cs     gắn GiaoVienId / HocSinhId vào phiếu đăng nhập
│   │   ├── HienThi.cs             hàm hỗ trợ định dạng cho view
│   │   └── ViewModels/            các lớp truyền dữ liệu giữa controller và view
│   ├── Data/
│   │   ├── QLDiemDbContext.cs     cấu hình bảng, khóa, ràng buộc
│   │   ├── DuLieuMau.cs           dữ liệu mẫu tự nạp lần đầu chạy
│   │   └── Migrations/            lịch sử thay đổi cấu trúc CSDL
│   ├── Services/
│   │   ├── QuyCheDanhGia.cs       ★ toàn bộ quy tắc Thông tư 22
│   │   ├── DiemService.cs         nhập, sửa, tra cứu đầu điểm
│   │   ├── KetQuaHocTapService.cs tổng kết, xếp loại, học bạ
│   │   └── ThongKeService.cs      thống kê, phổ điểm
│   ├── Views/                     29 file Razor
│   ├── wwwroot/                   CSS, Bootstrap, jQuery
│   ├── Program.cs                 cấu hình khởi động
│   └── appsettings.json           chuỗi kết nối
├── QLDiem.Tests/                  39 unit test
└── docs/
    └── product-backlog.md         Product Backlog và kế hoạch Sprint
```

Điểm đáng lưu ý: các hàm trong `QuyCheDanhGia` là **hàm thuần** — không chạm cơ sở dữ liệu,
nhận số vào trả số ra. Nhờ vậy kiểm thử được trực tiếp, và mọi nơi trong hệ thống
(nhập điểm, học bạ, thống kê) đều dùng chung một bộ quy tắc duy nhất.

## 3. Chạy dự án

Yêu cầu: .NET 8 SDK (hoặc mới hơn) và SQL Server LocalDB — cả hai thường đi kèm Visual Studio.

```bash
git clone <repo>
cd QLDiem
dotnet run --project QLDiem.App
```

Lần chạy đầu tiên, ứng dụng tự động:

1. Áp dụng migration để tạo cơ sở dữ liệu `QLDiemHocSinh`
2. Nạp dữ liệu mẫu: 1 năm học, 8 môn, 8 giáo viên, 2 lớp, 30 học sinh, 1.980 đầu điểm

Đổi chuỗi kết nối tại `QLDiem.App/appsettings.json` nếu dùng SQL Server khác LocalDB.

Trong Visual Studio: mở `QLDiem.slnx`, chuột phải **QLDiem** → *Set as Startup Project*, bấm F5.

### Tài khoản dùng thử

| Vai trò | Tên đăng nhập | Mật khẩu | Phạm vi |
|---|---|---|---|
| Quản trị viên | `admin` | `Admin@123` | Toàn bộ chức năng |
| Giáo viên | `gv001` … `gv008` | `Abc@123` | Nhập điểm lớp - môn được phân công |
| Học sinh | `hs001` … `hs003` | `Abc@123` | Xem kết quả học tập của mình |

### Chạy kiểm thử

```bash
dotnet test
```

## 4. Các đầu điểm theo Thông tư 22

### 4.1. Hai nhóm môn học

| Nhóm | Môn trong dự án | Cách đánh giá |
|---|---|---|
| Nhận xét **kết hợp** điểm số | Ngữ văn, Toán, Tiếng Anh, Vật lí, Hóa học, Lịch sử | Có đầu điểm, thang 10 |
| Chỉ **nhận xét** | Giáo dục thể chất, HĐTN-HN | Đạt / Chưa đạt |

### 4.2. Đầu điểm trong một học kỳ (môn tính điểm)

| Đầu điểm | Ký hiệu | Số lượng | Hệ số |
|---|---|---|---|
| Đánh giá thường xuyên | ĐĐGtx | 2, 3 hoặc 4 | 1 |
| Đánh giá giữa kỳ | ĐĐGgk | 1 | 2 |
| Đánh giá cuối kỳ | ĐĐGck | 1 | 3 |

Số ĐĐGtx phụ thuộc số tiết của môn trong năm học (Điều 6 khoản 1 điểm b):

| Số tiết / năm | Số ĐĐGtx mỗi học kỳ | Môn trong dự án |
|---|---|---|
| Tối đa 35 | 2 | — |
| 36 – 70 | 3 | Vật lí, Hóa học (70), Lịch sử (52) |
| Trên 70 | 4 | Ngữ văn, Toán, Tiếng Anh (105) |

### 4.3. Công thức

```
ĐTBmhk = (Tổng ĐĐGtx + 2 × ĐĐGgk + 3 × ĐĐGck) / (Số ĐĐGtx + 5)
ĐTBmcn = (ĐTBmhkI + 2 × ĐTBmhkII) / 3
```

Mọi đầu điểm và điểm trung bình đều lấy đến **một chữ số thập phân**.

Ví dụ kiểm chứng — môn Vật lí, học kỳ I: ĐĐGtx 5,9 – 5,1 – 4,9 | ĐĐGgk 6,2 | ĐĐGck 5,2
→ (15,9 + 12,4 + 15,6) / 8 = 5,4875 → **5,5**

### 4.4. Xếp loại kết quả học tập (Điều 9 khoản 3)

| Mức | Điều kiện |
|---|---|
| Tốt | Mọi môn nhận xét Đạt; mọi ĐTBm từ 6,5 trở lên, trong đó ít nhất 6 môn từ 8,0 |
| Khá | Mọi môn nhận xét Đạt; mọi ĐTBm từ 5,0 trở lên, trong đó ít nhất 6 môn từ 6,5 |
| Đạt | Nhiều nhất 1 môn nhận xét Chưa đạt; ít nhất 6 môn ĐTBm từ 5,0; không môn nào dưới 3,5 |
| Chưa đạt | Các trường hợp còn lại |

> Từ Thông tư 22, hệ thống **không** tính điểm trung bình chung tất cả các môn và
> không còn xếp loại Giỏi / Khá / Trung bình / Yếu / Kém như Thông tư 58 trước đây.
> Cột "Bình quân (tham khảo)" trong bảng tổng kết chỉ dùng để sắp thứ hạng hiển thị.

Vì tiêu chí đòi hỏi **ít nhất 6 môn** đạt ngưỡng, dữ liệu mẫu giữ đúng 6 môn tính điểm —
bớt đi thì mọi học sinh đều rơi vào mức Chưa đạt và chức năng xếp loại mất ý nghĩa khi demo.

### 4.5. Danh hiệu cuối năm (Điều 15)

- **Học sinh Xuất sắc**: rèn luyện Tốt, học tập Tốt và có ít nhất 6 môn ĐTBmcn từ 9,0
- **Học sinh Giỏi**: rèn luyện Tốt và học tập Tốt

## 5. Chức năng theo vai trò

| Chức năng | Quản trị | Giáo viên | Học sinh |
|---|:--:|:--:|:--:|
| Quản lý năm học, lớp, môn học, phân công giảng dạy | ✔ | | |
| Quản lý hồ sơ giáo viên | ✔ | | |
| Quản lý hồ sơ học sinh | ✔ | Xem, sửa | |
| Nhập, sửa điểm | Mọi lớp | Lớp - môn được phân công | |
| Tra cứu điểm và kết quả học tập | ✔ | ✔ | Của mình |
| Thống kê theo lớp, phổ điểm theo môn | ✔ | ✔ | |
| Chốt tổng kết học kỳ | ✔ | | |

## 6. Cơ sở dữ liệu

| Bảng | Nội dung |
|---|---|
| `NamHoc` | Năm học, đánh dấu năm đang hoạt động |
| `Lop` | Lớp học, khối, giáo viên chủ nhiệm |
| `HocSinh` | Hồ sơ học sinh, thuộc một lớp |
| `GiaoVien` | Hồ sơ giáo viên |
| `MonHoc` | Môn học, số tiết/năm, hình thức đánh giá |
| `PhanCongGiangDay` | Giáo viên nào dạy môn nào ở lớp nào |
| `Diem` | **Từng đầu điểm**, lưu theo dòng |
| `DanhGiaNhanXet` | Kết quả Đạt / Chưa đạt của môn chỉ nhận xét |
| `KetQuaHocKy` | Bản ghi tổng kết học kỳ, cả năm |
| `TaiKhoan`, `VaiTro`, … | Bảng của ASP.NET Core Identity |

Bảng `Diem` lưu mỗi đầu điểm thành một dòng `(HocSinh, MonHoc, NamHoc, HocKy, LoaiDiem, ThuTu)`
thay vì các cột cố định `diem_tx1, diem_tx2...`. Lý do: số ĐĐGtx thay đổi theo môn, và khi
Bộ điều chỉnh quy định thì chỉ cần sửa `QuyCheDanhGia`, không phải đổi lược đồ CSDL.
Ràng buộc `UNIQUE` trên tổ hợp sáu cột đó ngăn trùng đầu điểm.

## 7. Một vài lựa chọn kỹ thuật

**Không dùng async/await.** Toàn bộ thao tác cơ sở dữ liệu viết đồng bộ (`ToList`, `SaveChanges`)
cho dễ đọc. Ngoại lệ duy nhất là `AccountController`: ASP.NET Core Identity chỉ cung cấp
API bất đồng bộ để kiểm tra mật khẩu và tạo phiếu đăng nhập.

**GiaoVienId và HocSinhId lưu trong phiếu đăng nhập.** Xem `NguoiDungClaims.cs`.
Nhờ vậy controller biết ngay người đang đăng nhập là giáo viên nào mà không phải truy vấn lại.

**Culture bất biến khi nhận dữ liệu.** Điểm luôn nhập bằng dấu chấm (`8.5`). Nếu để culture
tiếng Việt, dấu chấm bị hiểu là ký tự phân nhóm hàng nghìn và `8.5` thành `85`.
Phần hiển thị dấu phẩy thập phân xử lý riêng trong `HienThi.Diem`.

## 8. Lệnh thường dùng

```bash
dotnet build                              # Biên dịch
dotnet test                               # Chạy 39 unit test
dotnet run --project QLDiem.App           # Chạy ứng dụng

# Tạo migration sau khi sửa thực thể
dotnet dotnet-ef migrations add <TenMigration> --project QLDiem.App --output-dir Data/Migrations

# Áp dụng migration
dotnet dotnet-ef database update --project QLDiem.App
```

## 9. Căn cứ pháp lý

- Thông tư 22/2021/TT-BGDĐT ngày 20/7/2021 quy định về đánh giá học sinh trung học cơ sở
  và trung học phổ thông
- Thông tư 32/2018/TT-BGDĐT ban hành Chương trình giáo dục phổ thông 2018
  (danh mục môn học và số tiết)
