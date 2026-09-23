using QLDiem.Domain.Enums;

namespace QLDiem.Application.DTOs;

/// <summary>Thống kê xếp loại học tập của một lớp trong một học kỳ.</summary>
public class ThongKeLopDto
{
    public int LopId { get; set; }
    public string TenLop { get; set; } = string.Empty;
    public int Khoi { get; set; }
    public int SiSo { get; set; }
    public int SoTot { get; set; }
    public int SoKha { get; set; }
    public int SoDat { get; set; }
    public int SoChuaDat { get; set; }

    public double TyLeTot => SiSo == 0 ? 0 : Math.Round(SoTot * 100.0 / SiSo, 1);
    public double TyLeKha => SiSo == 0 ? 0 : Math.Round(SoKha * 100.0 / SiSo, 1);
    public double TyLeDat => SiSo == 0 ? 0 : Math.Round(SoDat * 100.0 / SiSo, 1);
    public double TyLeChuaDat => SiSo == 0 ? 0 : Math.Round(SoChuaDat * 100.0 / SiSo, 1);

    /// <summary>Tỷ lệ học sinh từ mức Đạt trở lên.</summary>
    public double TyLeTuDatTroLen => SiSo == 0 ? 0 : Math.Round((SoTot + SoKha + SoDat) * 100.0 / SiSo, 1);
}

/// <summary>Thống kê kết quả của một môn học (theo lớp hoặc toàn trường).</summary>
public class ThongKeMonDto
{
    public int MonHocId { get; set; }
    public string TenMon { get; set; } = string.Empty;
    public int SoHocSinhCoDiem { get; set; }
    public decimal? DiemTrungBinh { get; set; }
    public decimal? DiemCaoNhat { get; set; }
    public decimal? DiemThapNhat { get; set; }

    /// <summary>Phổ điểm: số học sinh ở từng khoảng điểm.</summary>
    public int SoGioi { get; set; }      // >= 8,0
    public int SoKha { get; set; }       // 6,5 - 7,9
    public int SoTrungBinh { get; set; } // 5,0 - 6,4
    public int SoYeu { get; set; }       // 3,5 - 4,9
    public int SoKem { get; set; }       // < 3,5

    public double TyLeTuTrungBinh => SoHocSinhCoDiem == 0
        ? 0
        : Math.Round((SoGioi + SoKha + SoTrungBinh) * 100.0 / SoHocSinhCoDiem, 1);
}

/// <summary>Số liệu tổng quan hiển thị ở trang chủ.</summary>
public class TongQuanDto
{
    public string TenNamHoc { get; set; } = string.Empty;
    public int SoHocSinh { get; set; }
    public int SoGiaoVien { get; set; }
    public int SoLop { get; set; }
    public int SoMonHoc { get; set; }
    public int SoDauDiemDaNhap { get; set; }
    public List<ThongKeLopDto> ThongKeTheoLop { get; set; } = new();
}

/// <summary>Một học sinh trong bảng xếp hạng của lớp.</summary>
public class XepHangHocSinhDto
{
    public int ThuHang { get; set; }
    public int HocSinhId { get; set; }
    public string MaHocSinh { get; set; } = string.Empty;
    public string HoTen { get; set; } = string.Empty;
    public string TenLop { get; set; } = string.Empty;
    public MucDanhGia XepLoai { get; set; }
    public int SoMonTinhDiem { get; set; }

    /// <summary>
    /// Trung bình cộng ĐTBm các môn - chỉ dùng để sắp thứ tự hiển thị,
    /// KHÔNG phải căn cứ xếp loại theo Thông tư 22.
    /// </summary>
    public decimal? DiemBinhQuanThamKhao { get; set; }
}
