using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using Cursah;
using Xunit;

namespace TestDll
{
    public class UnitTest_Auth 
    {
        private readonly string connStr = "Server=CERTAINPOINT\\SQLEXPRESS;Database=AppForNote;Trusted_Connection=True;TrustServerCertificate=True;";

        /// <summary>
        /// Позитивный тест: Успешный вход пользователя.
        /// Проверяем вход под правильными учетными данными.
        /// </summary>
        [Fact]
        [Trait("Category", "Авторизация")]
        public void Login_ValidCredentials()
        {
            //#Act
            XDocument xml = XDocument.Load("TestParam.xml");
            string user = xml.Root.Element("Auth").Attribute("validUser").Value;
            string pass = xml.Root.Element("Auth").Attribute("validPass").Value;
            AuthService auth = new AuthService(connStr);

            // #Action – сохраняем результат работы метода в переменную
            User loggedInUser = auth.Login(user, pass);

            //#Result
            Assert.Equal(user, loggedInUser.Username);
        }

        public static IEnumerable<object[]> GetBadPasswords()
        {
            XDocument xml = XDocument.Load("TestParam.xml");
            foreach (var node in xml.Root.Elements("BadPass"))
            {
                yield return new object[] { node.Attribute("user").Value, node.Attribute("pass").Value };
            }
        }

        /// <summary>
        /// Негативный тест: Вход с неверным паролем (несколько вариантов).
        /// Проверяем, что неверные пароли вызывают AppException.
        /// </summary>
        [Theory]
        [Trait("Category", "Авторизация")]
        [MemberData(nameof(GetBadPasswords))]
        public void Login_InvalidPasswords_ThrowsException(string user, string badPass)
        {
            // #Act – Инициализация
            AuthService auth = new AuthService(connStr);

            // #Action & #Result
            Assert.Throws<AppException>(() => auth.Login(user, badPass));
        }

        /// <summary>
        /// Негативный тест: Вход под несуществующим пользователем.
        /// </summary>
        [Fact]
        [Trait("Category", "Авторизация")]
        public void Login_NonExistentUser_ThrowsException()
        {
            // #Act – Инициализация
            XDocument xml = XDocument.Load("TestParam.xml");
            string badUser = xml.Root.Element("Auth").Attribute("invalidUser").Value;
            string pass = xml.Root.Element("Auth").Attribute("validPass").Value;
            AuthService auth = new AuthService(connStr);

            // #Action & #Result
            Assert.Throws<AppException>(() => auth.Login(badUser, pass));
        }

        /// <summary>
        /// Негативный тест: Вход заблокированного пользователя.
        /// </summary>
        [Fact]
        [Trait("Category", "Авторизация")]
        public void Login_BlockedUser_ThrowsException()
        {
            // #Act – Инициализация
            XDocument xml = XDocument.Load("TestParam.xml");
            string blockUser = xml.Root.Element("Auth").Attribute("blockUser").Value;
            string pass = xml.Root.Element("Auth").Attribute("validPass").Value;
            AuthService auth = new AuthService(connStr);

            // #Action & #Result
            Assert.Throws<AppException>(() => auth.Login(blockUser, pass));
        }

        /// <summary>
        /// Позитивный тест: Автоматическое восстановление сессии.
        /// </summary>
        [Fact]
        [Trait("Category", "Авторизация")]
        public void Session_RestoreSession_Success()
        {
            // #Act – Инициализация
            AuthService auth = new AuthService(connStr);
            File.WriteAllText(".session", "{\"Id\":1,\"Username\":\"User1\",\"Role\":\"User\"}");

            // #Action – что мы должны сделать
            User loadedUser = auth.LoadSession();

            // #Result
            Assert.NotNull(loadedUser);
            if (File.Exists(".session")) File.Delete(".session");
        }

        /// <summary>
        /// Позитивный тест: Выход из учетной записи.
        /// Проверяем, что файл сессии корректно удаляется.
        /// </summary>
        [Fact]
        [Trait("Category", "Авторизация")]
        public void Session_Logout_DeletesFile()
        {
            // #Act – Инициализация
            AuthService auth = new AuthService(connStr);
            File.WriteAllText(".session", "FakeSessionData");

            // #Action – что мы должны сделать
            auth.Logout();

            // #Result
            Assert.False(File.Exists(".session"));
        }
    }
}