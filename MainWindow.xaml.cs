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
using PlanPractice.Logic;

namespace PlanPractice.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {   
        Db Db = new Db();
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
            if (Db.CurrentRole == Db.UserRoles.Admin)
            {
                AddRow_Grid.Visibility = Visibility.Visible;
                DeleteRow_Grid.Visibility = Visibility.Visible;
                EditRow_Grid.Visibility = Visibility.Visible;
            }
            else if (Db.CurrentRole == Db.UserRoles.Manager)
            {
                AddRow_Grid.Visibility = Visibility.Visible;
                EditRow_Grid.Visibility = Visibility.Visible;
            }

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
            CurrentTable = Db.GetDataTable(CurrentTableName);

            //Сортировка записей по ID записи (По возрастанию)
            string firstCol = CurrentTable.Columns[0].ColumnName;
            CurrentTable.DefaultView.Sort = $"{firstCol} ASC";

            MainDataGrid.ItemsSource = CurrentTable.DefaultView;
        }

        private void AddRow_Button_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentTable == null) return;

            AddRecordWindow addRecordWindow = new AddRecordWindow(CurrentTable);
            addRecordWindow.Owner = this;

            bool? dialogResult = addRecordWindow.ShowDialog();
            if (dialogResult == true)
            {
                try
                {
                    Db.AddRecord(addRecordWindow.ResultData, CurrentTable, CurrentTableName);
                }
                catch (Exception ex)
                {
                    //Логирование ошибки
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
            DataRow row = rowView.Row;

            AddRecordWindow addRecordWindow = new AddRecordWindow(CurrentTable, row);
            addRecordWindow.Title = "Редактирование записи";
            addRecordWindow.Owner = this;

            bool? dialogResult = addRecordWindow.ShowDialog();
            if (dialogResult == true)
            {
                try
                {
                    Db.EditRecord(addRecordWindow.ResultData, CurrentTable, CurrentTableName, row);
                }
                catch (Exception ex)
                {
                    //Логирование ошибки
                    MessageBox.Show("Не удалось редактировать запись! Возможно, данные в полях имеют неверный тип.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            RefreshTable();
        }

        private void DeleteRow_Button_Click(object sender, RoutedEventArgs e)
        {
            DataRowView rowView = (DataRowView)MainDataGrid.SelectedItem;
            DataRow row = rowView.Row;

            string message = $"ID записи: {row[0]}";
            DeleteWindow deleteWindow = new DeleteWindow(message);
            bool? dialogResult = deleteWindow.ShowDialog();
            if (dialogResult == true)
            {
                Db.DeleteRecord(CurrentTable, row, CurrentTableName);
            }
        }

        private void Refresh_Button_Click(object sender, RoutedEventArgs e)
        {
            RefreshTable();
        }
    }
}