using System.Data;
using System.IO;
using System.Xml.Linq;
using Microsoft.Data.SqlClient;
using Xunit;

namespace TestDll
{
    public class UnitTest_Database
    {
        /// <summary>
        /// Позитивный тест: Успешное подключение к БД.
        /// Проверяем, что корректная строка подключения открывает соединение.
        /// </summary>
        [Fact]
        [Trait("Category", "База данных")]
        public void DbConnection_ValidString()
        {
            // #Act – Инициализация
            XDocument xml = XDocument.Load("TestParam.xml");
            string connStr = xml.Root.Element("Db").Attribute("valid").Value;

            // #Action – что мы должны сделать
            using var conn = new SqlConnection(connStr);
            conn.Open();

            // #Result
            Assert.Equal(ConnectionState.Open, conn.State);
        }

        
        /// <summary>
        /// Негативный тест: Подключение к БД с некорректной строкой.
        /// Проверяем, что при неверном сервере выбрасывается SqlException.
        /// </summary>
        [Fact]
        [Trait("Category", "База данных")]
        public void DbConnection_InvalidString_ThrowsException()
        {
            // #Act – Инициализация
            XDocument xml = XDocument.Load("TestParam.xml");
            string badConnStr = xml.Root.Element("Db").Attribute("invalid").Value;

            // #Action & #Result
            Assert.Throws<SqlException>(() =>
            {
                using var conn = new SqlConnection(badConnStr);
                conn.Open();
            });
        }
    }
}