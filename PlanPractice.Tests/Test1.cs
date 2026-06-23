using PlanPractice.Logic;
using System.Data;

namespace PlanPractice.Tests
{
    [TestClass]
    public sealed class DataBaseManagerTests
    {
        private DataBaseManager _dbManager;

        // Этот метод запускается автоматически ПЕРЕД каждым тестом
        [TestInitialize]
        public void Setup()
        {
            _dbManager = new DataBaseManager();
        }

        [TestMethod]
        public void TryExecuteQuery_ValidSelectQuery_ReturnsTrueAndFillsTable()
        {
            // Arrange (Подготовка)
            string validQuery = "SELECT * FROM Роли";
            DataTable resultTable = new DataTable();

            // Act (Выполнение)
            bool isSuccess = DataBaseManager.TryExecuteQuery(validQuery, ref resultTable);

            // Assert (Проверка)
            Assert.IsTrue(isSuccess, "Метод должен вернуть true для корректного запроса.");
            Assert.IsTrue(resultTable.Columns.Count > 0, "Таблица должна заполниться колонками из БД.");
        }

        [TestMethod]
        public void TryExecuteQuery_InvalidQuery_ReturnsFalse()
        {
            // Arrange
            string invalidQuery = "SELECT * FROM ТаблицаКоторойНет";
            DataTable resultTable = new DataTable();

            // Act
            bool isSuccess = DataBaseManager.TryExecuteQuery(invalidQuery, ref resultTable);

            // Assert
            Assert.IsFalse(isSuccess, "Метод должен поймать Exception и вернуть false для кривого запроса.");
        }

        [TestMethod]
        public void AddRecord_ValidData_InsertsRowToDatabase()
        {
            //Достаем структуру таблицы
            DataTable table = new DataTable();
            DataBaseManager.TryExecuteQuery("SELECT * FROM Роли", ref table);
            int initialRowCount = table.Rows.Count;

            //Генерируем уникальное имя, чтобы точно найти его в БД
            string testRoleName = "ADD_TEST_" + DateTime.Now.Ticks;

            Dictionary<string, string> newData = new Dictionary<string, string>
            {
                //Передаем только те поля, которые заполняет пользователь (без ID, он автоинкрементный)
                { "Роль", testRoleName }
            };

            try
            {
                _dbManager.AddRecord(newData, table, "Роли");

                //Проверяем локальную таблицу
                Assert.AreEqual(initialRowCount + 1, table.Rows.Count, "В локальную DataTable не добавилась строка.");

                //Проверяем физическую БД Access (делаем отдельный запрос)
                DataTable checkTable = new DataTable();
                DataBaseManager.TryExecuteQuery($"SELECT * FROM Роли WHERE Роль = '{testRoleName}'", ref checkTable);

                Assert.AreEqual(1, checkTable.Rows.Count, "Запись физически не добавилась в базу данных Access.");
            }
            finally
            {
                //Скачиваем копию таблицы напрямую из БД
                DataTable freshTable = new DataTable();
                DataBaseManager.TryExecuteQuery("SELECT * FROM Роли", ref freshTable);

                //Ищем нашу тестовую запись уже в свежей таблице
                DataRow[] rowsToDelete = freshTable.Select($"Роль = '{testRoleName}'");

                if (rowsToDelete.Length > 0)
                {
                    //Удаляем строку
                    _dbManager.DeleteRecord(freshTable, rowsToDelete[0], "Роли");
                }
            }
        }

        [TestMethod]
        public void EditRecord_ValidData_UpdatesDatabaseAndRow()
        {
            DataTable table = new DataTable();
            DataBaseManager.TryExecuteQuery("SELECT * FROM Роли", ref table);

            // Берем первую попавшуюся запись и запоминаем её оригинальное значение
            DataRow rowToEdit = table.Rows[0];
            string originalRoleName = rowToEdit["Роль"].ToString();
            string testRoleName = "TEST_" + DateTime.Now.Ticks; // Уникальное имя для теста

            Dictionary<string, string> editData = new Dictionary<string, string>
            {
                { "Роль", testRoleName }
            };

            try
            {
                _dbManager.EditRecord(editData, table, "Роли", rowToEdit);

                //Проверяем, что метод обновил строку в оперативной памяти
                Assert.AreEqual(testRoleName, rowToEdit["Роль"], "Значение в оперативной памяти не обновилось.");

                //Проверяем, что метод сохранил данные в .accdb базу!
                DataTable checkTable = new DataTable();
                DataBaseManager.TryExecuteQuery("SELECT * FROM Роли", ref checkTable);
                Assert.AreEqual(testRoleName, checkTable.Rows[0]["Роль"], "Значение не сохранилось в физической базе данных Access.");
            }
            finally
            {
                //Возвращаем старое значение обратно в базу, чтобы после тестов она осталась чистой.
                Dictionary<string, string> revertData = new Dictionary<string, string>
                {
                    { "Роль", originalRoleName }
                };
                _dbManager.EditRecord(revertData, table, "Роли", rowToEdit);
            }
        }

        [TestMethod]
        public void Login_EmptyCredentials_ReturnsFalse()
        {
            // Arrange
            string emptyLogin = "";
            string emptyPassword = "";

            // Act
            bool result = _dbManager.Login(emptyLogin, emptyPassword);

            // Assert
            Assert.IsFalse(result, "Авторизация с пустыми данными должна провалиться.");
        }
    }
}
