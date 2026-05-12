using Microsoft.VisualStudio.TestTools.UnitTesting;
using Cursah;

namespace TestDll
{
    [TestClass]
    public class UnitTest_Notes
    {
        private readonly string connStr = "Server=CERTAINPOINT\\SQLEXPRESS;Database=AppForNote;Trusted_Connection=True;TrustServerCertificate=True;";
        private readonly int testUserId = 4;

        /// <summary>
        /// Позитивный тест: Создание новой заметки.
        /// </summary>
        [TestMethod]
        public void AddNote_ValidData_ExecutesWithoutErrors()
        {
            // #Act – Инициализация
            NoteService noteService = new NoteService(connStr);
            string title = "Тестовый заголовок";
            string content = "Содержимое из Unit Теста";

            // #Action – что мы должны сделать
            try
            {
                noteService.Add(testUserId, title, content);

                // #Result
                Assert.IsTrue(true);
            }
            catch (AppException)
            {
                // #Result
                Assert.Fail("Ожидалось успешное добавление заметки.");
            }
        }

        /// <summary>
        /// Позитивный тест: Редактирование существующей заметки.
        /// </summary>
        [TestMethod]
        public void EditNote_ValidData_ExecutesWithoutErrors()
        {
            // #Act – Инициализация
            int noteId = 8;
            NoteService noteService = new NoteService(connStr);
            string newTitle = "Редактирование";
            string newContent = "Содержимое из Unit Теста";

            // #Action – что мы должны сделать
            try
            {
                noteService.Edit(testUserId, noteId, newTitle, newContent);

                // #Result
                Assert.IsTrue(true);
            }
            catch (AppException)
            {
                // #Result
                Assert.Fail("Ожидалось успешное редактирование заметки.");
            }
        }

        /// <summary>
        /// Негативный тест: Попытка редактирования несуществующей заметки.
        /// </summary>
        [TestMethod]
        public void EditNote_NonExistentId_ThrowsAppException()
        {
            // #Act – Инициализация
            int fakeNoteId = 9999;
            NoteService noteService = new NoteService(connStr);

            // #Action – что мы должны сделать
            try
            {
                noteService.Edit(testUserId, fakeNoteId, "Редактирование2", "Содержимое из Unit Теста");

                // #Result
                Assert.Fail("Ожидалась ошибка, но редактирование прошло успешно.");
            }
            catch (AppException)
            {
                // #Result
                Assert.IsTrue(true);
            }
        }

        /// <summary>
        /// Позитивный тест: Успешное удаление заметки.
        /// </summary>
        [TestMethod]
        public void DeleteNote_ExistentNoteId_ExecutesWithoutErrors()
        {
            // #Act – Инициализация
            NoteService noteService = new NoteService(connStr);
            int noteId = 12;

            // #Action – что мы должны сделать
            try
            {
                noteService.Delete(testUserId, noteId);

                // #Result
                Assert.IsTrue(true);
            }
            catch (AppException)
            {
                // #Result
                Assert.Fail("Ожидалось успешное удаление заметки.");
            }
        }

        /// <summary>
        /// Негативный тест: Попытка удаления несуществующей заметки.
        /// </summary>
        [TestMethod]
        public void DeleteNote_NonExistentNoteId_ThrowsAppException()
        {
            // #Act – Инициализация
            NoteService noteService = new NoteService(connStr);
            int fakeNoteId = 99999;

            // #Action – что мы должны сделать
            try
            {
                noteService.Delete(testUserId, fakeNoteId);

                // #Result
                Assert.Fail("Ожидалась ошибка, но удаление прошло успешно.");
            }
            catch (AppException)
            {
                // #Result
                Assert.IsTrue(true);
            }
        }
    }
}