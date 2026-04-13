using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cursah
{
    #region Note Service
    /// <summary>
    /// Сервис управления текстовыми заметками.
    /// </summary>
    public class NoteService
    {
        private readonly string _connectionString;

        public NoteService(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Добавление заметки.
        /// </summary>
        public void Add(int userId, string title, string content)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            var cmd = new SqlCommand("INSERT INTO Notes (UserId, Title, Content) VALUES (@u, @t, @c)", conn);
            cmd.Parameters.AddWithValue("@u", userId);
            cmd.Parameters.AddWithValue("@t", title);
            cmd.Parameters.AddWithValue("@c", content);
            cmd.ExecuteNonQuery();
            Console.WriteLine("Заметка создана.");
        }

        /// <summary>
        /// Редактирование заметки.
        /// </summary>
        public void Edit(int userId, int noteId, string title, string content)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            var cmd = new SqlCommand("UPDATE Notes SET Title=@t, Content=@c, UpdatedAt=GETDATE() WHERE Id=@id AND UserId=@u", conn);
            cmd.Parameters.AddWithValue("@t", title);
            cmd.Parameters.AddWithValue("@c", content);
            cmd.Parameters.AddWithValue("@id", noteId);
            cmd.Parameters.AddWithValue("@u", userId);

            if (cmd.ExecuteNonQuery() == 0)
            {
                throw new AppException("404", "Заметка не найдена.");
            }
            Console.WriteLine("Заметка обновлена.");
        }

        /// <summary>
        /// Вывод списка заметок
        /// </summary>
        public void List(int userId)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            var cmd = new SqlCommand("SELECT Id, Title, Content FROM Notes WHERE UserId = @u", conn);
            cmd.Parameters.AddWithValue("@u", userId);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                Console.WriteLine($"[{reader.GetInt32(0)}] {reader.GetString(1)} \n Содержимое: {reader.GetString(2)}");
            }
        }

        /// <summary>
        /// Удаление заметки.
        /// </summary>
        public void Delete(int userId, int noteId)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            var cmd = new SqlCommand("DELETE FROM Notes WHERE Id = @id AND UserId = @u", conn);
            cmd.Parameters.AddWithValue("@id", noteId);
            cmd.Parameters.AddWithValue("@u", userId);

            if (cmd.ExecuteNonQuery() == 0)
            {
                throw new AppException("404", "Заметка не найдена.");
            }
            Console.WriteLine("Удалено.");
        }
    }
    #endregion
}
