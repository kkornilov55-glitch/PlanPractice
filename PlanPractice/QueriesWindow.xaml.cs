using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using PlanPractice.Logic;

namespace PlanPractice.UI
{
    /// <summary>
    /// Логика взаимодействия для QueriesWindow.xaml
    /// </summary>
    public partial class QueriesWindow : Window
    {
        private Db Db;
        public QueriesWindow(Db db)
        {
            InitializeComponent();
            Db = db;

            this.DataContext = this;
        }

        private void ExportReport_Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ExecutePredefined_Button_Click(object sender, RoutedEventArgs e)
        {
            string query = string.Empty;

            switch (PredefinedQueriesList.SelectedIndex)
            {
                case -1:
                    MessageBox.Show("Выберите запрос из списка!", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                case 0:
                    query = @"SELECT Продукция.[Название продукции], Продукция.Себестоимость, Продукция.Расценка, Продукция.[Плановое задание], Подразделения.Подразделение
                            FROM Продукция
                            INNER JOIN Подразделения ON Продукция.[ID подразделения] = Подразделения.[ID подразделения]";
                    break;
                case 1:
                    query = @"SELECT [Название продукции], Себестоимость, [Плановое задание], (Себестоимость * [Плановое задание]) AS [Общая стоимость]
                            FROM Продукция";
                    break;
                case 2:
                    query = @"SELECT Продукция.[Название продукции], Сырье.[Наименование сырья], [Продукция-сырье].[НП сырья]
                            FROM (Продукция
                            INNER JOIN [Продукция-сырье] ON Продукция.[ID продукции] = [Продукция-сырье].[ID продукции])
                            INNER JOIN Сырье ON [Продукция-сырье].[ID сырья] = Сырье.[ID сырья]";
                    break;
                case 3:
                    query = @"SELECT Подразделения.Подразделение, COUNT(Продукция.[ID продукции]) AS [Количество видов продукции]
                            FROM Подразделения 
                            LEFT JOIN Продукция ON Подразделения.[ID подразделения] = Продукция.[ID подразделения]
                            GROUP BY Подразделения.Подразделение";
                    break;
                case 4:
                    query = @"SELECT Продукция.[Название продукции], Энергоресурсы.Энергоресурс, [Продукция-энергоресурс].[НП энергоресурса]
                            FROM (Продукция 
                            INNER JOIN [Продукция-энергоресурс] ON Продукция.[ID продукции] = [Продукция-энергоресурс].[ID продукции]) 
                            INNER JOIN Энергоресурсы ON [Продукция-энергоресурс].[ID энергоресурса] = Энергоресурсы.[ID энергоресурса]";
                    break;
            }
            ExecuteQuery(query);
        }

        private void ExecuteCustom_Button_Click(object sender, RoutedEventArgs e)
        {
            ExecuteQuery(CustomSql_TextBox.Text);
        }

        private void ExecuteQuery(string query)
        {
            DataTable dt = new DataTable();

            if (Db.TryExecuteQuery(query, ref dt))
            {
                QueryResultGrid.ItemsSource = null;
                QueryResultGrid.ItemsSource = dt.DefaultView;
            }
            else
            {
                QueryResultGrid.ItemsSource = null;
                MessageBox.Show("Запрос отклонён! ", "Ошибка выполнения запроса", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
