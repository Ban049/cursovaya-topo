using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using BCrypt.Net;

namespace Cursah
{
    public class AuthService
    {
        private readonly string _cs;
        private const string SessionFile = ".session";

        public AuthService(string cs) => _cs = cs;

        public User Login(string username, string password)
        {
            using var conn = new SqlConnection(_cs);
            conn.Open();
            var cmd = new SqlCommand("SELECT Id, Username, PasswordHash, Role FROM Users WHERE Username = @u", conn);
            cmd.Parameters.AddWithValue("@u", username);

            using var r = cmd.ExecuteReader();
            if (r.Read() && BCrypt.Net.BCrypt.Verify(password, r.GetString(2)))
            {
                var user = new User(r.GetInt32(0), r.GetString(1), r.GetString(3));
                File.WriteAllText(SessionFile, JsonSerializer.Serialize(user));
                return user;
            }
            throw new AppException("401", "Неверное имя пользователя или пароль.");
        }

        public void Logout() { if (File.Exists(SessionFile)) File.Delete(SessionFile); }

        public User? LoadSession()
        {
            if (!File.Exists(SessionFile)) return null;
            try { return JsonSerializer.Deserialize<User>(File.ReadAllText(SessionFile)); }
            catch { return null; }
        }
    }
}
