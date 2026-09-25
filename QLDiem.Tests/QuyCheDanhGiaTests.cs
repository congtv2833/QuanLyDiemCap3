using QLDiem.Models;
using QLDiem.Services;


namespace QLDiem.Tests;

/// <summary>
/// Kiểm thử các quy tắc đánh giá theo Thông tư 22/2021/TT-BGDĐT.
/// Mỗi nhóm test bám sát một điều khoản cụ thể của thông tư.
/// </summary>
public class QuyCheDanhGiaTests
{
    // ----- Điều 6 khoản 1 điểm b: số đầu điểm thường xuyên -----

    [Theory]
    [InlineData(35, 2)]   // Giáo dục quốc phòng và an ninh
    [InlineData(18, 2)]
    [InlineData(36, 3)]
    [InlineData(52, 3)]   // Lịch sử
    [InlineData(70, 3)]   // Vật lí, Hóa học, Sinh học...
    [InlineData(71, 4)]
    [InlineData(105, 4)]  // Toán, Ngữ văn, Ngoại ngữ
    public void SoDauDiemThuongXuyen_TheoSoTietTrongNam(int soTiet, int mongDoi)
    {
        Assert.Equal(mongDoi, QuyCheDanhGia.SoDauDiemThuongXuyen(soTiet));
    }

    [Fact]
    public void SoDauDiemThuongXuyen_MonKhongCoTiet_TraVeKhong()
    {
        Assert.Equal(0, QuyCheDanhGia.SoDauDiemThuongXuyen(0));
    }

    // ----- Điều 9 khoản 1: hệ số và công thức ĐTBmhk -----

    [Fact]
    public void HeSo_DungTheoThongTu22()
    {
        Assert.Equal(1, QuyCheDanhGia.HeSo(LoaiDiem.ThuongXuyen));
        Assert.Equal(2, QuyCheDanhGia.HeSo(LoaiDiem.GiuaKy));
        Assert.Equal(3, QuyCheDanhGia.HeSo(LoaiDiem.CuoiKy));
    }

    [Fact]
    public void TinhDtbHocKy_MonBonDauDiemThuongXuyen()
    {
        // (8 + 7 + 9 + 6) + 2 x 8 + 3 x 7,5 = 68,5 ; chia cho 4 + 5 = 9 -> 7,611 -> 7,6
        var diems = new[]
        {
            new DiemThanhPhan(LoaiDiem.ThuongXuyen, 8m),
            new DiemThanhPhan(LoaiDiem.ThuongXuyen, 7m),
            new DiemThanhPhan(LoaiDiem.ThuongXuyen, 9m),
            new DiemThanhPhan(LoaiDiem.ThuongXuyen, 6m),
            new DiemThanhPhan(LoaiDiem.GiuaKy, 8m),
            new DiemThanhPhan(LoaiDiem.CuoiKy, 7.5m)
        };

        Assert.Equal(7.6m, QuyCheDanhGia.TinhDiemTrungBinhMonHocKy(diems));
    }

    [Fact]
    public void TinhDtbHocKy_MonHaiDauDiemThuongXuyen()
    {
        // (9 + 8) + 2 x 7 + 3 x 8 = 55 ; chia cho 2 + 5 = 7 -> 7,857 -> 7,9
        var diems = new[]
        {
            new DiemThanhPhan(LoaiDiem.ThuongXuyen, 9m),
            new DiemThanhPhan(LoaiDiem.ThuongXuyen, 8m),
            new DiemThanhPhan(LoaiDiem.GiuaKy, 7m),
            new DiemThanhPhan(LoaiDiem.CuoiKy, 8m)
        };

        Assert.Equal(7.9m, QuyCheDanhGia.TinhDiemTrungBinhMonHocKy(diems));
    }

    [Fact]
    public void TinhDtbHocKy_DiemCuoiKyCoTrongSoLonNhat()
    {
        var caoCuoiKy = new[]
        {
            new DiemThanhPhan(LoaiDiem.ThuongXuyen, 5m),
            new DiemThanhPhan(LoaiDiem.ThuongXuyen, 5m),
            new DiemThanhPhan(LoaiDiem.GiuaKy, 5m),
            new DiemThanhPhan(LoaiDiem.CuoiKy, 10m)
        };

        var caoGiuaKy = new[]
        {
            new DiemThanhPhan(LoaiDiem.ThuongXuyen, 5m),
            new DiemThanhPhan(LoaiDiem.ThuongXuyen, 5m),
            new DiemThanhPhan(LoaiDiem.GiuaKy, 10m),
            new DiemThanhPhan(LoaiDiem.CuoiKy, 5m)
        };

        Assert.True(QuyCheDanhGia.TinhDiemTrungBinhMonHocKy(caoCuoiKy)
                    > QuyCheDanhGia.TinhDiemTrungBinhMonHocKy(caoGiuaKy));
    }

    [Fact]
    public void TinhDtbHocKy_ThieuDiemCuoiKy_TraVeNull()
    {
        var diems = new[]
        {
            new DiemThanhPhan(LoaiDiem.ThuongXuyen, 8m),
            new DiemThanhPhan(LoaiDiem.GiuaKy, 8m)
        };

        Assert.Null(QuyCheDanhGia.TinhDiemTrungBinhMonHocKy(diems));
    }

    [Fact]
    public void TinhDtbHocKy_ThieuDiemGiuaKy_TraVeNull()
    {
        var diems = new[]
        {
            new DiemThanhPhan(LoaiDiem.ThuongXuyen, 8m),
            new DiemThanhPhan(LoaiDiem.CuoiKy, 8m)
        };

        Assert.Null(QuyCheDanhGia.TinhDiemTrungBinhMonHocKy(diems));
    }

    [Fact]
    public void TinhDtbHocKy_ChuaCoDiemThuongXuyen_VanTinhDuoc()
    {
        // (2 x 6 + 3 x 7) / 5 = 33 / 5 = 6,6
        var diems = new[]
        {
            new DiemThanhPhan(LoaiDiem.GiuaKy, 6m),
            new DiemThanhPhan(LoaiDiem.CuoiKy, 7m)
        };

        Assert.Equal(6.6m, QuyCheDanhGia.TinhDiemTrungBinhMonHocKy(diems));
    }

    // ----- Làm tròn đến chữ số thập phân thứ nhất -----

    [Theory]
    [InlineData(7.611, 7.6)]
    [InlineData(7.65, 7.7)]   // làm tròn ra xa số 0
    [InlineData(7.649, 7.6)]
    [InlineData(8.05, 8.1)]
    [InlineData(10.0, 10.0)]
    public void LamTron_DenMotChuSoThapPhan(decimal dauVao, decimal mongDoi)
    {
        Assert.Equal(mongDoi, QuyCheDanhGia.LamTron(dauVao));
    }

    // ----- Điều 9 khoản 2: ĐTBmcn, học kỳ II hệ số 2 -----

    [Fact]
    public void TinhDtbCaNam_HocKyHaiTinhHeSoHai()
    {
        // (7,6 + 2 x 8,0) / 3 = 23,6 / 3 = 7,866 -> 7,9
        Assert.Equal(7.9m, QuyCheDanhGia.TinhDiemTrungBinhMonCaNam(7.6m, 8.0m));
    }

    [Fact]
    public void TinhDtbCaNam_ThieuMotHocKy_TraVeNull()
    {
        Assert.Null(QuyCheDanhGia.TinhDiemTrungBinhMonCaNam(7.6m, null));
        Assert.Null(QuyCheDanhGia.TinhDiemTrungBinhMonCaNam(null, 8.0m));
    }

    // ----- Điều 9 khoản 3: xếp loại kết quả học tập -----

    [Fact]
    public void XepLoai_Tot_KhiMoiMonTu65VaItNhatSauMonTu80()
    {
        var diems = new[] { 8.5m, 8.2m, 9.0m, 8.0m, 8.8m, 8.1m, 7.0m, 6.5m, 6.9m };
        var nhanXet = new[] { KetQuaNhanXet.Dat, KetQuaNhanXet.Dat };

        Assert.Equal(MucDanhGia.Tot, QuyCheDanhGia.XepLoaiHocTap(diems, nhanXet));
    }

    [Fact]
    public void XepLoai_KhongDatTot_KhiChiCoNamMonTu80()
    {
        var diems = new[] { 8.5m, 8.2m, 9.0m, 8.0m, 8.8m, 7.9m, 7.0m, 6.5m, 6.9m };
        var nhanXet = new[] { KetQuaNhanXet.Dat };

        Assert.Equal(MucDanhGia.Kha, QuyCheDanhGia.XepLoaiHocTap(diems, nhanXet));
    }

    [Fact]
    public void XepLoai_KhongDatTot_KhiMotMonNhanXetChuaDat()
    {
        var diems = new[] { 8.5m, 8.2m, 9.0m, 8.0m, 8.8m, 8.1m, 7.0m, 6.5m, 6.9m };
        var nhanXet = new[] { KetQuaNhanXet.Dat, KetQuaNhanXet.ChuaDat };

        // Mọi môn đều từ 6,5 nên vẫn thỏa tiêu chí điểm của mức Đạt.
        Assert.Equal(MucDanhGia.Dat, QuyCheDanhGia.XepLoaiHocTap(diems, nhanXet));
    }

    [Fact]
    public void XepLoai_Kha_KhiMoiMonTu50VaItNhatSauMonTu65()
    {
        var diems = new[] { 7.0m, 6.8m, 7.5m, 6.5m, 6.6m, 6.9m, 5.5m, 5.0m, 5.2m };
        var nhanXet = new[] { KetQuaNhanXet.Dat };

        Assert.Equal(MucDanhGia.Kha, QuyCheDanhGia.XepLoaiHocTap(diems, nhanXet));
    }

    [Fact]
    public void XepLoai_Dat_KhiCoMonDuoi50NhungKhongMonNaoDuoi35()
    {
        var diems = new[] { 7.0m, 6.8m, 7.5m, 6.5m, 6.6m, 5.9m, 4.5m, 4.0m, 3.5m };
        var nhanXet = new[] { KetQuaNhanXet.Dat };

        Assert.Equal(MucDanhGia.Dat, QuyCheDanhGia.XepLoaiHocTap(diems, nhanXet));
    }

    [Fact]
    public void XepLoai_ChuaDat_KhiCoMonDuoi35()
    {
        var diems = new[] { 7.0m, 6.8m, 7.5m, 6.5m, 6.6m, 5.9m, 4.5m, 4.0m, 3.4m };
        var nhanXet = new[] { KetQuaNhanXet.Dat };

        Assert.Equal(MucDanhGia.ChuaDat, QuyCheDanhGia.XepLoaiHocTap(diems, nhanXet));
    }

    [Fact]
    public void XepLoai_ChuaDat_KhiKhongDuSauMonTu50()
    {
        var diems = new[] { 7.0m, 6.8m, 7.5m, 6.5m, 5.0m, 4.9m, 4.5m, 4.0m, 3.6m };

        Assert.Equal(MucDanhGia.ChuaDat,
            QuyCheDanhGia.XepLoaiHocTap(diems, Array.Empty<KetQuaNhanXet>()));
    }

    [Fact]
    public void XepLoai_ChuaDat_KhiHaiMonNhanXetChuaDat()
    {
        var diems = new[] { 8.5m, 8.2m, 9.0m, 8.0m, 8.8m, 8.1m, 7.0m, 6.5m, 6.9m };
        var nhanXet = new[] { KetQuaNhanXet.ChuaDat, KetQuaNhanXet.ChuaDat };

        Assert.Equal(MucDanhGia.ChuaDat, QuyCheDanhGia.XepLoaiHocTap(diems, nhanXet));
    }

    [Fact]
    public void XepLoai_ChuaCoDiem_TraVeChuaDat()
    {
        Assert.Equal(MucDanhGia.ChuaDat,
            QuyCheDanhGia.XepLoaiHocTap(Array.Empty<decimal>(), Array.Empty<KetQuaNhanXet>()));
    }

    // ----- Điều 15: danh hiệu thi đua -----

    [Fact]
    public void DanhHieu_XuatSac_KhiTotVaItNhatSauMonTu90()
    {
        var diems = new[] { 9.0m, 9.2m, 9.5m, 9.1m, 9.3m, 9.0m, 8.0m, 7.5m };

        Assert.Equal(DanhHieu.HocSinhXuatSac,
            QuyCheDanhGia.XetDanhHieu(MucDanhGia.Tot, MucDanhGia.Tot, diems));
    }

    [Fact]
    public void DanhHieu_Gioi_KhiTotNhungChuaDuSauMonTu90()
    {
        var diems = new[] { 9.0m, 9.2m, 9.5m, 9.1m, 9.3m, 8.9m, 8.0m, 7.5m };

        Assert.Equal(DanhHieu.HocSinhGioi,
            QuyCheDanhGia.XetDanhHieu(MucDanhGia.Tot, MucDanhGia.Tot, diems));
    }

    [Fact]
    public void DanhHieu_KhongDat_KhiRenLuyenChuaTot()
    {
        var diems = new[] { 9.0m, 9.2m, 9.5m, 9.1m, 9.3m, 9.0m };

        Assert.Equal(DanhHieu.KhongDat,
            QuyCheDanhGia.XetDanhHieu(MucDanhGia.Tot, MucDanhGia.Kha, diems));
    }

    // ----- Kiểm tra đủ đầu điểm -----

    [Fact]
    public void DaDuDauDiem_DuSoLuongTheoQuyDinh()
    {
        var diems = new[]
        {
            new DiemThanhPhan(LoaiDiem.ThuongXuyen, 8m),
            new DiemThanhPhan(LoaiDiem.ThuongXuyen, 7m),
            new DiemThanhPhan(LoaiDiem.ThuongXuyen, 9m),
            new DiemThanhPhan(LoaiDiem.GiuaKy, 8m),
            new DiemThanhPhan(LoaiDiem.CuoiKy, 8m)
        };

        Assert.True(QuyCheDanhGia.DaDuDauDiem(diems, 3));
        Assert.False(QuyCheDanhGia.DaDuDauDiem(diems, 4));
    }

    [Theory]
    [InlineData(-0.1, false)]
    [InlineData(0, true)]
    [InlineData(10, true)]
    [InlineData(10.1, false)]
    public void LaDiemHopLe_TrongKhoangKhongDenMuoi(decimal giaTri, bool mongDoi)
    {
        Assert.Equal(mongDoi, QuyCheDanhGia.LaDiemHopLe(giaTri));
    }
}
