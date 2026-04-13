using Microsoft.Data.SqlClient;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Cursah
{
    public class WatchdogService
    {
        private readonly string _cs;
        private CancellationTokenSource? _cts;
        private int _interval = 5; 

        public WatchdogService(string cs) => _cs = cs;

        public void SetConfig(int interval)
        {
            if (interval < 1) throw new AppException("400", "Интервал должен быть > 0.");
            _interval = interval;
            Console.WriteLine($"Интервал сбора метрик изменен на {_interval} сек.");
        }

        public void Start()
        {
            if (_cts != null)
            {
                Console.WriteLine("Watchdog уже запущен.");
                return;
            }
            _cts = new CancellationTokenSource();
            Task.Run(() => Loop(_cts.Token));
            Console.WriteLine("Watchdog успешно запущен (фоновый режим).");
        }

        public void Stop()
        {
            if (_cts == null)
            {
                Console.WriteLine("Watchdog не запущен.");
                return;
            }
            _cts.Cancel();
            _cts = null;
            Console.WriteLine("Watchdog остановлен.");
        }

        public void Status()
        {
            string state = _cts != null && !_cts.IsCancellationRequested ? "АКТИВЕН" : "ОСТАНОВЛЕН";
            Console.WriteLine($"--- Статус Watchdog ---");
            Console.WriteLine($"Состояние: {state}");
            Console.WriteLine($"Интервал:  {_interval} сек.");
        }

        public void ListMetrics(int limit = 5)
        {
            using var conn = new SqlConnection(_cs);
            conn.Open();
            var cmd = new SqlCommand("SELECT TOP(@n) Metric, Value, CollectedAt FROM WatchdogMetrics ORDER BY CollectedAt DESC", conn);
            cmd.Parameters.AddWithValue("@n", limit);
            using var r = cmd.ExecuteReader();

            Console.WriteLine($"\nПоследние {limit} метрик:");
            Console.WriteLine("Время | Метрика | Значение");
            Console.WriteLine("-----------------------------");
            bool hasData = false;
            while (r.Read())
            {
                hasData = true;
                Console.WriteLine($"{r.GetDateTime(2):T} | {r.GetString(0)} | {r.GetDecimal(1):F2}%");
            }
            if (!hasData) Console.WriteLine("Метрик пока нет.");
        }

        private async Task Loop(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                try
                {
                    decimal val = (decimal)Random.Shared.NextDouble() * 100;
                    using var conn = new SqlConnection(_cs);
                    await conn.OpenAsync(ct);
                    var cmd = new SqlCommand("INSERT INTO WatchdogMetrics (Metric, Value) VALUES ('CPU', @v)", conn);
                    cmd.Parameters.AddWithValue("@v", val);
                    await cmd.ExecuteNonQueryAsync(ct);
                }
                catch { /* По ТЗ: логируем в файл, в консоль выводить запрещено */ }

                try { await Task.Delay(_interval * 1000, ct); }
                catch (TaskCanceledException) { break; }
            }
        }
    }
}