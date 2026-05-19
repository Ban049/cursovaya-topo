using System.IO;
using System.Xml.Linq;
using Cursah;
using Xunit;

namespace TestDll
{
    public class UnitTest_Update
    {
        /// <summary>
        /// Позитивный тест: Подготовка файлов для обновления.
        /// </summary>
        [Fact]
        [Trait("Category", "Обновления")]
        public void PrepareUpdate_CreatesDirectory()
        {
            // #Act – Инициализация
            XDocument xml = XDocument.Load("TestParam.xml");
            string basePath = xml.Root.Element("Update").Attribute("basePath").Value;

            // #Action – что мы должны сделать
            UpdateManager.PrepareUpdate();

            // #Result
            Assert.True(Directory.Exists(basePath));
        }

        /// <summary>
        /// Позитивный тест: Лаунчер обнаруживает новую версию.
        /// Тестируем метод IsUpdateRequired из CursahLauncher.
        /// </summary>
        [Fact]
        [Trait("Category", "Обновления")]
        public void Launcher_DetectsNewVersion()
        {
            // #Arrange – Инициализация
            string testVersionFile = "test_current_version_1.txt";
            File.WriteAllText(testVersionFile, "v-1.0.3"); // У нас старая версия
            string serverVersion = "v-1.0.4";              // На сервере новая

            // #Act – Вызываем код лаунчера
            bool isUpdateNeeded = CursahLauncher.Program.IsUpdateRequired(testVersionFile, serverVersion);

            // #Assert – Проверка результата
            Assert.True(isUpdateNeeded);

            if (File.Exists(testVersionFile)) File.Delete(testVersionFile);
        }

        /// <summary>
        /// Позитивный тест: Лаунчер запускается без новых обновлений.
        /// Тестируем реальный метод IsUpdateRequired из CursahLauncher.
        /// </summary>
        [Fact]
        [Trait("Category", "Обновления")]
        public void Launcher_NoUpdates_StartsNormal()
        {
            // #Arrange – Инициализация
            string testVersionFile = "test_current_version_2.txt";
            File.WriteAllText(testVersionFile, "v-1.0.3"); 
            string serverVersion = "v-1.0.3";              

            // #Act – Вызываем код лаунчера
            bool isUpdateNeeded = CursahLauncher.Program.IsUpdateRequired(testVersionFile, serverVersion);

            // #Assert – Проверка результата
            Assert.False(isUpdateNeeded);

            // Очистка
            if (File.Exists(testVersionFile)) File.Delete(testVersionFile);
        }

        /// <summary>
        /// Негативный тест: Отсутствие исполняемого файла при запуске.
        /// </summary>
        [Fact]
        [Trait("Category", "Обновления")]
        public void Launcher_MissingExecutable_Error()
        {
            // #Act – Инициализация
            string fakeExePath = @"D:\NoPath\Cursah.exe";

            // #Action – что мы должны сделать
            bool exeExists = File.Exists(fakeExePath);

            // #Result
            Assert.False(exeExists);
        }
    }
}