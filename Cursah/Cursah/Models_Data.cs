using System;
using System.Collections.Generic;
using System.Text;

namespace Cursah
{

    public static class Roles
    {
        public const string User = "User";
        public const string Admin = "Admin";
        public const string WatchdogAdmin = "WatchdogAdmin";
    }

    public record User(int Id, string Username, string Role);

    public class AppException : Exception
    {
        public string ErrorCode { get; }
        public AppException(string code, string message) : base(message) => ErrorCode = code;
    }

}
