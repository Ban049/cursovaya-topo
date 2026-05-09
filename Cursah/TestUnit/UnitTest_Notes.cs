using Microsoft.VisualStudio.TestTools.UnitTesting;
using Cursah;

namespace TestDll
{
    [TestClass]
    public class UnitTest_Notes
    {
        private readonly string connStr = "Server=CERTAINPOINT\\SQLEXPRESS;Database=AppForNote;Trusted_Connection=True;TrustServerCertificate=True;";
        private readonly int testUserId = 4;

        [TestMethod]
        public void AddNote_ValidData()
        {
            NoteService noteService = new NoteService(connStr);

            try
            {
                
                noteService.Add(testUserId, "Тестовый заголовок", "Содержимое из Unit Теста");

            }
            catch (AppException)
            {
                Assert.Fail("Ожидалось успешное добавление");
            }


        }

        [TestMethod]
        public void EditNote_ValidData()
        {
            int NoteId = 8;
            NoteService noteService = new NoteService(connStr);


            try
            {
                noteService.Edit(testUserId, NoteId, "Редактирование", "Содержимое из Unit Теста");

            }
            catch (AppException)
            {
                Assert.Fail("Ожидалось успешное редактирование");
            }

        }

        [TestMethod]
        public void InvalidEditNote_ValidData()
        {
            int NoteId = 9999;
            NoteService noteService = new NoteService(connStr);
            try
            {
                noteService.Edit(testUserId, NoteId, "Редактирование2", "Содержимое из Unit Теста");
                Assert.Fail("Ожидалась ошибка, но редактирование прошло успешно");
            }
            catch (AppException)
            {
                // Успех, система не дала удалить несуществующую заметку
            }


        }

        [TestMethod]
        public void DeleteNote_ExistentNoteId()
        {
            NoteService noteService = new NoteService(connStr);
            int NoteId = 12;

            try
            {
                noteService.Delete(testUserId, NoteId);

            }
            catch (AppException)
            {
                Assert.Fail("Ожидалось успешное удаление");
            }
        }

        [TestMethod]
        public void DeleteNote_NonExistentNoteId()
        {
            NoteService noteService = new NoteService(connStr);
            int fakeNoteId = 99999;

            try
            {
                noteService.Delete(testUserId, fakeNoteId);
                Assert.Fail("Ожидалась ошибка 404, но удаление прошло успешно!");
            }
            catch (AppException)
            {
                // Успех, система не дала удалить несуществующую заметку
            }
        }
    }
}