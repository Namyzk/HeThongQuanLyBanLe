using DAL.DataHelper;
using Microsoft.Data.SqlClient;
using System;
using System.Data;

namespace DAL
{
    public sealed class RefreshToken_DAL
    {
        public bool Insert(string tokenHash, string accountId, DateTime expiresAtUtc)
        {
            const string sql = @"
                INSERT INTO dbo.RefreshToken (RefreshTokenHash, AccountId, ExpiresAtUtc)
                VALUES (@Hash, @AccountId, @ExpiresAtUtc);";

            return Connect.ExecuteNonQuery(sql, new[]
            {
                new SqlParameter("@Hash", SqlDbType.Char, 64) { Value = tokenHash },
                new SqlParameter("@AccountId", SqlDbType.Char, 15) { Value = accountId },
                new SqlParameter("@ExpiresAtUtc", SqlDbType.DateTime2) { Value = expiresAtUtc }
            }) == 1;
        }

        //Thu hồi token cũ và lưu token mới trong cùng transaction.</summary>
        public string? Rotate(string oldHash, string newHash, DateTime newExpiresAtUtc)
        {
            return Connect.ExecuteInTransaction<string?>((connection, transaction) =>
            {
                const string updateSql = @"
                    UPDATE dbo.RefreshToken
                    SET RevokedAtUtc = SYSUTCDATETIME(), ReplacedByHash = @NewHash
                    OUTPUT inserted.AccountId
                    WHERE RefreshTokenHash = @OldHash
                      AND RevokedAtUtc IS NULL
                      AND ExpiresAtUtc > SYSUTCDATETIME();";

                using var update = new SqlCommand(updateSql, connection, transaction);
                update.Parameters.Add("@NewHash", SqlDbType.Char, 64).Value = newHash;
                update.Parameters.Add("@OldHash", SqlDbType.Char, 64).Value = oldHash;
                var accountId = update.ExecuteScalar()?.ToString()?.Trim();
                if (string.IsNullOrWhiteSpace(accountId)) return null;

                const string insertSql = @"
                    INSERT INTO dbo.RefreshToken (RefreshTokenHash, AccountId, ExpiresAtUtc)
                    VALUES (@Hash, @AccountId, @ExpiresAtUtc);";
                using var insert = new SqlCommand(insertSql, connection, transaction);
                insert.Parameters.Add("@Hash", SqlDbType.Char, 64).Value = newHash;
                insert.Parameters.Add("@AccountId", SqlDbType.Char, 15).Value = accountId;
                insert.Parameters.Add("@ExpiresAtUtc", SqlDbType.DateTime2).Value = newExpiresAtUtc;
                insert.ExecuteNonQuery();
                return accountId;
            });
        }

        public bool Revoke(string tokenHash)
        {
            const string sql = @"
                UPDATE dbo.RefreshToken
                SET RevokedAtUtc = SYSUTCDATETIME()
                WHERE RefreshTokenHash = @Hash
                  AND RevokedAtUtc IS NULL;";

            return Connect.ExecuteNonQuery(sql, new[]
            {
                new SqlParameter("@Hash", SqlDbType.Char, 64) { Value = tokenHash }
            }) == 1;
        }
    }
}
