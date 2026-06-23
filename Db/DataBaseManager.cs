using Plan.Logic;
using System.Data;
using System.Data.Common;
using System.Data.OleDb;
using System.Security.Cryptography;
using System.Text;

namespace PlanPractice.Logic
{
    public class DataBaseManager
    {
        public const string ConnectionString = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=Plan.accdb;";
        public OleDbDataAdapter MyDataAdapter;
        public OleDbCommandBuilder MyBuilder;

        public enum UserRoles
        {
            Guest, //Может только просматривать таблицы
            Manager, //Просмотр таблиц, запросы, создание отчётов
            Admin //Редактирование/удаление/добавление записей, все остальное также доступно
        }
        public UserRoles CurrentRole;
        public bool Login(string login, string password)
        {
            using (OleDbConnection MyConnect = new OleDbConnection(ConnectionString))
            {
                MyConnect.Open();

                // Прописываем запрос для получения роли
                string query = @"SELECT Роли.Роль
                             FROM Пользователи
                             INNER JOIN Роли ON Пользователи.[ID роли] = Роли.[ID роли]
                             WHERE Пользователи.Логин = @Login AND Пользователи.Пароль = @HashedPassword";

                // Создаём комманду для выполнения запроса, защищённо вставляем параметрs логина и пароля
                OleDbCommand command = new OleDbCommand(query, MyConnect);
                command.Parameters.AddWithValue("@Login", login);
                command.Parameters.AddWithValue("@HashedPassword", HashPassword(password));

                // Создаём адаптер для выполнения команды и заполнения локальной копии таблицы
                MyDataAdapter = new OleDbDataAdapter(command);
                DataTable table = new DataTable();
                MyDataAdapter.Fill(table);

                //Проверяем, найдены ли такие аккаунты
                if (table.Rows.Count > 0)
                {
                    //Получаем роль
                    CurrentRole = CheckUserRole(table.Rows[0]["Роль"].ToString());
                    return true;
                }
                else return false;
            }
        }
        private static string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                // Преобразуем введенный пароль  в набор байтов
                byte[] inputBytes = Encoding.UTF8.GetBytes(password);

                // Вычисляем хеш (получаем зашифрованный массив байтов)
                byte[] hashBytes = sha256.ComputeHash(inputBytes);

                // Конвертируем байты хеша в строку
                return string.Join("-",hashBytes);
            }
        }
        private UserRoles CheckUserRole(string roleText)
        {
            switch (roleText)
            {
                case "Администратор":
                    return UserRoles.Admin;
                case "Менеджер":
                    return UserRoles.Manager;
                default:
                    return UserRoles.Guest;
            }
        }
        public bool Register(string newLogin, string newPassword)
        {
            using (OleDbConnection MyConnect = new OleDbConnection(ConnectionString))
            {
                MyConnect.Open();

                //Если логин доступен (Не использован прежде) регистрируем пользователя с ролью гостя
                if (CheckLoginAvailability(newLogin))
                {
                    string hashedPassword = HashPassword(newPassword);

                    //Загрузка таблицы пользователей
                    string query = "SELECT * FROM Пользователи";
                    OleDbCommand command = new OleDbCommand(query, MyConnect);
                    MyDataAdapter = new OleDbDataAdapter(command);
                    DataTable usersTable = new DataTable();
                    MyDataAdapter.Fill(usersTable);

                    //Добавление записи в локальную таблицу
                    DataRow newRow = usersTable.NewRow();
                    newRow["Логин"] = newLogin;
                    newRow["Пароль"] = hashedPassword;
                    newRow["ID роли"] = 4; // Код роли (Гость)
                    usersTable.Rows.Add(newRow);

                    //Инициализация и найстройка билдера команд
                    MyBuilder = new OleDbCommandBuilder(MyDataAdapter);
                    MyBuilder.QuotePrefix = "[";
                    MyBuilder.QuoteSuffix = "]";

                    //Обновление таблицы БД
                    MyDataAdapter.Update(usersTable);

                    //Login(newLogin, newPassword);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            
        }
        private bool CheckLoginAvailability(string newLogin)
        {
            using (OleDbConnection MyConnect = new OleDbConnection(ConnectionString))
            {
                MyConnect.Open();

                string CheckQuery = "SELECT COUNT(*) FROM Пользователи WHERE Логин = @Login";

                OleDbCommand command = new OleDbCommand(CheckQuery, MyConnect);
                command.Parameters.AddWithValue("@Login", newLogin);

                int usersValue = Convert.ToInt32(command.ExecuteScalar());
                if (usersValue > 0)
                    return false;
                else
                    return true;
            }
        }

        public List<string> GetListTableNames()
        {
            List<string> listTableNames = new List<string>();

            using (OleDbConnection MyConnect = new OleDbConnection(ConnectionString))
            {
                MyConnect.Open();

                DataTable schema = MyConnect.GetSchema("Tables");
                foreach (DataRow row in schema.Rows)
                {
                    string tableName = row["TABLE_NAME"].ToString();
                    string tableType = row["TABLE_TYPE"].ToString();

                    if (tableType == "TABLE")
                    {
                        if ((tableName == "Пользователи" || tableName == "Роли") && CurrentRole != UserRoles.Admin) continue; //Скрытие таблиц пользователей и ролей для всех кроме админа
                        listTableNames.Add(tableName);
                    }
                }

                return listTableNames;
            }
        }
        public static DataTable GetDataTable(string TableName)
        {
            DataTable dt = new DataTable();

            using (OleDbConnection MyConnect = new OleDbConnection(ConnectionString))
            {
                MyConnect.Open();

                string query = $"SELECT * FROM [{TableName}]";
                OleDbCommand command = new OleDbCommand(query, MyConnect);

                using (OleDbDataAdapter adapter = new OleDbDataAdapter(command))
                    adapter.Fill(dt);
            }
            return dt;   
        }
        private void UpdateTable(DataTable table, string tableName)
        {
            using (OleDbConnection MyConnect = new OleDbConnection(ConnectionString))
            {
                MyConnect.Open();

                string query = $"SELECT * FROM [{tableName}]";
                MyDataAdapter = new OleDbDataAdapter(query, MyConnect);

                MyBuilder = new OleDbCommandBuilder(MyDataAdapter);
                MyBuilder.QuotePrefix = "[";
                MyBuilder.QuoteSuffix = "]";

                MyDataAdapter.Update(table);
            }
        }
        public void AddRecord(Dictionary<string, string> rec, DataTable table, string tableName)
        {
            DataRow newRow = table.NewRow();
            foreach (string col in rec.Keys)
            {
                newRow[col] = rec[col];
            }
            table.Rows.Add(newRow);

            UpdateTable(table, tableName);
        }
        
        public void DeleteRecord(DataTable table, DataRow row, string tableName)
        {
            row.Delete();

            UpdateTable(table, tableName);
        }

        public void EditRecord(Dictionary<string, string> EditRec, DataTable table, string tableName, DataRow row)
        {
            foreach (string col in EditRec.Keys)
            {
                row[col] = EditRec[col];
            }

            UpdateTable(table, tableName);
        }

        public static bool TryExecuteQuery(string query, ref DataTable dt)
        {
            using (OleDbConnection MyConnect = new OleDbConnection(ConnectionString))
            {
                MyConnect.Open();

                OleDbCommand command = new OleDbCommand(query, MyConnect);

                try
                {
                    using (OleDbDataAdapter adapter = new OleDbDataAdapter(command))
                        adapter.Fill(dt);
                }
                catch (Exception ex)
                {
                    Logger.ErrorLog(ex);
                    return false;
                }
            }
            return true;
        }
    }
}
