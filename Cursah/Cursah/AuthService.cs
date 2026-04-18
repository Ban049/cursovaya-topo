using Microsoft.Data.SqlClient;
using System;
using System.IO;
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

            // ДОБАВЛЕНО: Выборка поля BlockedUntil (оно под индексом 4)
            var cmd = new SqlCommand("SELECT Id, Username, PasswordHash, Role, BlockedUntil FROM Users WHERE Username = @u", conn);
            cmd.Parameters.AddWithValue("@u", username);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                // Сначала проверяем правильность пароля
                if (BCrypt.Net.BCrypt.Verify(password, reader.GetString(2)))
                {
                    // ДОБАВЛЕНО: Проверка блокировки
                    if (!reader.IsDBNull(4))
                    {
                        DateTime blockedUntil = reader.GetDateTime(4);
                        if (blockedUntil > DateTime.Now)
                        {
                            // Если время блокировки еще не прошло, не пускаем
                            throw new AppException("403", $"Ваша учетная запись заблокирована до {blockedUntil:dd.MM.yyyy HH:mm}.");
                        }
                    }

                    // Если все проверки пройдены, создаем сессию
                    var user = new User(reader.GetInt32(0), reader.GetString(1), reader.GetString(3));
                    File.WriteAllText(SessionFile, JsonSerializer.Serialize(user));

                    Console.WriteLine($"\n[УСПЕХ] Вы успешно вошли в систему как {user.Username} (Роль: {user.Role}).");
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
                Console.WriteLine("[ИНФО] Локальный файл сессии очищен.");
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
                var user = JsonSerializer.Deserialize<User>(File.ReadAllText(SessionFile));
                if (user != null)
                {
                    Console.WriteLine($"[ИНФОРМАЦИЯ] Автоматический вход выполнен ({user.Username}).");
                }
                return user;
            }
            catch
            {
                return null;
            }
        }
    }
    #endregion
}