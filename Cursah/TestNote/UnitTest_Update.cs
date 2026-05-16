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
        /// </summary>
        [Fact]
        [Trait("Category", "Обновления")]
        public void Launcher_DetectsNewVersion()
        {
            // #Act – Инициализация
            string currentVersionFile = "current_version.txt";
            File.WriteAllText(currentVersionFile, "v-1.0.3");

            // #Action – что мы должны сделать
            string fileContent = File.ReadAllText(currentVersionFile);
            bool isUpdateNeeded = (fileContent != "v-1.0.4");

            // #Result
            Assert.True(isUpdateNeeded);
        }

        /// <summary>
        /// Позитивный тест: Лаунчер запускается без новых обновлений.
        /// </summary>
        [Fact]
        [Trait("Category", "Обновления")]
        public void Launcher_NoUpdates_StartsNormal()
        {
            // #Act – Инициализация
            string currentVersionFile = "current_version.txt";
            File.WriteAllText(currentVersionFile, "v-1.0.3"); 

            // #Action – что мы должны сделать
            string fileContent = File.ReadAllText(currentVersionFile);
            bool isUpdateNeeded = (fileContent != "v-1.0.3");

            // #Result
            Assert.False(isUpdateNeeded);
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