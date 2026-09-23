using QLDiem.Application.DTOs;
using QLDiem.Domain.Enums;

namespace QLDiem.Application.Interfaces;

/// <summary>Thống kê, tổng hợp kết quả học tập.</summary>
public interface IThongKeService
{
    /// <summary>Số liệu tổng quan của năm học đang hoạt động, dùng cho trang chủ.</summary>
    Task<TongQuanDto> LayTongQuanAsync(HocKy hocKy);

    /// <summary>Phân bố xếp loại học tập của từng lớp trong một học kỳ.</summary>
    Task<List<ThongKeLopDto>> ThongKeTheoLopAsync(int namHocId, HocKy hocKy, int? khoi = null);

    /// <summary>Phổ điểm từng môn; truyền lopId để giới hạn trong một lớp.</summary>
    Task<List<ThongKeMonDto>> ThongKeTheoMonAsync(int namHocId, HocKy hocKy, int? lopId = null);
}
