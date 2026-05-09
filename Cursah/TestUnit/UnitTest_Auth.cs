using Microsoft.VisualStudio.TestTools.UnitTesting;
using Cursah;

namespace TestDll
{
    [TestClass]
    public class UnitTest_Auth
    {
        private readonly string connStr = "Server=CERTAINPOINT\\SQLEXPRESS;Database=AppForNote;Trusted_Connection=True;TrustServerCertificate=True;";

        /// <summary>
        /// Позитивный тест: Успешная авторизация пользователя.
        /// </summary>
        [TestMethod]
        public void Login_ValidCredentials_ExecutesSuccessfully()
        {
            // #Act – Инициализация
            AuthService authService = new AuthService(connStr);
            string testUser = "User1";
            string validPass = "1qaz!QAZ";

            // #Action – что мы должны сделать
            try
            {
                authService.Login(testUser, validPass);

                // #Result
                Assert.IsTrue(true);
            }
            catch (AppException)
            {
                // #Result
                Assert.Fail("Ожидалась успешная авторизация, но получена ошибка.");
            }
        }

        /// <summary>
        /// Негативный тест: Попытка входа заблокированного пользователя.
        /// </summary>
        [TestMethod]
        public void Login_BlockedUser_ThrowsAppException()
        {
            // #Act – Инициализация
            AuthService authService = new AuthService(connStr);
            string testUser = "Block";
            string validPass = "1qaz!QAZ";

            // #Action – что мы должны сделать
            try
            {
                authService.Login(testUser, validPass);

                // #Result
                Assert.Fail("Ожидалась ошибка блокировки, но вход был выполнен.");
            }
            catch (AppException)
            {
                // #Result
                Assert.IsTrue(true);
            }
        }

        /// <summary>
        /// Негативный тест: Попытка входа с неверным паролем.
        /// </summary>
        [TestMethod]
        public void Login_InvalidPassword_ThrowsAppException()
        {
            // #Act – Инициализация
            AuthService authService = new AuthService(connStr);
            string testUser = "User1";
            string wrongPass = "123";

            // #Action – что мы должны сделать
            try
            {
                authService.Login(testUser, wrongPass);

                // #Result
                Assert.Fail("Ожидалась ошибка неверного пароля, но вход был выполнен.");
            }
            catch (AppException)
            {
                // #Result
                Assert.IsTrue(true);
            }
        }

        /// <summary>
        /// Негативный тест: Попытка входа под несуществующим логином.
        /// </summary>
        [TestMethod]
        public void Login_NonExistentUser_ThrowsAppException()
        {
            // #Act – Инициализация
            AuthService authService = new AuthService(connStr);

            // #Action – что мы должны сделать
            try
            {
                authService.Login("NoUser123", "12345");

                // #Result
                Assert.Fail("Ожидалась ошибка, но несуществующий пользователь вошел в систему.");
            }
            catch (AppException)
            {
                // #Result
                Assert.IsTrue(true);
            }
        }
    }
}