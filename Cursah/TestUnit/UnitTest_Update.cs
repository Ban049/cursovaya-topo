using Microsoft.VisualStudio.TestTools.UnitTesting;
using Cursah;
using System.IO;

namespace TestDll
{
    [TestClass]
    public class UnitTest_Update
    {
        [TestMethod]
        public void PrepareUpdate_ExecutesAndCreatesDirectory()
        {
            // Act: Запускаем статический метод
            UpdateManager.PrepareUpdate();

            // Assert: Проверяем, что базовая папка была создана
            // D:\Учёба\3 курс\2 сем\Тестирование и отладка ПО\Курсач\NoteService
            string expectedBasePath = @"D:\Учёба\3 курс\2 сем\Тестирование и отладка ПО\Курсач\NoteService";

            bool dirExists = Directory.Exists(expectedBasePath);
            Assert.IsTrue(dirExists, "Базовая папка для обновлений не была создана.");
        }
    }
}