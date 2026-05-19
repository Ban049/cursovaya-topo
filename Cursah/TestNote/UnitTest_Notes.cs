using System.Xml.Linq;
using Cursah;
using Xunit;

namespace TestDll
{
    public class UnitTest_Notes
    {
        private readonly string connStr = "Server=CERTAINPOINT\\SQLEXPRESS;Database=AppForNote;Trusted_Connection=True;TrustServerCertificate=True;";

        /// <summary>
        /// Позитивный тест: Создание новой заметки.
        /// </summary>
        [Fact]
        [Trait("Category", "Заметки")]
        public void AddNote_Success()
        {
            // #Act – Инициализация
            XDocument xml = XDocument.Load("TestParam.xml");
            int uId = int.Parse(xml.Root.Element("Notes").Attribute("uId").Value);
            string title = xml.Root.Element("Notes").Attribute("title").Value;
            NoteService notes = new NoteService(connStr);

            // #Action – что мы должны сделать
            notes.Add(uId, title, "Описание");

            // #Result
            Assert.True(true);
        }

        /// <summary>
        /// Позитивный тест: Вывод списка заметок.
        /// </summary>
        [Fact]
        [Trait("Category", "Заметки")]
        public void ListNotes_Success()
        {
            // #Act – Инициализация
            XDocument xml = XDocument.Load("TestParam.xml");
            int uId = int.Parse(xml.Root.Element("Notes").Attribute("uId").Value);
            NoteService notes = new NoteService(connStr);

            // #Action – что мы должны сделать
            notes.List(uId);

            // #Result
            Assert.True(true);
        }

        /// <summary>
        /// Позитивный тест: Редактирование своей заметки.
        /// </summary>
        [Fact]
        [Trait("Category", "Заметки")]
        public void EditOwnNote_Success()
        {
            // #Act – Инициализация
            XDocument xml = XDocument.Load("TestParam.xml");
            int uId = int.Parse(xml.Root.Element("Notes").Attribute("uId").Value);
            int nId = int.Parse(xml.Root.Element("Notes").Attribute("nrId").Value);
            string newTitle = xml.Root.Element("Notes").Attribute("newTitle").Value;
            NoteService notes = new NoteService(connStr);

            // #Action – что мы должны сделать
            notes.Edit(uId, nId, newTitle, "Изменено");

            // #Result
            Assert.True(true);
        }

        /// <summary>
        /// Негативный тест: Редактирование несуществующей заметки.
        /// </summary>
        [Fact]
        [Trait("Category", "Заметки")]
        public void EditNonExistentNote_ThrowsException()
        {
            // #Act – Инициализация
            XDocument xml = XDocument.Load("TestParam.xml");
            int uId = int.Parse(xml.Root.Element("Notes").Attribute("uId").Value);
            int fakeId = int.Parse(xml.Root.Element("Notes").Attribute("fakeId").Value);
            NoteService notes = new NoteService(connStr);

            // #Action & #Result
            Assert.Throws<AppException>(() => notes.Edit(uId, fakeId, "Ошибка", "..."));
        }

        /// <summary>
        /// Негативный тест: Удаление несуществующей заметки.
        /// </summary>
        [Fact]
        [Trait("Category", "Заметки")]
        public void DeleteNonExistentNote_ThrowsException()
        {
            // #Act – Инициализация
            XDocument xml = XDocument.Load("TestParam.xml");
            int uId = int.Parse(xml.Root.Element("Notes").Attribute("uId").Value);
            int fakeId = int.Parse(xml.Root.Element("Notes").Attribute("fakeId").Value);
            NoteService notes = new NoteService(connStr);

            // #Action & #Result
            Assert.Throws<AppException>(() => notes.Delete(uId, fakeId));
        }

        /// <summary>
        /// Позитивный тест : Удаление своей заметки.
        /// </summary>
        [Fact]
        [Trait("Category", "Заметки")]
        public void DeleteOwnNote_Success()
        {
            // #Act – Инициализация
            XDocument xml = XDocument.Load("TestParam.xml");
            int uId = int.Parse(xml.Root.Element("Notes").Attribute("uId").Value);
            int nId = int.Parse(xml.Root.Element("Notes").Attribute("nId").Value);
            NoteService notes = new NoteService(connStr);

            // #Action – что мы должны сделать
            notes.Delete(uId, nId);

            // #Result
            Assert.True(true);
        }
    }
}