namespace QLDiem.Domain.Entities;

/// <summary>Năm học, ví dụ "2025-2026". Mọi dữ liệu lớp - phân công - điểm đều gắn với một năm học.</summary>
public class NamHoc
{
    public int Id { get; set; }
    public string Ten { get; set; } = string.Empty;
    public DateOnly NgayBatDau { get; set; }
    public DateOnly NgayKetThuc { get; set; }

    /// <summary>Năm học đang diễn ra. Chỉ một năm học được phép bật cờ này.</summary>
    public bool DangHoatDong { get; set; }

    public ICollection<Lop> DanhSachLop { get; set; } = new List<Lop>();
}
