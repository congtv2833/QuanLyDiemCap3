namespace QLDiem.Domain.Entities;

/// <summary>Phân công một giáo viên dạy một môn ở một lớp trong năm học.</summary>
public class PhanCongGiangDay
{
    public int Id { get; set; }

    public int GiaoVienId { get; set; }
    public GiaoVien GiaoVien { get; set; } = null!;

    public int LopId { get; set; }
    public Lop Lop { get; set; } = null!;

    public int MonHocId { get; set; }
    public MonHoc MonHoc { get; set; } = null!;

    public int NamHocId { get; set; }
    public NamHoc NamHoc { get; set; } = null!;
}
