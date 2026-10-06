using System;
using System.Data;
using DAL.DataHelper;
using Microsoft.Data.SqlClient;

namespace DAL;

public sealed class PayOsPayment_DAL
{
    public decimal? GetInvoiceAmountDue(string maHoaDon)
    {
        const string sql = @"
            SELECT CAST(ROUND(
                SUM(CAST(ISNULL(CT.TONGTIEN, 0) AS DECIMAL(19, 2)))
                + ISNULL(MAX(CAST(H.THUEVAT AS DECIMAL(19, 2))), 0)
                - ISNULL(MAX(CAST(H.GIAMGIA AS DECIMAL(19, 2))), 0), 0
            ) AS DECIMAL(19, 0)) AS SoTienPhaiTra
            FROM dbo.HOADONBAN H
            INNER JOIN dbo.CT_HDB CT ON RTRIM(CT.MAHDBAN) = RTRIM(H.MAHDBAN)
            WHERE RTRIM(H.MAHDBAN) = RTRIM(@MaHoaDon)
            GROUP BY H.MAHDBAN;";

        var table = Connect.ExecuteQuery(sql, new[]
        {
            new SqlParameter("@MaHoaDon", SqlDbType.Char, 15) { Value = maHoaDon.Trim() }
        });

        if (table.Rows.Count == 0 || table.Rows[0]["SoTienPhaiTra"] == DBNull.Value)
            return null;

        return Convert.ToDecimal(table.Rows[0]["SoTienPhaiTra"]);
    }

    public DataTable GetLatestPayOsByInvoice(string maHoaDon)
    {
        const string sql = @"
            SELECT TOP (1) MATHANHTOAN, MAHDBAN, SOTIENTHANHTOAN, TRANGTHAI,
                   PAYOS_ORDER_CODE, PAYOS_PAYMENT_LINK_ID, PAYOS_CHECKOUT_URL, PAYOS_QR_CODE,
                   PAYOS_TRANSACTION_REFERENCE, PAYOS_CREATED_AT_UTC
            FROM dbo.THANHTOAN
            WHERE RTRIM(MAHDBAN) = RTRIM(@MaHoaDon) AND PHUONGTHUC = N'PayOS'
            ORDER BY CASE WHEN TRANGTHAI = N'Đã thanh toán' THEN 0 ELSE 1 END,
                     PAYOS_CREATED_AT_UTC DESC, MATHANHTOAN DESC;";

        return Connect.ExecuteQuery(sql, new[]
        {
            new SqlParameter("@MaHoaDon", SqlDbType.Char, 15) { Value = maHoaDon.Trim() }
        });
    }

    public bool CreatePendingPayment(string maThanhToan, string maHoaDon, decimal soTien, long orderCode)
    {
        const string sql = @"
            INSERT INTO dbo.THANHTOAN
                (MATHANHTOAN, MAHDBAN, PHUONGTHUC, SOTIENTHANHTOAN, NGAYTHANHTOAN,
                 TRANGTHAI, PAYOS_ORDER_CODE, PAYOS_CREATED_AT_UTC)
            SELECT @MaThanhToan, H.MAHDBAN, N'PayOS', @SoTien, NULL,
                   N'Chưa thanh toán', @OrderCode, SYSUTCDATETIME()
            FROM dbo.HOADONBAN H
            WHERE RTRIM(H.MAHDBAN) = RTRIM(@MaHoaDon)
              AND NOT EXISTS (
                  SELECT 1 FROM dbo.THANHTOAN T
                  WHERE RTRIM(T.MAHDBAN) = RTRIM(H.MAHDBAN)
                    AND T.TRANGTHAI = N'Đã thanh toán'
              );";

        int rows = Connect.ExecuteNonQuery(sql, new[]
        {
            new SqlParameter("@MaThanhToan", SqlDbType.Char, 15) { Value = maThanhToan },
            new SqlParameter("@MaHoaDon", SqlDbType.Char, 15) { Value = maHoaDon.Trim() },
            new SqlParameter("@SoTien", SqlDbType.Float) { Value = soTien },
            new SqlParameter("@OrderCode", SqlDbType.BigInt) { Value = orderCode }
        });

        return rows == 1;
    }

    public bool SavePayOsLink(long orderCode, string paymentLinkId, string checkoutUrl, string qrCode)
    {
        const string sql = @"
            UPDATE dbo.THANHTOAN
            SET PAYOS_PAYMENT_LINK_ID = @PaymentLinkId,
                PAYOS_CHECKOUT_URL = @CheckoutUrl,
                PAYOS_QR_CODE = @QrCode
            WHERE PAYOS_ORDER_CODE = @OrderCode
              AND TRANGTHAI = N'Chưa thanh toán';";

        return Connect.ExecuteNonQuery(sql, new[]
        {
            new SqlParameter("@OrderCode", SqlDbType.BigInt) { Value = orderCode },
            new SqlParameter("@PaymentLinkId", SqlDbType.NVarChar, 100) { Value = paymentLinkId },
            new SqlParameter("@CheckoutUrl", SqlDbType.NVarChar, 1000) { Value = checkoutUrl },
            new SqlParameter("@QrCode", SqlDbType.NVarChar, -1) { Value = qrCode }
        }) == 1;
    }

    public void MarkPayOsLinkFailed(long orderCode)
    {
        const string sql = @"
            UPDATE dbo.THANHTOAN
            SET TRANGTHAI = N'Tạo QR thất bại'
            WHERE PAYOS_ORDER_CODE = @OrderCode
              AND TRANGTHAI = N'Chưa thanh toán'
              AND PAYOS_PAYMENT_LINK_ID IS NULL;";

        Connect.ExecuteNonQuery(sql, new[]
        {
            new SqlParameter("@OrderCode", SqlDbType.BigInt) { Value = orderCode }
        });
    }

    public bool ConfirmPayOsPayment(long orderCode, int amount, string reference, string paymentLinkId, DateTime paidAt)
    {
        const string updateSql = @"
            UPDATE dbo.THANHTOAN
            SET TRANGTHAI = N'Đã thanh toán',
                NGAYTHANHTOAN = @PaidAt,
                PAYOS_TRANSACTION_REFERENCE = @Reference
            WHERE PAYOS_ORDER_CODE = @OrderCode
              AND CAST(ROUND(SOTIENTHANHTOAN, 0) AS BIGINT) = @Amount
              AND (PAYOS_PAYMENT_LINK_ID IS NULL OR PAYOS_PAYMENT_LINK_ID = @PaymentLinkId)
              AND TRANGTHAI = N'Chưa thanh toán';";

        int changed = Connect.ExecuteNonQuery(updateSql, new[]
        {
            new SqlParameter("@PaidAt", SqlDbType.DateTime) { Value = paidAt },
            new SqlParameter("@Reference", SqlDbType.NVarChar, 100) { Value = reference },
            new SqlParameter("@OrderCode", SqlDbType.BigInt) { Value = orderCode },
            new SqlParameter("@Amount", SqlDbType.Int) { Value = amount },
            new SqlParameter("@PaymentLinkId", SqlDbType.NVarChar, 100) { Value = paymentLinkId }
        });

        if (changed == 1)
            return true;

        // Webhook của PayOS có thể được gửi lặp lại; chấp nhận lặp đúng giao dịch đã ghi.
        const string duplicateSql = @"
            SELECT COUNT(*)
            FROM dbo.THANHTOAN
            WHERE PAYOS_ORDER_CODE = @OrderCode
              AND CAST(ROUND(SOTIENTHANHTOAN, 0) AS BIGINT) = @Amount
              AND TRANGTHAI = N'Đã thanh toán'
              AND PAYOS_TRANSACTION_REFERENCE = @Reference
              AND (PAYOS_PAYMENT_LINK_ID IS NULL OR PAYOS_PAYMENT_LINK_ID = @PaymentLinkId);";

        var result = Connect.ExecuteQuery(duplicateSql, new[]
        {
            new SqlParameter("@OrderCode", SqlDbType.BigInt) { Value = orderCode },
            new SqlParameter("@Amount", SqlDbType.Int) { Value = amount },
            new SqlParameter("@Reference", SqlDbType.NVarChar, 100) { Value = reference },
            new SqlParameter("@PaymentLinkId", SqlDbType.NVarChar, 100) { Value = paymentLinkId }
        });

        return result.Rows.Count > 0 && Convert.ToInt32(result.Rows[0][0]) > 0;
    }
}
