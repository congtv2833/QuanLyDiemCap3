using QLDiem.Application.DTOs;
using QLDiem.Domain.Enums;

namespace QLDiem.Application.Interfaces;

/// <summary>Nhập, sửa và tra cứu các đầu điểm.</summary>
public interface IDiemService
{
    /// <summary>Lấy bảng điểm của một lớp theo môn và học kỳ, kèm ĐTBmhk đã tính sẵn.</summary>
    Task<BangDiemLopDto?> LayBangDiemLopAsync(int lopId, int monHocId, HocKy hocKy);

    /// <summary>Lưu toàn bộ bảng điểm vừa nhập. Ô để trống nghĩa là xóa đầu điểm đó.</summary>
    Task<KetQuaThaoTac> LuuBangDiemAsync(LuuBangDiemDto duLieu);

    /// <summary>Chi tiết các đầu điểm của một học sinh trong một học kỳ, dùng cho màn hình tra cứu.</summary>
    Task<List<ChiTietDiemMonDto>> LayChiTietDiemAsync(int hocSinhId, HocKy hocKy);
}
