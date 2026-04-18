using Microsoft.Data.SqlClient;
using System;

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

            Console.WriteLine($"[УСПЕХ] Заметка '{title}' успешно создана.");
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
                throw new AppException("404", "Заметка не найдена или у вас нет прав на ее редактирование.");
            }

            Console.WriteLine($"[УСПЕХ] Заметка (ID: {noteId}) успешно обновлена.");
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
            bool hasNotes = false;

            Console.WriteLine("\n--- Ваши заметки ---");
            while (reader.Read())
            {
                hasNotes = true;
                Console.WriteLine($"[ID: {reader.GetInt32(0)}] {reader.GetString(1)}");
                Console.WriteLine($"Содержимое: {reader.GetString(2)}");
                Console.WriteLine("--------------------");
            }

            if (!hasNotes)
            {
                Console.WriteLine("У вас пока нет ни одной заметки. Используйте 'note add', чтобы создать первую.");
                Console.WriteLine("--------------------");
            }
            Console.WriteLine(); // Пустая строка для красоты вывода
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
                throw new AppException("404", "Заметка не найдена или у вас нет прав на ее удаление.");
            }

            Console.WriteLine($"[УСПЕХ] Заметка (ID: {noteId}) успешно удалена.");
        }
    }
    #endregion
}