using Microsoft.Data.SqlClient;
using System;

namespace Cursah
{
    #region Admin Service
    /// <summary>
    /// Сервис для административных задач (управление пользователями).
    /// </summary>
    public class AdminService
    {
        private readonly string _connectionString;

        public AdminService(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Создание пользователя.
        /// </summary>
        public void CreateUser(string username, string password, string role)
        {
            if (role != Roles.User && role != Roles.Admin && role != Roles.WatchdogAdmin)
            {
                throw new AppException("400", "Невалидная роль.");
            }

            string hash = BCrypt.Net.BCrypt.HashPassword(password);

            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            var cmd = new SqlCommand("INSERT INTO Users (Username, PasswordHash, Role) VALUES (@u, @h, @r)", conn);
            cmd.Parameters.AddWithValue("@u", username);
            cmd.Parameters.AddWithValue("@h", hash);
            cmd.Parameters.AddWithValue("@r", role);
            cmd.ExecuteNonQuery();

            Console.WriteLine($"[УСПЕХ] Пользователь '{username}' с ролью '{role}' успешно создан.");
        }

        /// <summary>
        /// Вывод списка пользователей.
        /// </summary>
        public void ListUsers()
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            var cmd = new SqlCommand("SELECT Id, Username, Role, BlockedUntil FROM Users", conn);
            using var reader = cmd.ExecuteReader();

            Console.WriteLine("\nID | Username | Role | Статус");
            Console.WriteLine("-------------------------------------------------");

            bool hasUsers = false;
            while (reader.Read())
            {
                hasUsers = true;
                string status = "Активен";
                if (!reader.IsDBNull(3))
                {
                    DateTime blockedUntil = reader.GetDateTime(3);
                    if (blockedUntil > DateTime.Now)
                    {
                        status = $"Заблокирован до {blockedUntil:dd.MM.yyyy}";
                    }
                }

                Console.WriteLine($"{reader.GetInt32(0)} | {reader.GetString(1)} | {reader.GetString(2)} | {status}");
            }

            if (!hasUsers)
            {
                Console.WriteLine("Список пользователей пуст.");
            }
            Console.WriteLine("-------------------------------------------------\n");
        }

        /// <summary>
        /// Удаление пользователя.
        /// </summary>
        public void DeleteUser(int id)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            var cmd = new SqlCommand("DELETE FROM Users WHERE Id = @id", conn);
            cmd.Parameters.AddWithValue("@id", id);

            int rows = cmd.ExecuteNonQuery();
            if (rows == 0)
            {
                throw new AppException("404", "Пользователь не найден.");
            }
            Console.WriteLine($"[УСПЕХ] Пользователь с ID {id} успешно удален из системы.");
        }

        /// <summary>
        /// Блокировка пользователя на указанное количество дней.
        /// </summary>
        public void BlockUser(int id, int days)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            // DATEADD прибавляет указанное количество дней к текущей дате
            var cmd = new SqlCommand("UPDATE Users SET BlockedUntil = DATEADD(day, @d, GETDATE()) WHERE Id = @id", conn);
            cmd.Parameters.AddWithValue("@d", days);
            cmd.Parameters.AddWithValue("@id", id);

            int rows = cmd.ExecuteNonQuery();
            if (rows == 0)
            {
                throw new AppException("404", "Пользователь не найден.");
            }

            Console.WriteLine($"[УСПЕХ] Учетная запись (ID: {id}) успешно заблокирована на {days} дней.");
        }

        /// <summary>
        /// Разблокировка пользователя (снятие временной блокировки).
        /// </summary>
        public void UnblockUser(int id)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            var cmd = new SqlCommand("UPDATE Users SET BlockedUntil = NULL WHERE Id = @id", conn);
            cmd.Parameters.AddWithValue("@id", id);

            int rows = cmd.ExecuteNonQuery();
            if (rows == 0)
            {
                throw new AppException("404", "Пользователь не найден.");
            }

            Console.WriteLine($"Учетная запись (ID: {id}) успешно разблокирована.");
        }
    }
    #endregion
}