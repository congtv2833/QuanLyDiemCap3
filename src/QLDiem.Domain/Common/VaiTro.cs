namespace QLDiem.Domain.Common;

/// <summary>Ba vai trò sử dụng hệ thống.</summary>
public static class VaiTro
{
    public const string Admin = "Admin";
    public const string GiaoVien = "GiaoVien";
    public const string HocSinh = "HocSinh";

    public static readonly string[] TatCa = [Admin, GiaoVien, HocSinh];

    public static string TenHienThi(string vaiTro) => vaiTro switch
    {
        Admin => "Quản trị viên",
        GiaoVien => "Giáo viên",
        HocSinh => "Học sinh",
        _ => vaiTro
    };
}
