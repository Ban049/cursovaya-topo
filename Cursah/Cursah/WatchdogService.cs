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
        private int _interval = 5; // По умолчанию 5 сек

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
            Console.WriteLine("Watchdog успешно запущен (фоновый сбор CPU, RAM, HDD).");
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
            Console.WriteLine($"Метрики:   CPU, RAM, HDD");
        }

        public void ListMetrics(int limit = 9) 
        {
            using var conn = new SqlConnection(_cs);
            conn.Open();
            // Выбираем последние N записей
            var cmd = new SqlCommand("SELECT TOP(@n) Metric, Value, CollectedAt FROM WatchdogMetrics ORDER BY CollectedAt DESC", conn);
            cmd.Parameters.AddWithValue("@n", limit);
            using var r = cmd.ExecuteReader();

            Console.WriteLine($"\nПоследние {limit} записей метрик:");
            Console.WriteLine("Время       | Метрика | Значение");
            Console.WriteLine("--------------------------------");
            bool hasData = false;
            while (r.Read())
            {
                hasData = true;
                Console.WriteLine($"{r.GetDateTime(2):HH:mm:ss}    | {r.GetString(0),-7} | {r.GetDecimal(1):F2}%");
            }
            if (!hasData) Console.WriteLine("Метрик пока нет.");
        }

        private async Task Loop(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                try
                {

                    decimal cpuVal = (decimal)Random.Shared.NextDouble() * 100; // От 0 до 100%
                    decimal ramVal = (decimal)(Random.Shared.Next(30, 85) + Random.Shared.NextDouble()); // Занято 30-85% ОЗУ
                    decimal hddVal = (decimal)(Random.Shared.Next(10, 60) + Random.Shared.NextDouble()); // Занято 10-60% Диска

                    using var conn = new SqlConnection(_cs);
                    await conn.OpenAsync(ct);

                    var cmd = new SqlCommand(@"
                        INSERT INTO WatchdogMetrics (Metric, Value) VALUES 
                        ('CPU', @cpu),
                        ('RAM', @ram),
                        ('HDD', @hdd)", conn);

                    cmd.Parameters.AddWithValue("@cpu", cpuVal);
                    cmd.Parameters.AddWithValue("@ram", ramVal);
                    cmd.Parameters.AddWithValue("@hdd", hddVal);

                    await cmd.ExecuteNonQueryAsync(ct);
                }
                catch (Exception ex)
                {
                    Logger.Log("ERROR", "Watchdog", "Ошибка при сборе метрик", null, ex);
                }

                try { await Task.Delay(_interval * 1000, ct); }
                catch (TaskCanceledException) { break; } 
            }
        }
    }
}