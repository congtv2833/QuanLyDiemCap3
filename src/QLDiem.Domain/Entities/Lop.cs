namespace QLDiem.Domain.Entities;

public class Lop
{
    public int Id { get; set; }
    public string Ten { get; set; } = string.Empty;

    /// <summary>Khối 10, 11 hoặc 12.</summary>
    public int Khoi { get; set; }

    public int NamHocId { get; set; }
    public NamHoc NamHoc { get; set; } = null!;

    public int? GiaoVienChuNhiemId { get; set; }
    public GiaoVien? GiaoVienChuNhiem { get; set; }

    public ICollection<HocSinh> HocSinhs { get; set; } = new List<HocSinh>();
    public ICollection<PhanCongGiangDay> PhanCongs { get; set; } = new List<PhanCongGiangDay>();
}
