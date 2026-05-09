using Microsoft.VisualStudio.TestTools.UnitTesting;
using Cursah;

namespace TestDll
{
    [TestClass]
    public class UnitTest_Auth
    {
        private readonly string connStr = "Server=CERTAINPOINT\\SQLEXPRESS;Database=AppForNote;Trusted_Connection=True;TrustServerCertificate=True;";

        [TestMethod]
        public void Login_ValidPassword()
        {
            AuthService authService = new AuthService(connStr);
            string testUser = "User1";
            string wrongPass = "1qaz!QAZ";

            try
            {
                authService.Login(testUser, wrongPass);
                // Тест пройден
            }
            catch (AppException)
            {
                Assert.Fail("Ожидалось успешное вхождение");

            }
        }

        [TestMethod]
        public void Login_BlockUser()
        {
            AuthService authService = new AuthService(connStr);
            string testUser = "Block";
            string wrongPass = "1qaz!QAZ";

            try
            {
                authService.Login(testUser, wrongPass);
                Assert.Fail("Ожидалась ошибка авторизации, но вход был выполнен");
            }
            catch (AppException)
            {
                // Тест пройден
            }
        }

        [TestMethod]
        public void Login_InvalidPassword_ThrowsAppException()
        {
            AuthService authService = new AuthService(connStr);
            string testUser = "User1";
            string wrongPass = "123";

            try
            {
                authService.Login(testUser, wrongPass);
                Assert.Fail("Ожидалась ошибка авторизации, но вход был выполнен");
            }
            catch (AppException)
            {
                // Тест пройден
            }
        }

        [TestMethod]
        public void Login_NonExistentUser_ThrowsAppException()
        {
            AuthService authService = new AuthService(connStr);

            try
            {
                authService.Login("NoUser123", "12345");
                Assert.Fail("Ожидалась ошибка, но несуществующий пользователь вошел в систему!");
            }
            catch (AppException)
            {
                // Тест пройден
            }
        }
    }
}