using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Data.SqlClient;
using System.Data;

namespace TestDll
{
    [TestClass]
    public class UnitTest_Database
    {
        private readonly string validConnStr = "Server=CERTAINPOINT\\SQLEXPRESS;Database=AppForNote;Trusted_Connection=True;TrustServerCertificate=True;";
        private readonly string invalidConnStr = "Server=NoConn\\SQLEXPRESS;Database=AppForNote;Trusted_Connection=True;TrustServerCertificate=True;";

        /// <summary>
        /// Позитивный тест: Успешное подключение к БД с корректной строкой.
        /// </summary>
        [TestMethod]
        public void DbConnection_ValidString_OpensSuccessfully()
        {
            // #Act – Инициализация
            string connStr = validConnStr;
            ConnectionState expected = ConnectionState.Open;

            // #Action – что мы должны сделать
            using var conn = new SqlConnection(connStr);
            conn.Open();
            ConnectionState actual = conn.State;

            // #Result
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Негативный тест: Ошибка при попытке подключения с неверным сервером.
        /// </summary>
        [TestMethod]
        public void DbConnection_InvalidString_ThrowsSqlException()
        {
            // #Act – Инициализация
            string connStr = invalidConnStr;

            // #Action – что мы должны сделать
            try
            {
                using var conn = new SqlConnection(connStr);
                conn.Open();

                // #Result (если код дошел сюда - ошибка)
                Assert.Fail("Ожидалась ошибка SqlException, но подключение прошло успешно.");
            }
            catch (SqlException)
            {
                // #Result (успешная отработка негативного теста)
                Assert.IsTrue(true);
            }
        }
    }
}