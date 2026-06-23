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

        [TestMethod]
        public void Register_DuplicateLogin_ReturnsFalse()
        {
            // Arrange
            string testUser = "DuplicateTest_" + DateTime.Now.Ticks;
            string password = "TestPassword123";

            try
            {
                //Первая успешная регистрация
                bool firstTry = _dbManager.Register(testUser, password);
                Assert.IsTrue(firstTry, "Первая регистрация уникального пользователя должна пройти успешно.");

                //Попытка зарегистрировать тот же логин
                bool secondTry = _dbManager.Register(testUser, "DifferentPassword456");

                // Assert
                Assert.IsFalse(secondTry, "Программа не должна позволять регистрацию с уже занятым логином.");
            }
            finally
            {
                //Уборка: удаляем тестового пользователя из базы
                DataTable usersTable = new DataTable();
                DataBaseManager.TryExecuteQuery("SELECT * FROM Пользователи", ref usersTable);
                DataRow[] rows = usersTable.Select($"Логин = '{testUser}'");

                if (rows.Length > 0)
                {
                    _dbManager.DeleteRecord(usersTable, rows[0], "Пользователи");
                }
            }
        }
        [TestMethod]
        public void Login_WrongPassword_ReturnsFalse()
        {
            //Пытаемся войти под системным логином
            string login = "Admin";
            string wrongPassword = "DefinitelyWrongPassword!@#";

            // Act
            bool result = _dbManager.Login(login, wrongPassword);

            // Assert
            Assert.IsFalse(result, "Вход с неверным паролем или логином должен быть отклонен.");
        }
        [TestMethod]
        public void GetListTableNames_GuestRole_HidesSystemTables()
        {
            // Arrange: Имитируем, что залогинился обычный Гость
            _dbManager.CurrentRole = DataBaseManager.UserRoles.Guest;

            // Act
            List<string> tables = _dbManager.GetListTableNames();

            // Assert
            Assert.IsFalse(tables.Contains("Пользователи"), "Гость не должен видеть таблицу 'Пользователи'.");
            Assert.IsFalse(tables.Contains("Роли"), "Гость не должен видеть таблицу 'Роли'.");
            Assert.IsTrue(tables.Count > 0, "Гость должен видеть хотя бы какие-то производственные таблицы.");
        }
        [TestMethod]
        public void GetListTableNames_AdminRole_ShowsAllTables()
        {
            // Arrange: Имитируем вход Администратора
            _dbManager.CurrentRole = DataBaseManager.UserRoles.Admin;

            // Act
            List<string> tables = _dbManager.GetListTableNames();

            // Assert
            Assert.IsTrue(tables.Contains("Пользователи"), "Администратор должен видеть таблицу 'Пользователи'.");
            Assert.IsTrue(tables.Contains("Роли"), "Администратор должен видеть таблицу 'Роли'.");
        }
        [TestMethod]
        public void GetDataTable_ValidTableName_ReturnsFilledTable()
        {
            // Arrange
            string tableName = "Роли"; // Базовая таблица, которая есть всегда

            // Act
            DataTable dt = DataBaseManager.GetDataTable(tableName);

            // Assert
            Assert.IsNotNull(dt, "Метод не должен возвращать null.");
            Assert.IsTrue(dt.Columns.Contains("Роль"), "Таблица должна содержать колонку 'Роль', как в схеме БД.");
            Assert.IsTrue(dt.Rows.Count > 0, "Таблица 'Роли' не должна быть пустой (в ней должны быть заложены системные роли).");
        }
        [TestMethod]
        public void DeleteRecord_ValidRow_RemovesFromDatabase()
        {
            //Создаем временную строку специально для удаления
            DataTable table = new DataTable();
            DataBaseManager.TryExecuteQuery("SELECT * FROM Роли", ref table);

            string testRole = "DELETE_TEST_" + DateTime.Now.Ticks;
            Dictionary<string, string> newData = new Dictionary<string, string> { { "Роль", testRole } };

            //Добавляем запись
            _dbManager.AddRecord(newData, table, "Роли");

            //Скачиваем свежую таблицу, чтобы у записи появился ID от базы данных
            DataTable freshTable = new DataTable();
            DataBaseManager.TryExecuteQuery("SELECT * FROM Роли", ref freshTable);
            DataRow[] rowsToDelete = freshTable.Select($"Роль = '{testRole}'");

            Assert.IsTrue(rowsToDelete.Length > 0, "Запись для теста удаления не была создана.");

            //Вызываем метод удаления
            _dbManager.DeleteRecord(freshTable, rowsToDelete[0], "Роли");

            //Делаем запрос в базу, чтобы убедиться, что строки там больше нет
            DataTable checkTable = new DataTable();
            DataBaseManager.TryExecuteQuery($"SELECT * FROM Роли WHERE Роль = '{testRole}'", ref checkTable);

            Assert.AreEqual(0, checkTable.Rows.Count, "Метод DeleteRecord не удалил запись из физической базы данных.");
        }
        [TestMethod]
        public void Login_SqlInjectionAttempt_ReturnsFalse()
        {
            //Классическая хакерская атака, которая пытается обмануть логику базы
            string injectionLogin = "Admin' OR '1'='1";
            string anyPassword = "AnyRandomPassword";

            // Act
            bool result = _dbManager.Login(injectionLogin, anyPassword);

            // Assert
            Assert.IsFalse(result, "КРИТИЧЕСКАЯ УЯЗВИМОСТЬ! Система пропустила SQL-инъекцию при авторизации.");
        }
    }
}
