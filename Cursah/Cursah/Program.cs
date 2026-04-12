using Microsoft.Data.SqlClient;

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