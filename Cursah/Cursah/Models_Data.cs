using System;
using System.Collections.Generic;
using System.Text;

namespace Cursah
{

        public record User(int Id, string Username, string Role);
        public record Note(int Id, int UserId, string Title, string Content, DateTime CreatedAt);
        public record SystemLog(int Id, string Level, string Module, string Message, DateTime CreatedAt);

        public class AppException : Exception
        {
            public string ErrorCode { get; }
            public int Severity { get; }
            public AppException(string code, string message, int severity = 1) : base(message)
            {
                ErrorCode = code; Severity = severity;
            }
        }    
   
}
