using Microsoft.Data.SqlClient;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Cursah
{
    #region Watchdog Service
    /// <summary>
    /// Сервис фонового мониторинга системных ресурсов.
    /// </summary>
    public class WatchdogService
    {
        private readonly string _connectionString;
        private CancellationTokenSource? _cancellationTokenSource;
        private int _interval = 5;

        // Счетчики производительности Windows
        private PerformanceCounter? _cpuCounter;
        private PerformanceCounter? _ramCounter;

        public WatchdogService(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Настройка интервала сбора данных.
        /// </summary>
        public void SetConfig(int interval)
        {
            if (interval < 1)
            {
                throw new AppException("400", "Интервал должен быть > 0.");
            }
            _interval = interval;
            Console.WriteLine($"Интервал сбора метрик изменен на {_interval} сек.");
        }

        /// <summary>
        /// Запуск сбора данных.
        /// </summary>
        public void Start()
        {
            if (_cancellationTokenSource != null)
            {
                Console.WriteLine("Watchdog уже запущен.");
                return;
            }

            try
            {
                if (OperatingSystem.IsWindows())
                {
                    _cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
                    _ramCounter = new PerformanceCounter("Memory", "% Committed Bytes In Use");

                    _cpuCounter.NextValue();
                    _ramCounter.NextValue();
                }
                else
                {
                    Console.WriteLine("[Внимание] Cбор CPU/RAM поддерживается только в ОС Windows.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[!] Ошибка инициализации счетчиков производительности: {ex.Message}");
            }

            _cancellationTokenSource = new CancellationTokenSource();
            Task.Run(() => Loop(_cancellationTokenSource.Token));

            Console.WriteLine("Watchdog успешно запущен (фоновый сбор CPU, RAM, HDD).");
        }

        /// <summary>
        /// Остановка сбора данных.
        /// </summary>
        public void Stop()
        {
            if (_cancellationTokenSource == null)
            {
                Console.WriteLine("Watchdog не запущен.");
                return;
            }
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource = null;

            // Освобождаем ресурсы счетчиков
            _cpuCounter?.Dispose();
            _ramCounter?.Dispose();
            _cpuCounter = null;
            _ramCounter = null;

            Console.WriteLine("Watchdog остановлен.");
        }

        /// <summary>
        /// Проверка статуса системы сбора данных.
        /// </summary>
        public void Status()
        {
            string state = _cancellationTokenSource != null && !_cancellationTokenSource.IsCancellationRequested ? "АКТИВЕН" : "ОСТАНОВЛЕН";
            Console.WriteLine($"--- Статус Watchdog ---");
            Console.WriteLine($"Состояние: {state}");
            Console.WriteLine($"Интервал:  {_interval} сек.");
            Console.WriteLine($"Метрики:   CPU, RAM, HDD");
        }

        /// <summary>
        /// Вывод собранных метрик.
        /// </summary>
        public void ListMetrics(int limit = 9)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            var cmd = new SqlCommand("SELECT TOP(@n) Metric, Value, CollectedAt FROM WatchdogMetrics ORDER BY CollectedAt DESC", conn);
            cmd.Parameters.AddWithValue("@n", limit);

            using var reader = cmd.ExecuteReader();

            Console.WriteLine($"\nПоследние {limit} записей метрик:");
            Console.WriteLine("Время       | Метрика | Значение");
            Console.WriteLine("--------------------------------");

            bool hasData = false;
            while (reader.Read())
            {
                hasData = true;
                Console.WriteLine($"{reader.GetDateTime(2):HH:mm:ss}    | {reader.GetString(0),-7} | {reader.GetDecimal(1):F2}%");
            }

            if (!hasData)
            {
                Console.WriteLine("Метрик пока нет.");
            }
        }

        /// <summary>
        /// Запись метрик в базу данных.
        /// </summary>
        private async Task Loop(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    decimal cpuVal = 0;
                    decimal ramVal = 0;
                    decimal hddVal = 0;

                    if (OperatingSystem.IsWindows() && _cpuCounter != null && _ramCounter != null)
                    {
                        cpuVal = (decimal)_cpuCounter.NextValue();
                        ramVal = (decimal)_ramCounter.NextValue();
                    }

                    string driveName = Path.GetPathRoot(Environment.CurrentDirectory) ?? "C:\\";
                    var driveInfo = new DriveInfo(driveName);

                    if (driveInfo.IsReady)
                    {
                        double totalSpace = driveInfo.TotalSize;
                        double freeSpace = driveInfo.AvailableFreeSpace;
                        hddVal = (decimal)(((totalSpace - freeSpace) / totalSpace) * 100);
                    }

                    using var conn = new SqlConnection(_connectionString);
                    await conn.OpenAsync(token);

                    var cmd = new SqlCommand(@"
                        INSERT INTO WatchdogMetrics (Metric, Value) VALUES 
                        ('CPU', @cpu),
                        ('RAM', @ram),
                        ('HDD', @hdd)", conn);

                    cmd.Parameters.AddWithValue("@cpu", cpuVal);
                    cmd.Parameters.AddWithValue("@ram", ramVal);
                    cmd.Parameters.AddWithValue("@hdd", hddVal);

                    await cmd.ExecuteNonQueryAsync(token);
                }
                catch (Exception ex)
                {
                    Logger.Log("ERROR", "Watchdog", "Ошибка при сборе метрик", null, ex);
                }

                try
                {
                    await Task.Delay(_interval * 1000, token);
                }
                catch (TaskCanceledException)
                {
                    break;
                }
            }
        }
    }
    #endregion
}