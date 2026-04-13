using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using BCrypt.Net;

namespace Cursah
{
    #region Auth Service
    /// <summary>
    /// Сервис для управления авторизацией и сессиями пользователей.
    /// </summary>
    public class AuthService
    {
        private readonly string _connectionString;
        private const string SessionFile = ".session";

        public AuthService(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Выполняет авторизацию пользователя.
        /// </summary>
        public User Login(string username, string password)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            var cmd = new SqlCommand("SELECT Id, Username, PasswordHash, Role FROM Users WHERE Username = @u", conn);
            cmd.Parameters.AddWithValue("@u", username);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                if (BCrypt.Net.BCrypt.Verify(password, reader.GetString(2)))
                {
                    var user = new User(reader.GetInt32(0), reader.GetString(1), reader.GetString(3));
                    File.WriteAllText(SessionFile, JsonSerializer.Serialize(user));
                    return user;
                }
            }
            throw new AppException("401", "Неверное имя пользователя или пароль.");
        }
        /// <summary>
        /// Выполняет выход из сессии пользователя.
        /// </summary>
        public void Logout()
        {
            if (File.Exists(SessionFile))
            {
                File.Delete(SessionFile);
            }
        }

        /// <summary>
        /// Проверка сессии пользователя.
        /// </summary>
        public User? LoadSession()
        {
            if (!File.Exists(SessionFile))
            {
                return null;
            }

            try
            {
                return JsonSerializer.Deserialize<User>(File.ReadAllText(SessionFile));
            }
            catch
            {
                return null;
            }
        }
    }
    #endregion
}
