using Cursah;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Text.RegularExpressions;

class Program
{
    static string? ConnStr;
    static User? CurrentUser;

    static AuthService? Auth;
    static NoteService? Notes;
    static WatchdogService? Watchdog;
    static AdminService? Admin;

    static async Task Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
        ConnStr = config.GetConnectionString("DefaultConnection");

        Auth = new(ConnStr!);
        Notes = new(ConnStr!);
        Watchdog = new(ConnStr!);
        Admin = new(ConnStr!);
        Logger.Init(ConnStr!);

        CurrentUser = Auth.LoadSession();

        while (true)
        {
            Console.Write(CurrentUser == null ? "guest> " : $"{CurrentUser.Username}> ");
            string? input = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(input)) continue;

            try { await Execute(input); }
            catch (AppException ex) { Console.WriteLine($"[!] {ex.Message}"); }
            catch (Exception ex) { Logger.Log("ERROR", "Main", ex.Message, CurrentUser?.Id, ex); }
        }
    }

    static async Task Execute(string raw)
    {
        var args = Parse(raw);
        string cmd = args.GetValueOrDefault("cmd", "");
        string sub = args.GetValueOrDefault("sub", "");

        switch (cmd)
        {
            case "login":
                CurrentUser = Auth!.Login(args["u"], args["p"]);
                break;
            case "logout":
                Auth!.Logout(); CurrentUser = null;
                break;

            case "note":
                Check(Roles.User);
                if (sub == "add") Notes!.Add(CurrentUser!.Id, args["t"], args["c"]);
                else if (sub == "edit") Notes!.Edit(CurrentUser!.Id, int.Parse(args["id"]), args["t"], args["c"]);
                else if (sub == "list") Notes!.List(CurrentUser!.Id);
                else if (sub == "del") Notes!.Delete(CurrentUser!.Id, int.Parse(args["id"]));
                break;

            case "user":
                Check(Roles.Admin);
                if (sub == "add") Admin!.CreateUser(args["u"], args["p"], args["r"]);
                else if (sub == "list") Admin!.ListUsers();
                else if (sub == "del") Admin!.DeleteUser(int.Parse(args["id"]));
                break;

            case "watchdog":
                Check(Roles.WatchdogAdmin);
                if (sub == "run") Watchdog!.Start();
                else if (sub == "stop") Watchdog!.Stop();
                else if (sub == "config") Watchdog!.SetConfig(int.Parse(args["i"]));
                break;

            case "help": ShowHelp(); break;
            case "exit": Environment.Exit(0); break;
            default: Console.WriteLine("Неизвестная команда."); break;
        }
    }

    static void Check(string role)
    {
        if (CurrentUser == null) throw new AppException("401", "Нужен логин.");
        if (CurrentUser.Role != role) throw new AppException("403", "Нет прав.");
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

    static void ShowHelp()
    {
        Console.WriteLine("Доступные команды:");
        Console.WriteLine("  user add -u [name] -p [pass] -r [User|Admin|WatchdogAdmin]");
        Console.WriteLine("  user list | user del -id [id]");
        Console.WriteLine("  note add -t \"title\" -c \"text\" | note edit -id [id] -t \"..\" -c \"..\"");
        Console.WriteLine("  watchdog run | stop | config -i [sec]");
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

}

