using Microsoft.Data.SqlClient;
using System;
using System.Data;

namespace DAL.DataHelper
{
    public class Connect
    {
        public static string? ConnectionString { get; set; }

        public static SqlConnection GetConnection()
        {
            if (string.IsNullOrWhiteSpace(ConnectionString))
            {
                throw new InvalidOperationException(
                    "ConnectionString chưa được khởi tạo!");
            }

            return new SqlConnection(ConnectionString);
        }

        public static DataTable ExecuteQuery( string query,
            SqlParameter[]? parameters = null)
        {
            DataTable dt = new DataTable();

            using (SqlConnection conn = GetConnection())
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.CommandType = CommandType.Text;

                if (parameters != null)
                {
                    cmd.Parameters.AddRange(parameters);
                }

                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    da.Fill(dt);
                }
            }

            return dt;
        }


        public static int ExecuteStoredProcedureReturnValue( string procedureName, SqlParameter[]? parameters = null)
        {
            using (SqlConnection conn = GetConnection())
            using (SqlCommand cmd = new SqlCommand(procedureName, conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                if (parameters != null)
                {
                    cmd.Parameters.AddRange(parameters);
                }

                SqlParameter returnParameter = new SqlParameter
                {
                    ParameterName = "@RETURN_VALUE",
                    SqlDbType = SqlDbType.Int,
                    Direction = ParameterDirection.ReturnValue
                };

                cmd.Parameters.Add(returnParameter);

                conn.Open();

                cmd.ExecuteNonQuery();

                return Convert.ToInt32(returnParameter.Value);
            }
        }


        public static int ExecuteNonQuery(string query, SqlParameter[]? parameters = null)
        {
            using (SqlConnection conn = GetConnection())
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.CommandType = CommandType.Text;

                if (parameters != null)
                {
                    cmd.Parameters.AddRange(parameters);
                }

                conn.Open();

                return cmd.ExecuteNonQuery();
            }
        }

        public static T ExecuteInTransaction<T>(Func<SqlConnection, SqlTransaction, T> operation)
        {
            ArgumentNullException.ThrowIfNull(operation);
            using SqlConnection conn = GetConnection();
            conn.Open();
            using SqlTransaction transaction = conn.BeginTransaction();
            try
            {
                T result = operation(conn, transaction);
                transaction.Commit();
                return result;
            }
            catch
            {
                try { transaction.Rollback(); }
                catch { /* Keep the original database exception. */ }
                throw;
            }
        }

        public static DataTable ExecuteStoredProcedure( string procedureName, SqlParameter[]? parameters = null)
        {
            DataTable dt = new DataTable();

            using (SqlConnection conn = GetConnection())
            using (SqlCommand cmd = new SqlCommand(procedureName, conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                if (parameters != null)
                {
                    cmd.Parameters.AddRange(parameters);
                }

                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    da.Fill(dt);
                }
            }

            return dt;
        }

        public static int ExecuteStoredProcedureNonQuery( string procedureName,  SqlParameter[]? parameters = null)
        {
            using (SqlConnection conn = GetConnection())
            using (SqlCommand cmd = new SqlCommand(procedureName, conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                if (parameters != null)
                {
                    cmd.Parameters.AddRange(parameters);
                }

                conn.Open();

                return cmd.ExecuteNonQuery();
            }
        }
    }
}
