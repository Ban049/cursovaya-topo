using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Text;

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
            Console.WriteLine($"Интервал изменен на {_interval} сек.");
        }

        public void Start()
        {
            if (_cts != null) return;
            _cts = new CancellationTokenSource();
            Task.Run(() => Loop(_cts.Token));
            Console.WriteLine("Watchdog запущен.");
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
                catch { /* Логируем в файл */ }
                await Task.Delay(_interval * 1000, ct);
            }
        }

        public void Stop() { _cts?.Cancel(); _cts = null; Console.WriteLine("Остановлен."); }
    }
}
