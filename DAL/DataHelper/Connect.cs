using Microsoft.Data.SqlClient;
using System;
using System.Data;

namespace DAL.DataHelper
{
    public class Connect
    {
        public static string? ConnectionString { get; set; }

        // =====================================================
        // LẤY CONNECTION
        // =====================================================
        public static SqlConnection GetConnection()
        {
            if (string.IsNullOrWhiteSpace(ConnectionString))
            {
                throw new InvalidOperationException(
                    "ConnectionString chưa được khởi tạo!");
            }

            return new SqlConnection(ConnectionString);
        }

        // =====================================================
        // SQL QUERY - SELECT
        // =====================================================
        public static DataTable ExecuteQuery(
            string query,
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







        public static int ExecuteStoredProcedureReturnValue(
    string procedureName,
    SqlParameter[]? parameters = null)
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








        // =====================================================
        // SQL QUERY - INSERT / UPDATE / DELETE
        // =====================================================
        public static int ExecuteNonQuery(
            string query,
            SqlParameter[]? parameters = null)
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

        // =====================================================
        // STORED PROCEDURE - SELECT
        // =====================================================
        public static DataTable ExecuteStoredProcedure(
            string procedureName,
            SqlParameter[]? parameters = null)
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

        // =====================================================
        // STORED PROCEDURE - INSERT / UPDATE / DELETE
        // =====================================================
        public static int ExecuteStoredProcedureNonQuery(
            string procedureName,
            SqlParameter[]? parameters = null)
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