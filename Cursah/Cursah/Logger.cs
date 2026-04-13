using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cursah
{
    #region Logger
    /// <summary>
    /// Глобальный класс для записи логов в файл и базу данных.
    /// </summary>
    public static class Logger
    {
        private static string? _connectionString;

        public static void Init(string connectionString)
        {
            _connectionString = connectionString;
        }

        public static void Log(string level, string module, string message, int? userId = null, Exception? ex = null)
        {
            try
            {
                Directory.CreateDirectory("logs");
                string entry = $"[{DateTime.Now:G}] [{level}] [{module}] {message} {ex?.Message}";
                File.AppendAllLines("logs/app.log", new[] { entry });
            }
            catch
            {
                
            }

            try
            {
                using var conn = new SqlConnection(_connectionString);
                conn.Open();

                const string sql = "INSERT INTO SystemLogs (Level, Module, UserId, ErrorCode, Message, StackTrace) VALUES (@l,@m,@u,@e,@msg,@s)";
                using var cmd = new SqlCommand(sql, conn);

                cmd.Parameters.AddWithValue("@l", level);
                cmd.Parameters.AddWithValue("@m", module);
                cmd.Parameters.AddWithValue("@u", (object?)userId ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@e", (ex as AppException)?.ErrorCode ?? "500");
                cmd.Parameters.AddWithValue("@msg", message);
                cmd.Parameters.AddWithValue("@s", (object?)ex?.StackTrace ?? DBNull.Value);

                cmd.ExecuteNonQuery();
            }
            catch
            {
                
            }
        }
    }
    #endregion
}
