using Cursah;
using Microsoft.Data.SqlClient;
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


static void ViewLogs(Dictionary<string, string> args)
{
    int limit = int.TryParse(args.GetValueOrDefault("n", "5"), out var val) ? val : 5;
    using var conn = new SqlConnection(ConnStr);
    conn.Open();
    var cmd = new SqlCommand("SELECT TOP(@n) CreatedAt, Level, Message FROM SystemLogs ORDER BY CreatedAt DESC", conn);
    cmd.Parameters.AddWithValue("@n", limit);
    using var r = cmd.ExecuteReader();
    while (r.Read()) Console.WriteLine($"{r.GetDateTime(0):T} [{r.GetString(1)}] {r.GetString(2)}");

}