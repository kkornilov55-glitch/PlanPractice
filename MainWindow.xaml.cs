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
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Connect_Button_Click(object sender, RoutedEventArgs e)
        {
            AuthWindow auth = new AuthWindow(Db);
            auth.Show();
        }

        private void TablesTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {

        }

        private void AddRow_Button_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}