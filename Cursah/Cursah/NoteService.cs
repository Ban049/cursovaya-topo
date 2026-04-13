using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cursah
{
    public class NoteService
    {
        private readonly string _cs;
        public NoteService(string cs) => _cs = cs;

        public void Add(int uid, string title, string content)
        {
            using var conn = new SqlConnection(_cs);
            conn.Open();
            var cmd = new SqlCommand("INSERT INTO Notes (UserId, Title, Content) VALUES (@u, @t, @c)", conn);
            cmd.Parameters.AddWithValue("@u", uid);
            cmd.Parameters.AddWithValue("@t", title);
            cmd.Parameters.AddWithValue("@c", content);
            cmd.ExecuteNonQuery();
            Console.WriteLine("Заметка создана.");
        }

        public void Edit(int uid, int noteId, string title, string content)
        {
            using var conn = new SqlConnection(_cs);
            conn.Open();
            var cmd = new SqlCommand("UPDATE Notes SET Title=@t, Content=@c, UpdatedAt=GETDATE() WHERE Id=@id AND UserId=@u", conn);
            cmd.Parameters.AddWithValue("@t", title);
            cmd.Parameters.AddWithValue("@c", content);
            cmd.Parameters.AddWithValue("@id", noteId);
            cmd.Parameters.AddWithValue("@u", uid);
            if (cmd.ExecuteNonQuery() == 0) throw new AppException("404", "Заметка не найдена.");
            Console.WriteLine("Заметка обновлена.");
        }

        public void List(int uid)
        {
            using var conn = new SqlConnection(_cs);
            conn.Open();
            var cmd = new SqlCommand("SELECT Id, Title, Content FROM Notes WHERE UserId = @u", conn);
            cmd.Parameters.AddWithValue("@u", uid);
            using var r = cmd.ExecuteReader();
            while (r.Read()) 
                Console.WriteLine($"[{r.GetInt32(0)}] {r.GetString(1)} \n Содержимое: {r.GetString(2)}");
        }

        public void Delete(int uid, int noteId)
        {
            using var conn = new SqlConnection(_cs);
            conn.Open();
            var cmd = new SqlCommand("DELETE FROM Notes WHERE Id = @id AND UserId = @u", conn);
            cmd.Parameters.AddWithValue("@id", noteId);
            cmd.Parameters.AddWithValue("@u", uid);
            if (cmd.ExecuteNonQuery() == 0) throw new AppException("404", "Заметка не найдена.");
            Console.WriteLine("Удалено.");
        }
    }
}
