using System;

namespace Cursah
{
    #region Constants
    /// <summary>
    /// Содержит константы ролей пользователей в системе.
    /// </summary>
    public static class Roles
    {
        public const string User = "User";
        public const string Admin = "Admin";
        public const string WatchdogAdmin = "WatchdogAdmin";
    }
    #endregion

    #region Models
    /// <summary>
    /// Класс, описывающий текущего пользователя сессии.
    /// </summary>
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Role { get; set; }

        public User(int id, string username, string role)
        {
            Id = id;
            Username = username;
            Role = role;
        }
    }
    #endregion

    #region Exceptions
    /// <summary>
    /// Кастомный класс исключений для обработки бизнес-логики приложения.
    /// </summary>
    public class AppException : Exception
    {
        public string ErrorCode { get; }

        public AppException(string code, string message) : base(message)
        {
            ErrorCode = code;
        }
    }
    #endregion
}