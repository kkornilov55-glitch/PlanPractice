using System.Data;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Plan.Logic;
using PlanPractice.Logic;

namespace PlanPractice.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {   
        DataBaseManager Db = new DataBaseManager();
        public List<string> TableNames { get; set; }
        private DataTable CurrentTable { get; set; }
        private string CurrentTableName { get; set; }
        public MainWindow()
        {
            InitializeComponent();

            this.DataContext = this;
        }

        private void Connect_Button_Click(object sender, RoutedEventArgs e)
        {
            Connect();
        }
        private void Connect()
        {
            AuthWindow auth = new AuthWindow(Db);
            auth.Owner = this;

            bool? DialogResult = auth.ShowDialog();
            if (DialogResult == true)
            {
                DataBaseName_TextBox.Text = "Plan";

                TableNames = Db.GetListTableNames();
                TablesTreeView.ItemsSource = null;
                TablesTreeView.ItemsSource = TableNames;

                ConnectButtonGrid.Visibility = Visibility.Collapsed;
                DisconnectButtonGrid.Visibility = Visibility.Visible;

                ShowAvailableButtons();
            }
        }
        private void ShowAvailableButtons()
        {
            if (Db.CurrentRole == DataBaseManager.UserRoles.Admin)
            {
                AddRow_Grid.Visibility = Visibility.Visible;
                DeleteRow_Grid.Visibility = Visibility.Visible;
                EditRow_Grid.Visibility = Visibility.Visible;
            }
            else if (Db.CurrentRole == DataBaseManager.UserRoles.Manager)
            {
                AddRow_Grid.Visibility = Visibility.Visible;
                EditRow_Grid.Visibility = Visibility.Visible;
            }

            QueriesAndReports_Grid.Visibility = Visibility.Visible;
            QueriesAndReports_Separator.Visibility = Visibility.Visible;
            Refresh_Grid.Visibility = Visibility.Visible;
        }
        private void Disconnect_Button_Click(object sender, RoutedEventArgs e)
        {
            //Отчищаем данные предыдущего подключения к БД
            DataBaseName_TextBox.Text = string.Empty;
            TablesTreeView.ItemsSource = null;
            MainDataGrid.ItemsSource = null;
            CurrentTable = null;

            //Меняем кнопки местами
            DisconnectButtonGrid.Visibility = Visibility.Collapsed;
            ConnectButtonGrid.Visibility = Visibility.Visible;

            //Прячем кнопки управления данными
            AddRow_Grid.Visibility = Visibility.Collapsed;
            DeleteRow_Grid.Visibility = Visibility.Collapsed;
            EditRow_Grid.Visibility = Visibility.Collapsed;
            Refresh_Grid.Visibility = Visibility.Collapsed;

            //Прячем кнопку открытия окна запросов и отчётов (А также разделитель между ней и списком таблиц)
            QueriesAndReports_Grid.Visibility = Visibility.Collapsed;
            QueriesAndReports_Separator.Visibility = Visibility.Collapsed;

            MessageBoxResult result = MessageBox.Show("Произведено отключение от БД. Пожалуйста, авторизуйтесь повторно для продолжения работы!", "Отключение от БД", MessageBoxButton.OKCancel, MessageBoxImage.Information);
            if (result == MessageBoxResult.OK)
                Connect();

        }
        private void TablesTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (TablesTreeView.SelectedItem == null) return;
            CurrentTableName = TablesTreeView.SelectedItem.ToString();

            //Вывод таблицы
            RefreshTable();
        }
        private void RefreshTable()
        {
            if (CurrentTableName == "Продукция-сырье")
            {
                string query = @"SELECT [Продукция-сырье].[ID записи], 
                                         Продукция.[Название продукции], 
                                         Сырье.[Наименование сырья], 
                                         [Продукция-сырье].[НП сырья] 
                                  FROM ([Продукция-сырье] 
                                  INNER JOIN Продукция ON [Продукция-сырье].[ID продукции] = Продукция.[ID продукции]) 
                                  INNER JOIN Сырье ON [Продукция-сырье].[ID сырья] = Сырье.[ID сырья]";
                DataTable dt = new DataTable();
                DataBaseManager.TryExecuteQuery(query, ref dt);
                CurrentTable = dt;
            }
            else if (CurrentTableName == "Продукция-энергоресурс")
            {
                string query = @"SELECT 
                [Продукция-энергоресурс].[ID записи],
                Продукция.[Название продукции], 
                Энергоресурсы.[Энергоресурс], 
                [Продукция-энергоресурс].[НП энергоресурса]
                FROM ([Продукция-энергоресурс]
                INNER JOIN Продукция ON [Продукция-энергоресурс].[ID продукции] = Продукция.[ID продукции])
                INNER JOIN Энергоресурсы ON [Продукция-энергоресурс].[ID энергоресурса] = Энергоресурсы.[ID энергоресурса]";
                DataTable dt = new DataTable();
                DataBaseManager.TryExecuteQuery(query, ref dt);
                CurrentTable = dt;
            }
            else
            {
                CurrentTable = DataBaseManager.GetDataTable(CurrentTableName);
            }

            //Сортировка записей по ID записи (По возрастанию)
            string firstCol = CurrentTable.Columns[0].ColumnName;
            CurrentTable.DefaultView.Sort = $"{firstCol} ASC";

            MainDataGrid.ItemsSource = CurrentTable.DefaultView;
        }

        private void AddRow_Button_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentTable == null) return;

            AddRecordWindow addRecordWindow = new AddRecordWindow(DataBaseManager.GetDataTable(CurrentTableName));
            addRecordWindow.Owner = this;

            bool? dialogResult = addRecordWindow.ShowDialog();
            if (dialogResult == true)
            {
                try
                {
                    Db.AddRecord(addRecordWindow.ResultData, DataBaseManager.GetDataTable(CurrentTableName), CurrentTableName);
                }
                catch (Exception ex)
                {
                    Logger.ErrorLog(ex);
                    MessageBox.Show("Не удалось добавить запись! Возможно, данные в полях имеют неверный тип.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            RefreshTable();
        }

        private void EditRow_Button_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentTable == null) return;

            //Получаем строчку для редактирования
            if (MainDataGrid.SelectedItem == null)
            {
                MessageBox.Show("Пожалуйста, выберите запись для редактирования!", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return; // Прерываем выполнение
            }


            DataRowView rowView = (DataRowView)MainDataGrid.SelectedItem;
            string idColumnName = CurrentTable.Columns[0].ColumnName;
            object recordId = rowView[0];

            DataTable pureTable = DataBaseManager.GetDataTable(CurrentTableName);
            DataRow pureRowToEdit = DataBaseManager.FindRowById(pureTable, idColumnName, recordId);
            if (pureRowToEdit == null)
            {
                MessageBox.Show("Запись не найдена в базе данных!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            AddRecordWindow addRecordWindow = new AddRecordWindow(pureTable, pureRowToEdit);
            addRecordWindow.Title = "Редактирование записи";
            addRecordWindow.Owner = this;

            bool? dialogResult = addRecordWindow.ShowDialog();
            if (dialogResult == true)
            {
                try
                {
                    Db.EditRecord(addRecordWindow.ResultData, pureTable, CurrentTableName, pureRowToEdit);
                }
                catch (Exception ex)
                {
                    Logger.ErrorLog(ex);
                    MessageBox.Show("Не удалось редактировать запись! Возможно, данные в полях имеют неверный тип.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            RefreshTable();
        }

        private void DeleteRow_Button_Click(object sender, RoutedEventArgs e)
        {
            DataRowView rowView = (DataRowView)MainDataGrid.SelectedItem;
            if (rowView == null )
            {
                MessageBox.Show("Пожалуйста, выбирите запись для удаления!", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            string idColumnName = CurrentTable.Columns[0].ColumnName;
            object recordId = rowView[0];

            string message = $"ID записи: {recordId}";
            DeleteWindow deleteWindow = new DeleteWindow(message);
            bool? dialogResult = deleteWindow.ShowDialog();
            if (dialogResult == true)
            {
                DataTable pureTable = DataBaseManager.GetDataTable(CurrentTableName);
                DataRow pureRowToDelete = DataBaseManager.FindRowById(pureTable, idColumnName, recordId);

                if (pureRowToDelete != null)
                {
                    Db.DeleteRecord(pureTable, pureRowToDelete, CurrentTableName);
                    RefreshTable();
                }
            }
        }

        private void Refresh_Button_Click(object sender, RoutedEventArgs e)
        {
            RefreshTable();
        }

        private void OpenQueriesWindow_Button_Click(object sender, RoutedEventArgs e)
        {
            QueriesWindow queriesWindow = new QueriesWindow(Db);
            queriesWindow.ShowDialog();        
        }
    }
}