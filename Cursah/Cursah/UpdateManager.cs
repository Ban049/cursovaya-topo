using System;
using System.IO;

namespace Cursah
{
    #region Update Manager
    /// <summary>
    /// Управляет процессом подготовки системы к обновлению.
    /// </summary>
    public static class UpdateManager
    {
        private const string BaseUpdateFolder = @"D:\Учёба\3 курс\2 сем\Тестирование и отладка ПО\Курсач\NoteService";

        /// <summary>
        /// Сканирует существующие версии, вычисляет новую и создает для нее папку.
        /// </summary>
        public static void PrepareUpdate()
        {
            try
            {
                if (!Directory.Exists(BaseUpdateFolder))
                {
                    Directory.CreateDirectory(BaseUpdateFolder);
                }

                // Получаем название следующей версии
                string nextVersionName = GetNextVersionName();
                string newVersionDir = Path.Combine(BaseUpdateFolder, nextVersionName);

                Directory.CreateDirectory(newVersionDir);

                Console.WriteLine("--------------------------------------------------");
                Console.WriteLine($"[СИСТЕМА] Подготовлена папка для новой версии: {nextVersionName}");
                Console.WriteLine($"Путь: {newVersionDir}");
                Console.WriteLine("Действия для обновления:");
                Console.WriteLine("1. Скомпилируйте новую версию проекта.");
                Console.WriteLine("2. Вручную скопируйте новый Cursah.exe в созданную папку.");
                Console.WriteLine("3. Закройте эту программу (команда exit).");
                Console.WriteLine("4. Запустите Лаунчер.");
                Console.WriteLine("--------------------------------------------------");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ОШИБКА] Не удалось создать папку обновления: {ex.Message}");
            }
        }

        /// <summary>
        /// Ищет максимальную версию в папке и возвращает версию.
        /// </summary>
        private static string GetNextVersionName()
        {
            string[] directories = Directory.GetDirectories(BaseUpdateFolder);

            Version maxVersion = new Version(0, 0, 0);
            bool foundAny = false;

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
                            foundAny = true;
                        }
                    }
                }
            }

            // Если папок с версиями вообще нет, возвращаем стартовую 1.0.0
            if (!foundAny)
            {
                return "v-1.0.0";
            }

            int major = maxVersion.Major == -1 ? 1 : maxVersion.Major;
            int minor = maxVersion.Minor == -1 ? 0 : maxVersion.Minor;
            int build = maxVersion.Build == -1 ? 0 : maxVersion.Build;

            Version nextVersion = new Version(major, minor, build + 1);

            return "v-" + nextVersion.ToString(3);
        }
    }
    #endregion
}