using System.ComponentModel.DataAnnotations;

namespace QLDiem.Models;

/// <summary>
/// Các biểu thức chính quy dùng chung cho việc kiểm tra dữ liệu nhập.
/// Đặt thành hằng số để dùng lại ở nhiều thực thể và sửa một chỗ khi cần.
/// </summary>
public static class QuyTacNhapLieu
{
    /// <summary>Số điện thoại Việt Nam: đúng 10 chữ số, bắt đầu bằng 0.</summary>
    public const string SoDienThoai = @"^0\d{9}$";
    public const string LoiSoDienThoai = "Số điện thoại phải gồm đúng 10 chữ số và bắt đầu bằng số 0.";

    /// <summary>Mã định danh: chỉ chữ cái không dấu và chữ số, 2 đến 20 ký tự.</summary>
    public const string MaDinhDanh = @"^[A-Za-z0-9]{2,20}$";
    public const string LoiMaDinhDanh = "Mã chỉ gồm chữ cái không dấu và chữ số, độ dài từ 2 đến 20 ký tự.";

    /// <summary>Tên năm học dạng 2025-2026.</summary>
    public const string TenNamHoc = @"^\d{4}-\d{4}$";
    public const string LoiTenNamHoc = "Năm học phải theo dạng 2025-2026.";
}

/// <summary>
/// Ngày sinh phải nằm trong quá khứ và người đó không quá <see cref="TuoiToiDa"/> tuổi.
/// Dùng cho cả <c>DateOnly</c> và <c>DateOnly?</c>.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class NgaySinhHopLeAttribute : ValidationAttribute
{
    public int TuoiToiDa { get; set; } = 100;

    /// <summary>Tuổi tối thiểu tính đến hôm nay; 0 nghĩa là chỉ cần ngày trong quá khứ.</summary>
    public int TuoiToiThieu { get; set; }

    protected override ValidationResult? IsValid(object? value, ValidationContext context)
    {
        if (value is null) return ValidationResult.Success; // để [Required] lo phần bắt buộc
        if (value is not DateOnly ngay) return ValidationResult.Success;

        var homNay = DateOnly.FromDateTime(DateTime.Today);
        var ten = context.DisplayName;

        if (ngay >= homNay)
            return new ValidationResult($"{ten} phải là một ngày trong quá khứ.");

        int tuoi = homNay.Year - ngay.Year;
        if (ngay > homNay.AddYears(-tuoi)) tuoi--;

        if (tuoi > TuoiToiDa)
            return new ValidationResult($"{ten} không hợp lệ: quá {TuoiToiDa} tuổi.");

        if (TuoiToiThieu > 0 && tuoi < TuoiToiThieu)
            return new ValidationResult($"{ten} không hợp lệ: phải từ {TuoiToiThieu} tuổi trở lên.");

        return ValidationResult.Success;
    }
}

/// <summary>Ngày kết thúc năm học phải sau ngày bắt đầu.</summary>
[AttributeUsage(AttributeTargets.Property)]
public class SauNgayAttribute : ValidationAttribute
{
    private readonly string _tenThuocTinhTruoc;

    public SauNgayAttribute(string tenThuocTinhTruoc) => _tenThuocTinhTruoc = tenThuocTinhTruoc;

    protected override ValidationResult? IsValid(object? value, ValidationContext context)
    {
        if (value is not DateOnly ngaySau) return ValidationResult.Success;

        var thuocTinh = context.ObjectType.GetProperty(_tenThuocTinhTruoc);
        if (thuocTinh?.GetValue(context.ObjectInstance) is not DateOnly ngayTruoc)
            return ValidationResult.Success;

        return ngaySau > ngayTruoc
            ? ValidationResult.Success
            : new ValidationResult($"{context.DisplayName} phải sau ngày bắt đầu.");
    }
}
