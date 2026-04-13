using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Text;

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

            Console.WriteLine($"Пользователь {username} создан.");
        }
        /// <summary>
        /// Вывод списка пользователей.
        /// </summary>
        public void ListUsers()
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            var cmd = new SqlCommand("SELECT Id, Username, Role FROM Users", conn);
            using var reader = cmd.ExecuteReader();

            Console.WriteLine("\nID | Username | Role");
            while (reader.Read())
            {
                Console.WriteLine($"{reader.GetInt32(0)} | {reader.GetString(1)} | {reader.GetString(2)}");
            }
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
            Console.WriteLine("Пользователь удален.");
        }
    }
    #endregion
}
