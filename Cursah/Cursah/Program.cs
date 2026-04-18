using Cursah;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Text.RegularExpressions;

class Program
{
    #region Global Variables

    private static string? connStr;
    private static User? currentUser;

    private static AuthService? auth;
    private static NoteService? notes;
    private static WatchdogService? watchdog;
    private static AdminService? admin;
    #endregion

    #region Main Entry Point
    static async Task Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        UpdateManager.ApplyPendingUpdate();

        var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
        connStr = config.GetConnectionString("DefaultConnection");

        auth = new AuthService(connStr!);
        notes = new NoteService(connStr!);
        watchdog = new WatchdogService(connStr!);
        admin = new AdminService(connStr!);
        Logger.Init(connStr!);

        currentUser = auth.LoadSession();

        while (true)
        {
            Console.Write(currentUser == null ? "guest> " : $"{currentUser.Username}> ");
            string? input = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(input))
            {
                continue;
            }

            try
            {
                await Execute(input);
            }
            catch (AppException ex)
            {
                Console.WriteLine($"[!] {ex.Message}");
            }
            catch (Exception ex)
            {
                Logger.Log("ERROR", "Main", ex.Message, currentUser?.Id, ex);
            }
        }
    }
    #endregion

    #region Command Execution Logic
    /// <summary>
    /// Обрабатывает введенную пользователем команду.
    /// </summary>
    /// <param name="raw">Строка ввода из консоли</param>
    static async Task Execute(string raw)
    {
        var args = Parse(raw);
        string cmd = args.GetValueOrDefault("cmd", "");
        string sub = args.GetValueOrDefault("sub", "");

        switch (cmd)
        {
            case "login":
                currentUser = auth!.Login(args.GetValueOrDefault("u", ""), args.GetValueOrDefault("p", ""));
                Console.WriteLine($"Добро пожаловать, {currentUser.Username}!");
                break;

            case "logout":
                auth!.Logout();
                currentUser = null;
                Console.WriteLine("Вы вышли из системы.");

                if (File.Exists("update.pending"))
                {
                    Console.WriteLine("Ожидает установка обновления. Завершение работы...");
                    Environment.Exit(0);
                }
                break;

            case "note":
                Check(Roles.User);
                if (sub == "add")
                    notes!.Add(currentUser!.Id, args["t"], args["c"]);
                else if (sub == "edit")
                    notes!.Edit(currentUser!.Id, int.Parse(args["id"]), args["t"], args["c"]);
                else if (sub == "list")
                    notes!.List(currentUser!.Id);
                else if (sub == "del")
                    notes!.Delete(currentUser!.Id, int.Parse(args["id"]));
                break;

            case "user":
                Check(Roles.Admin);
                if (sub == "add")
                    admin!.CreateUser(args["u"], args["p"], args["r"]);
                else if (sub == "list")
                    admin!.ListUsers();
                else if (sub == "del")
                    admin!.DeleteUser(int.Parse(args["id"]));
                else if (sub == "block")
                    admin!.BlockUser(int.Parse(args["id"]), int.Parse(args["d"]));
                break;

            case "system":
                Check(Roles.Admin);
                if (sub == "logs") ViewLogs(args);
                else if (sub == "update") UpdateManager.PrepareUpdate();
                break;

            case "watchdog":
                Check(Roles.WatchdogAdmin);
                if (sub == "run") watchdog!.Start();
                else if (sub == "stop") watchdog!.Stop();
                else if (sub == "config") watchdog!.SetConfig(int.Parse(args["i"]));
                else if (sub == "status") watchdog!.Status();
                else if (sub == "list") watchdog!.ListMetrics(args.ContainsKey("n") ? int.Parse(args["n"]) : 9);
                break;

            case "help":
                ShowHelp();
                break;

            case "exit":
                Environment.Exit(0);
                break;

            default:
                Console.WriteLine("Неизвестная команда. Введите help для справки.");
                break;
        }
    }
    #endregion

    #region Helpers
    /// <summary>
    /// Проверяет наличие прав у текущего пользователя.
    /// </summary>
    /// <param name="role">Требуемая роль</param>
    static void Check(string role)
    {
        if (currentUser == null)
        {
            throw new AppException("401", "Нужен логин.");
        }
        if (currentUser.Role != role)
        {
            throw new AppException("403", "Нет прав для выполнения этой команды.");
        }
    }

    /// <summary>
    /// Разбирает строку ввода в словарь аргументов.
    /// </summary>
    static Dictionary<string, string> Parse(string s)
    {
        var dictionary = new Dictionary<string, string>();
        var parts = s.Split(' ', 2);
        dictionary["cmd"] = parts[0];

        if (parts.Length > 1)
        {
            var sub = Regex.Match(parts[1], @"^\w+");
            if (sub.Success)
            {
                dictionary["sub"] = sub.Value;
            }

            var ms = Regex.Matches(parts[1], @"-(\w+)\s+(""([^""]*)""|(\S+))");
            foreach (Match m in ms)
            {
                dictionary[m.Groups[1].Value] = m.Groups[3].Success ? m.Groups[3].Value : m.Groups[4].Value;
            }
        }
        return dictionary;
    }

    /// <summary>
    /// Выводит доступные команды
    /// </summary>
    static void ShowHelp()
    {
        Console.WriteLine("Доступные команды для всех:");
        Console.WriteLine("  login -u [Username] -p [Password] (Войти в систему)");
        Console.WriteLine("  logout (Выход из сессии)");
        Console.WriteLine("  help (Вывести список команд)");
        Console.WriteLine("  exit (Выход из приложения)");
        Console.WriteLine("Доступные команды для пользователей:");
        Console.WriteLine("  note list (Вывести список заметок)");
        Console.WriteLine($"  note add -t {"Заголовок"} -c {"Содержимое"} (Добавить заметку)");
        Console.WriteLine($"  note edit -id [ID] -t {"Заголовок"} -c {"Содержимое"} (Редактировать существующую заметку)");
        Console.WriteLine("  note del -id [id] (Удалить заметку)");
        Console.WriteLine("Доступные команды для Администраторов:");
        Console.WriteLine("  user add -u [name] -p [pass] -r [User|Admin|WatchdogAdmin] (Добавить пользователя)");
        Console.WriteLine("  user list (Вывести список пользователей)");
        Console.WriteLine("  user del -id [id] (Удалить пользователя)");
        Console.WriteLine("  user block -id [id] -d [дни] (Заблокировать пользователя на N дней)");
        Console.WriteLine("  system logs -n [кол-во] (Посмотреть последние логи системы)");
        Console.WriteLine("  system update (Подготовка системы к обновлению)");
        Console.WriteLine("Доступные команды для WatchdogAdmin:");
        Console.WriteLine("  watchdog run (Запуск фонового сбора метрик CPU)");
        Console.WriteLine("  watchdog stop (Остановка сбора метрик)");
        Console.WriteLine("  watchdog config -i [секунды] (Установка интервала сбора данных)");
        Console.WriteLine("  watchdog status (Проверка состояния модуля)");
    }

    /// <summary>
    /// Выводит системные логи из БД.
    /// </summary>
    static void ViewLogs(Dictionary<string, string> args)
    {
        int limit = int.TryParse(args.GetValueOrDefault("n", "5"), out var val) ? val : 5;

        using var conn = new SqlConnection(connStr);
        conn.Open();

        var cmd = new SqlCommand("SELECT TOP(@n) CreatedAt, Level, Message FROM SystemLogs ORDER BY CreatedAt DESC", conn);
        cmd.Parameters.AddWithValue("@n", limit);

        using var reader = cmd.ExecuteReader();
        Console.WriteLine("\nДата/Время | Уровень | Сообщение");
        Console.WriteLine("----------------------------------");
        while (reader.Read())
        {
            Console.WriteLine($"{reader.GetDateTime(0):G} | [{reader.GetString(1)}] | {reader.GetString(2)}");
        }
    }
    #endregion
}