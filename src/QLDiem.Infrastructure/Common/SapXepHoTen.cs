using System.Globalization;

namespace QLDiem.Infrastructure.Common;

/// <summary>
/// Sắp xếp danh sách theo thông lệ Việt Nam: theo Tên trước, sau đó đến Họ và chữ đệm,
/// so sánh theo bảng chữ cái tiếng Việt.
/// </summary>
public static class SapXepHoTen
{
    private static readonly CompareInfo TiengViet =
        CultureInfo.GetCultureInfo("vi-VN").CompareInfo;

    private sealed class TiengVietComparer : IComparer<string>
    {
        public int Compare(string? x, string? y) =>
            TiengViet.Compare(x ?? string.Empty, y ?? string.Empty, CompareOptions.IgnoreCase);
    }

    private static readonly IComparer<string> Comparer = new TiengVietComparer();

    /// <summary>Tách phần Tên (từ cuối cùng) trong họ tên đầy đủ.</summary>
    public static string LayTen(string hoTen)
    {
        if (string.IsNullOrWhiteSpace(hoTen)) return string.Empty;
        var phan = hoTen.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return phan[^1];
    }

    public static IOrderedEnumerable<T> SapTheoTen<T>(this IEnumerable<T> nguon, Func<T, string> chonHoTen) =>
        nguon.OrderBy(x => LayTen(chonHoTen(x)), Comparer)
             .ThenBy(chonHoTen, Comparer);
}
