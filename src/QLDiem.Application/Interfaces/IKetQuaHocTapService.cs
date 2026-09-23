using QLDiem.Application.DTOs;
using QLDiem.Domain.Enums;

namespace QLDiem.Application.Interfaces;

/// <summary>Tổng kết điểm, xếp loại học tập và lập học bạ.</summary>
public interface IKetQuaHocTapService
{
    /// <summary>Học bạ của một học sinh trong năm học: ĐTBm từng môn theo học kỳ, cả năm và xếp loại.</summary>
    Task<HocBaDto?> LayHocBaAsync(int hocSinhId, int namHocId);

    /// <summary>Bảng tổng kết của cả lớp trong một học kỳ (hoặc cả năm), đã sắp xếp theo kết quả.</summary>
    Task<List<XepHangHocSinhDto>> LayBangTongKetLopAsync(int lopId, HocKy hocKy);

    /// <summary>
    /// Tính lại và ghi kết quả học kỳ cho toàn bộ học sinh của lớp
    /// vào bảng KetQuaHocKy, phục vụ tra cứu và thống kê nhanh.
    /// </summary>
    Task<KetQuaThaoTac> TongKetLopAsync(int lopId, HocKy hocKy);
}
