using DAL.DataHelper;
using Microsoft.Data.SqlClient;
using System.Data;

namespace DAL;


public sealed class AuditLog_DAL
{
    public async Task WriteChangeAsync( string serviceName, string method, string path, int statusCode,string operationType, string entityName, string? recordKey,
        string result,string? userId, string? userName, string? role,  string? clientIp,  string traceId,  long elapsedMilliseconds,   CancellationToken cancellationToken = default)
    {
        const string sql = @"INSERT INTO dbo.AuditLog
        (ServiceName, HttpMethod, RequestPath, StatusCode, UserId, UserName, RoleName,
         ClientIp, TraceId, ElapsedMilliseconds, OperationType, EntityName, RecordKey, Result)
    VALUES
        (@ServiceName, @HttpMethod, @RequestPath, @StatusCode, @UserId, @UserName, @RoleName,
         @ClientIp, @TraceId, @ElapsedMilliseconds, @OperationType, @EntityName, @RecordKey, @Result);";

        await using var connection = Connect.GetConnection();
        await using var command = new SqlCommand(sql, connection);

        command.Parameters.Add("@ServiceName", SqlDbType.NVarChar, 80).Value = Limit(serviceName, 80);
        command.Parameters.Add("@HttpMethod", SqlDbType.NVarChar, 12).Value = Limit(method, 12);
        command.Parameters.Add("@RequestPath", SqlDbType.NVarChar, 1000).Value = Limit(path, 1000);
        command.Parameters.Add("@StatusCode", SqlDbType.SmallInt).Value = statusCode;
        command.Parameters.Add("@UserId", SqlDbType.NVarChar, 100).Value = DbValue(userId, 100);
        command.Parameters.Add("@UserName", SqlDbType.NVarChar, 100).Value = DbValue(userName, 100);
        command.Parameters.Add("@RoleName", SqlDbType.NVarChar, 50).Value = DbValue(role, 50);
        command.Parameters.Add("@ClientIp", SqlDbType.NVarChar, 45).Value = DbValue(clientIp, 45);
        command.Parameters.Add("@TraceId", SqlDbType.NVarChar, 100).Value = Limit(traceId, 100);
        command.Parameters.Add("@ElapsedMilliseconds", SqlDbType.BigInt).Value = elapsedMilliseconds;
        command.Parameters.Add("@OperationType", SqlDbType.NVarChar, 20).Value = Limit(operationType, 20);
        command.Parameters.Add("@EntityName", SqlDbType.NVarChar, 128).Value = Limit(entityName, 128);
        command.Parameters.Add("@RecordKey", SqlDbType.NVarChar, 200).Value = DbValue(recordKey, 200);
        command.Parameters.Add("@Result", SqlDbType.NVarChar, 20).Value = Limit(result, 20);

        await connection.OpenAsync(cancellationToken);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static object DbValue(string? value, int maxLength) =>
        string.IsNullOrWhiteSpace(value) ? DBNull.Value : Limit(value, maxLength);

    private static string Limit(string? value, int maxLength) =>
        string.IsNullOrEmpty(value) ? string.Empty : value.Length <= maxLength ? value : value[..maxLength];
}
