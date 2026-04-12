using System;
using System.Collections.Generic;
using System.Text;

namespace Cursah
{
    public static class UpdateManager
    {
        public static void ApplyPendingUpdate()
        {
            if (File.Exists("update.pending"))
            {
                Console.WriteLine("Обнаружено обновление! Выполняется замена файлов");
                File.Delete("update.pending");
                Console.WriteLine("Приложение обновлено.");
            }
        }

        public static void PrepareUpdate()
        {
            Directory.CreateDirectory("temp");
            File.WriteAllText("update.pending", "v1.1.0");
            Console.WriteLine("Обновление подготовлено. Оно будет применено при следующем запуске.");
        }
    }
}
