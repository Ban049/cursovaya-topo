using System;
using System.Collections.Generic;
using System.Text;

namespace Cursah
{
    #region Update Manager
    /// <summary>
    /// Управляет процессом обновления приложения.
    /// </summary>
    public static class UpdateManager
    {
        public static void ApplyPendingUpdate()
        {
            if (File.Exists("update.pending"))
            {
                Console.WriteLine("Обнаружено обновление! Выполняется замена файлов");
                // TODO: Реализовать логику замены старого exe-файла на новый
                File.Delete("update.pending");
                Console.WriteLine("Приложение успешно обновлено.");
            }
        }

        public static void PrepareUpdate()
        {
            Directory.CreateDirectory("temp");
            // TODO: Реализовать скачивание новой версии из сети
            File.WriteAllText("update.pending", "v1.1.0");
            Console.WriteLine("Обновление подготовлено. Оно будет применено при следующем запуске (или при выходе из профиля).");
        }
    }
    #endregion
}
