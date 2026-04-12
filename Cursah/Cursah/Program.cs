using Cursah;
using System.Data;
using System.Text.RegularExpressions;

class Program
{
    static string? ConnStr;
    static User? CurrentUser;


    static async Task Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;


        while (true)
        {
            Console.Write(CurrentUser == null ? "guest> " : $"{CurrentUser.Username}> ");
            string? input = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(input)) continue;


        }
    }




    static Dictionary<string, string> Parse(string s)
    {
        var d = new Dictionary<string, string>();
        var parts = s.Split(' ', 2);
        d["cmd"] = parts[0];
        if (parts.Length > 1)
        {
            var sub = Regex.Match(parts[1], @"^\w+");
            if (sub.Success) d["sub"] = sub.Value;
            var ms = Regex.Matches(parts[1], @"-(\w+)\s+(""([^""]*)""|(\S+))");
            foreach (Match m in ms) d[m.Groups[1].Value] = m.Groups[3].Success ? m.Groups[3].Value : m.Groups[4].Value;
        }
        return d;
    }


}