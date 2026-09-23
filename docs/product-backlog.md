# Product Backlog và kế hoạch Sprint

Dự án: **Xây dựng hệ thống quản lý điểm học sinh trường cấp 3**
Phương pháp: Agile – Scrum, Sprint dài 2 tuần.

## 1. Vai trò trong nhóm

| Vai trò | Trách nhiệm |
|---|---|
| Product Owner | Giữ Product Backlog, xác định thứ tự ưu tiên, nghiệm thu từng Sprint |
| Scrum Master | Điều phối các buổi họp, gỡ vướng mắc cho nhóm |
| Development Team | Phân tích, thiết kế, lập trình, kiểm thử |

Các hoạt động định kỳ: Sprint Planning đầu mỗi Sprint, Daily Scrum 15 phút,
Sprint Review và Sprint Retrospective vào cuối Sprint.

## 2. Product Backlog

Điểm số dùng thang Fibonacci (1, 2, 3, 5, 8, 13).

| # | User Story | Ưu tiên | Điểm | Sprint |
|---|---|:---:|:---:|:---:|
| US-01 | Là quản trị viên, tôi muốn đăng nhập và phân quyền theo vai trò để mỗi người chỉ thấy chức năng của mình | Cao | 5 | 1 |
| US-02 | Là quản trị viên, tôi muốn quản lý năm học và đánh dấu năm học đang hoạt động | Cao | 3 | 1 |
| US-03 | Là quản trị viên, tôi muốn quản lý danh mục môn học kèm số tiết/năm để hệ thống tự suy ra số đầu điểm | Cao | 5 | 1 |
| US-04 | Là quản trị viên, tôi muốn thêm, sửa, xóa và tìm kiếm hồ sơ học sinh | Cao | 5 | 1 |
| US-05 | Là quản trị viên, tôi muốn quản lý hồ sơ giáo viên | Trung bình | 3 | 1 |
| US-06 | Là quản trị viên, tôi muốn quản lý lớp học và gán giáo viên chủ nhiệm | Cao | 3 | 2 |
| US-07 | Là quản trị viên, tôi muốn phân công giáo viên dạy từng môn ở từng lớp | Cao | 5 | 2 |
| US-08 | Là giáo viên, tôi muốn nhập điểm cho cả lớp trên một bảng theo đúng số đầu điểm của môn | Cao | 13 | 2 |
| US-09 | Là giáo viên, tôi muốn hệ thống tự tính ĐTBmhk theo công thức của Thông tư 22 | Cao | 8 | 2 |
| US-10 | Là giáo viên, tôi muốn nhập kết quả Đạt / Chưa đạt cho các môn chỉ đánh giá bằng nhận xét | Cao | 5 | 3 |
| US-11 | Là giáo viên, tôi muốn hệ thống tính ĐTBmcn với học kỳ II hệ số 2 | Cao | 3 | 3 |
| US-12 | Là giáo viên, tôi muốn xem bảng tổng kết và xếp loại học tập của lớp mình | Cao | 8 | 3 |
| US-13 | Là học sinh, tôi muốn xem kết quả học tập của mình theo từng học kỳ và cả năm | Cao | 5 | 3 |
| US-14 | Là học sinh, tôi muốn xem chi tiết từng đầu điểm để biết điểm nào kéo kết quả xuống | Trung bình | 3 | 3 |
| US-15 | Là ban giám hiệu, tôi muốn xem thống kê xếp loại theo lớp và theo khối | Cao | 8 | 4 |
| US-16 | Là ban giám hiệu, tôi muốn xem phổ điểm từng môn để đánh giá chất lượng giảng dạy | Trung bình | 5 | 4 |
| US-17 | Là quản trị viên, tôi muốn chốt tổng kết học kỳ và lưu lại kết quả | Trung bình | 5 | 4 |
| US-18 | Là giáo viên, tôi muốn in bảng tổng kết lớp và kết quả học tập của học sinh | Thấp | 3 | 4 |
| US-19 | Là người dùng, tôi muốn đổi mật khẩu tài khoản của mình | Thấp | 2 | 4 |

Tổng: 19 user story, 92 điểm.

## 3. Kế hoạch Sprint

### Sprint 1 – Nền tảng và danh mục (21 điểm)

Mục tiêu: dựng được khung dự án, đăng nhập phân quyền và các danh mục cơ bản.

- Thiết lập giải pháp bốn tầng, EF Core Code First, migration đầu tiên.
- US-01, US-02, US-03, US-04, US-05.
- Viết unit test cho quy tắc suy ra số đầu điểm từ số tiết.

**Definition of Done**: chạy được ứng dụng, đăng nhập bằng ba vai trò, thêm sửa xóa
được năm học, môn học, học sinh, giáo viên; toàn bộ test xanh.

### Sprint 2 – Lớp học và nhập điểm (29 điểm)

Mục tiêu: hoàn thành nghiệp vụ trọng tâm là nhập điểm.

- US-06, US-07, US-08, US-09.
- Thiết kế bảng `Diem` lưu theo dòng để số đầu điểm thay đổi được theo môn.
- Viết unit test cho công thức ĐTBmhk với các trường hợp 2, 3, 4 ĐĐGtx.

**Definition of Done**: giáo viên nhập được điểm cả lớp trên một bảng, cột ĐTBmhk
tính đúng công thức, dữ liệu lưu xuống cơ sở dữ liệu và tải lại chính xác.

### Sprint 3 – Tổng kết và tra cứu (24 điểm)

Mục tiêu: hoàn thiện vòng đời của điểm, từ nhập tới kết quả cuối năm.

- US-10, US-11, US-12, US-13, US-14.
- Cài đặt bộ tiêu chí xếp loại Tốt / Khá / Đạt / Chưa đạt theo Điều 9.
- Viết unit test cho từng nhánh xếp loại và cho danh hiệu cuối năm.

**Definition of Done**: học bạ hiển thị đủ ĐTBm ba cột (HKI, HKII, cả năm),
xếp loại đúng theo bộ tiêu chí, học sinh chỉ xem được kết quả của chính mình.

### Sprint 4 – Thống kê và hoàn thiện (18 điểm)

Mục tiêu: phục vụ nhu cầu quản lý và hoàn thiện sản phẩm.

- US-15, US-16, US-17, US-18, US-19.
- Rà soát giao diện, kiểm thử hồi quy, viết tài liệu hướng dẫn.

**Definition of Done**: các báo cáo thống kê khớp với dữ liệu thực tế trong CSDL,
in được bảng tổng kết, tài liệu bàn giao hoàn chỉnh.

## 4. Definition of Done chung

Một user story được coi là hoàn thành khi:

1. Code đã viết xong và biên dịch không lỗi, không cảnh báo.
2. Logic nghiệp vụ mới có unit test tương ứng và toàn bộ test xanh.
3. Chức năng chạy đúng trên dữ liệu mẫu, đã được thành viên khác trong nhóm kiểm tra.
4. Không phá vỡ chức năng đã có (chạy lại toàn bộ test).
5. Đã cập nhật tài liệu nếu có thay đổi về nghiệp vụ hoặc cách sử dụng.

## 5. Rủi ro đã nhận diện

| Rủi ro | Ảnh hưởng | Cách xử lý |
|---|---|---|
| Hiểu sai quy định của Thông tư 22 | Tính sai điểm, sai xếp loại | Trích dẫn điều khoản ngay trong mã nguồn; viết unit test cho từng điều khoản |
| Số đầu điểm khác nhau giữa các môn | Lược đồ CSDL cứng nhắc, khó mở rộng | Lưu mỗi đầu điểm một dòng thay vì cột cố định |
| Nhầm lẫn với cách tính cũ của Thông tư 58 | Báo cáo sai bản chất | Không tính điểm trung bình chung; ghi rõ cột bình quân chỉ để tham khảo |
| Nhập sai điểm ngoài thang 10 | Dữ liệu bẩn | Kiểm tra ở cả trình duyệt và máy chủ; chỉ lưu khi toàn bảng hợp lệ |
