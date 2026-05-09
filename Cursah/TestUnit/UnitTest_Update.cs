using Microsoft.VisualStudio.TestTools.UnitTesting;
using Cursah;
using System.IO;

namespace TestDll
{
    [TestClass]
    public class UnitTest_Update
    {
        /// <summary>
        /// Позитивный тест: Подготовка директории обновления.
        /// </summary>
        [TestMethod]
        public void PrepareUpdate_ExecutesAndCreatesDirectory()
        {
            // #Act – Инициализация
            string expectedBasePath = @"D:\Учёба\3 курс\2 сем\Тестирование и отладка ПО\Курсач\NoteService";
            bool expectedResult = true;

            // #Action – что мы должны сделать
            UpdateManager.PrepareUpdate();
            bool actualResult = Directory.Exists(expectedBasePath);

            // #Result
            Assert.AreEqual(expectedResult, actualResult);
        }
    }
}