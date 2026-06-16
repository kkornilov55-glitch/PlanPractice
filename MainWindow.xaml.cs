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
            AuthWindow auth = new AuthWindow(Db);
            auth.Owner = this;

            bool? DialogResult = auth.ShowDialog();
            if (DialogResult == true)
            {
                TableNames = Db.GetListTableNames();
                TablesTreeView.ItemsSource = null;
                TablesTreeView.ItemsSource = TableNames;
            }
        }

        private void TablesTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {

        }

        private void AddRow_Button_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}