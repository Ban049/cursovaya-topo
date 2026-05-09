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

        [TestMethod]
        public void DbConnection_ValidString_OpensSuccessfully()
        {
            using var conn = new SqlConnection(validConnStr);
            conn.Open();

            Assert.AreEqual(ConnectionState.Open, conn.State);
        }

        [TestMethod]
        public void DbConnection_InvalidString_ThrowsSqlException()
        {
            try
            {
                // Пробуем подключиться с некорректной строкой
                using var conn = new SqlConnection(invalidConnStr);
                conn.Open();

                // Если код дошел до этой строчки (ошибки не было) 
                Assert.Fail("Ожидалась ошибка SqlException");
            }
            catch (SqlException)
            {
                // Тест пройден
            }
        }
    }
}