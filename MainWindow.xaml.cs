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
            }
        }
        private void Disconnect_Button_Click(object sender, RoutedEventArgs e)
        {
            DataBaseName_TextBox.Text = string.Empty;
            TablesTreeView.ItemsSource = null;
            MainDataGrid.ItemsSource = null;

            DisconnectButtonGrid.Visibility = Visibility.Collapsed;
            ConnectButtonGrid.Visibility = Visibility.Visible;

            MessageBoxResult result = MessageBox.Show("Произведено отключение от БД. Пожалуйста, авторизуйтесь повторно для продолжения работы!", "Отключение от БД", MessageBoxButton.OKCancel, MessageBoxImage.Information);
            if (result == MessageBoxResult.OK)
                Connect();

        }
        private void TablesTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (TablesTreeView.SelectedItem == null) return;
            string tableName = TablesTreeView.SelectedItem.ToString();

            //Вывод таблицы
            DataTable dataTable = Db.GetDataTable(tableName);
            MainDataGrid.ItemsSource = dataTable.DefaultView;
        }

        private void AddRow_Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void EditRow_Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void DeleteRow_Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Refresh_Button_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}