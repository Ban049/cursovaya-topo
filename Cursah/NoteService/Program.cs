using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace CursahLauncher
{
    class Program
    {
        #region Constants
        private const string NoteBookFolder = @"D:\Учёба\3 курс\2 сем\Тестирование и отладка ПО\Курсач\NoteService";
        private const string ExecutableName = "Cursah.exe";
        private const string CurrentVersionFile = "current_version.txt";
        #endregion

        #region Main Entry Point
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.WriteLine("=== Cursah Launcher ===");

            if (!Directory.Exists(NoteBookFolder))
            {
                Directory.CreateDirectory(NoteBookFolder);
                Console.WriteLine($"Папка {NoteBookFolder} не найдена. Создана пустая папка.");
                Console.WriteLine("Пожалуйста, поместите туда папку с версией программы (например, v-1.0.0).");
                Console.ReadLine();
                return;
            }

            string targetFolderPath = GetLatestVersionFolder();

            if (string.IsNullOrEmpty(targetFolderPath))
            {
                Console.WriteLine("Не найдено ни одной версии программы в папке NoteBook.");
                Console.ReadLine();
                return;
            }

            string targetExePath = Path.Combine(targetFolderPath, ExecutableName);
            string latestVersionName = new DirectoryInfo(targetFolderPath).Name;

            CheckForUpdatesAndLaunch(latestVersionName, targetExePath);
        }
        #endregion

        #region Core Logic
        /// <summary>
        /// Ищет папку с самой новой версией (v-X.X.X) внутри папки NoteBook.
        /// </summary>
        static string GetLatestVersionFolder()
        {
            string[] directories = Directory.GetDirectories(NoteBookFolder);
            Version maxVersion = new Version(0, 0, 0);
            string latestFolderPath = string.Empty;

            foreach (string dir in directories)
            {
                string folderName = new DirectoryInfo(dir).Name;

                if (folderName.StartsWith("v-"))
                {
                    string versionString = folderName.Substring(2); 

                    if (Version.TryParse(versionString, out Version parsedVersion))
                    {
                        if (parsedVersion.CompareTo(maxVersion) > 0)
                        {
                            maxVersion = parsedVersion;
                            latestFolderPath = dir;
                        }
                    }
                }
            }

            return latestFolderPath;
        }

        /// <summary>
        /// Проверяет, является ли версия новой, имитирует обновление и запускает программу.
        /// </summary>
        static void CheckForUpdatesAndLaunch(string latestVersionName, string targetExePath)
        {
            string currentVersion = string.Empty;

            // Читаем текущую (последнюю запущенную) версию из файла
            if (File.Exists(CurrentVersionFile))
            {
                currentVersion = File.ReadAllText(CurrentVersionFile).Trim();
            }

            // Если версия обновилась
            if (currentVersion != latestVersionName)
            {
                Console.WriteLine($"Обнаружена новая версия: {latestVersionName}");
                Console.WriteLine("Выполняется обновление файлов...");

                Thread.Sleep(10000);

                File.WriteAllText(CurrentVersionFile, latestVersionName);
                Console.WriteLine("Обновление завершено успешно!");
            }
            else
            {
                Console.WriteLine($"У вас установлена актуальная версия: {latestVersionName}");
            }

            Console.WriteLine("Запуск программы...");

            if (File.Exists(targetExePath))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = targetExePath,
                    UseShellExecute = true
                });
            }
            else
            {
                Console.WriteLine($"[ОШИБКА] Исполняемый файл не найден по пути: {targetExePath}");
                Console.ReadLine();
            }
        }
        #endregion
    }
}