using QLDiem.Domain.Enums;

namespace QLDiem.Domain.Common;

/// <summary>Một đầu điểm rút gọn, dùng làm đầu vào cho các hàm tính toán thuần.</summary>
public readonly record struct DiemThanhPhan(LoaiDiem LoaiDiem, decimal GiaTri);
