# Hệ thống quản lý điểm học sinh THPT

Ứng dụng web quản lý học sinh, lớp học, môn học và kết quả học tập cấp THPT,
xây dựng theo **Thông tư 22/2021/TT-BGDĐT** của Bộ Giáo dục và Đào tạo.

## 1. Công nghệ

| Thành phần | Lựa chọn |
|---|---|
| Nền tảng | .NET 10 |
| Giao diện | ASP.NET Core MVC + Razor Views + Bootstrap 5 |
| Truy cập dữ liệu | Entity Framework Core 10 (Code First + Migrations) |
| Cơ sở dữ liệu | SQL Server (LocalDB khi chạy thử) |
| Xác thực, phân quyền | ASP.NET Core Identity (3 vai trò) |
| Kiểm thử | xUnit |

## 2. Kiến trúc

Dự án chia bốn tầng theo mô hình Clean Architecture, phụ thuộc luôn hướng vào trong:

```
QLDiem.Web  ──►  QLDiem.Infrastructure  ──►  QLDiem.Application  ──►  QLDiem.Domain
(MVC, views)     (EF Core, dịch vụ)          (DTO, interface)        (thực thể, quy chế)
```

| Project | Vai trò |
|---|---|
| `src/QLDiem.Domain` | Thực thể, enum và **toàn bộ quy tắc của Thông tư 22** (`QuyChe/QuyCheDanhGia.cs`). Không phụ thuộc thư viện ngoài. |
| `src/QLDiem.Application` | DTO và interface dịch vụ (`IDiemService`, `IKetQuaHocTapService`, `IThongKeService`). |
| `src/QLDiem.Infrastructure` | `DbContext`, cấu hình bảng, migration, dữ liệu mẫu và phần hiện thực các dịch vụ. |
| `src/QLDiem.Web` | Controller, view, xác thực và phân quyền. |
| `tests/QLDiem.Tests` | 39 unit test cho quy chế tính điểm và xếp loại. |

Điểm đáng lưu ý: các hàm tính điểm trong `QuyCheDanhGia` là **hàm thuần** - không chạm
cơ sở dữ liệu - nên kiểm thử được trực tiếp, và mọi nơi trong hệ thống (nhập điểm, học bạ,
thống kê) đều dùng chung một bộ quy tắc đó.

## 3. Chạy dự án

Yêu cầu: .NET 10 SDK và SQL Server LocalDB (đi kèm Visual Studio) hoặc SQL Server.

```bash
git clone <repo> && cd QLDiem
dotnet restore
dotnet run --project src/QLDiem.Web
```

Lần chạy đầu tiên, ứng dụng tự động:

1. Áp dụng migration để tạo cơ sở dữ liệu `QLDiemHocSinh`.
2. Nạp dữ liệu mẫu: 1 năm học, 16 môn học, 16 giáo viên, 4 lớp, 120 học sinh
   và bảng điểm đầy đủ của cả hai học kỳ.

Đổi chuỗi kết nối tại `src/QLDiem.Web/appsettings.json` nếu dùng SQL Server khác LocalDB.

### Tài khoản dùng thử

| Vai trò | Tên đăng nhập | Mật khẩu | Phạm vi |
|---|---|---|---|
| Quản trị viên | `admin` | `Admin@123` | Toàn bộ chức năng |
| Giáo viên | `gv001` … `gv016` | `Abc@123` | Nhập điểm các lớp - môn được phân công |
| Học sinh | `hs0001` … `hs0005` | `Abc@123` | Xem kết quả học tập của chính mình |

### Chạy kiểm thử

```bash
dotnet test
```

## 4. Các đầu điểm theo Thông tư 22

### 4.1. Hai nhóm môn học

| Nhóm | Môn (cấp THPT) | Cách đánh giá |
|---|---|---|
| Nhận xét **kết hợp** điểm số | Toán, Ngữ văn, Ngoại ngữ, Lịch sử, Vật lí, Hóa học, Sinh học, Địa lí, GDKT&PL, Tin học, Công nghệ, GDQP-AN | Có đầu điểm, thang 10 |
| Chỉ **nhận xét** | Giáo dục thể chất, Âm nhạc, Mĩ thuật, HĐTN-HN, Nội dung giáo dục địa phương | Đạt / Chưa đạt |

### 4.2. Đầu điểm trong một học kỳ (môn tính điểm)

| Đầu điểm | Ký hiệu | Số lượng | Hệ số |
|---|---|---|---|
| Đánh giá thường xuyên | ĐĐGtx | 2, 3 hoặc 4 | 1 |
| Đánh giá giữa kỳ | ĐĐGgk | 1 | 2 |
| Đánh giá cuối kỳ | ĐĐGck | 1 | 3 |

Số ĐĐGtx phụ thuộc số tiết của môn trong năm học (Điều 6 khoản 1 điểm b):

| Số tiết / năm | Số ĐĐGtx mỗi học kỳ | Ví dụ |
|---|---|---|
| Tối đa 35 | 2 | Giáo dục quốc phòng và an ninh |
| 36 – 70 | 3 | Lịch sử, Vật lí, Hóa học, Địa lí |
| Trên 70 | 4 | Toán, Ngữ văn, Tiếng Anh |

### 4.3. Công thức

```
ĐTBmhk = (Tổng ĐĐGtx + 2 × ĐĐGgk + 3 × ĐĐGck) / (Số ĐĐGtx + 5)
ĐTBmcn = (ĐTBmhkI + 2 × ĐTBmhkII) / 3
```

Mọi đầu điểm và điểm trung bình đều lấy đến **một chữ số thập phân**.

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

### 4.5. Danh hiệu cuối năm (Điều 15)

- **Học sinh Xuất sắc**: rèn luyện Tốt, học tập Tốt và có ít nhất 6 môn ĐTBmcn từ 9,0.
- **Học sinh Giỏi**: rèn luyện Tốt và học tập Tốt.

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

Các bảng chính:

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

Bảng `Diem` lưu mỗi đầu điểm thành một dòng `(HocSinh, MonHoc, NamHoc, HocKy, LoaiDiem, ThuTu)`
thay vì các cột cố định `diem_tx1, diem_tx2...`. Lý do: số ĐĐGtx thay đổi theo môn, và khi
Bộ điều chỉnh quy định thì chỉ cần sửa quy tắc trong `QuyCheDanhGia`, không phải đổi lược đồ CSDL.
Ràng buộc `UNIQUE` trên tổ hợp sáu cột đó ngăn trùng đầu điểm.

## 7. Cấu trúc thư mục

```
QLDiem/
├── QLDiem.sln
├── docs/
│   └── product-backlog.md        # Product Backlog và kế hoạch Sprint
├── src/
│   ├── QLDiem.Domain/
│   │   ├── Entities/             # NamHoc, Lop, HocSinh, MonHoc, Diem...
│   │   ├── Enums/                # HocKy, LoaiDiem, LoaiDanhGia, MucDanhGia...
│   │   └── QuyChe/               # QuyCheDanhGia.cs - quy tắc Thông tư 22
│   ├── QLDiem.Application/
│   │   ├── DTOs/
│   │   └── Interfaces/
│   ├── QLDiem.Infrastructure/
│   │   ├── Data/                 # DbContext, Migrations, DuLieuMau
│   │   ├── Identity/
│   │   └── Services/             # DiemService, KetQuaHocTapService, ThongKeService
│   └── QLDiem.Web/
│       ├── Controllers/
│       ├── Models/
│       └── Views/
└── tests/
    └── QLDiem.Tests/
```

## 8. Lệnh thường dùng

```bash
dotnet build                                  # Biên dịch toàn bộ
dotnet test                                   # Chạy unit test
dotnet run --project src/QLDiem.Web           # Chạy ứng dụng

# Tạo migration mới sau khi đổi thực thể
dotnet dotnet-ef migrations add <TenMigration> \
  --project src/QLDiem.Infrastructure \
  --startup-project src/QLDiem.Web \
  --output-dir Data/Migrations

# Áp dụng migration
dotnet dotnet-ef database update \
  --project src/QLDiem.Infrastructure \
  --startup-project src/QLDiem.Web
```

## 9. Căn cứ pháp lý

- Thông tư 22/2021/TT-BGDĐT ngày 20/7/2021 quy định về đánh giá học sinh trung học cơ sở
  và trung học phổ thông.
- Thông tư 32/2018/TT-BGDĐT ban hành Chương trình giáo dục phổ thông 2018
  (danh mục môn học và số tiết).
