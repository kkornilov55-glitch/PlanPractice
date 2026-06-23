using Microsoft.Win32;
using PlanPractice.Logic;
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
using System.Linq;
using System.IO;

namespace PlanPractice.UI
{
    /// <summary>
    /// Логика взаимодействия для QueriesWindow.xaml
    /// </summary>
    public partial class QueriesWindow : Window
    {
        private DataBaseManager Db;
        public QueriesWindow(DataBaseManager db)
        {
            InitializeComponent();
            Db = db;

            this.DataContext = this;
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

            if (DataBaseManager.TryExecuteQuery(query, ref dt))
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
        private void ExportReport_Button_Click(object sender, RoutedEventArgs e)
        {
            DataView dataView = (DataView)QueryResultGrid.ItemsSource;

            //Проверяем, есть ли вообще данные в таблице
            if (QueryResultGrid.ItemsSource == null)
            {
                MessageBox.Show("Сначала выполните запрос, чтобы получить данные для отчёта!", "Внимание", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            DataTable table = dataView.Table;
            if (table.Rows.Count == 0)
            {
                MessageBox.Show("Таблица пуста, нечего экспортировать.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            //Открываем окно сохранения файла
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "CSV файл|*.csv";
            saveFileDialog.Title = "Сохранить отчёт";
            saveFileDialog.FileName = "Отчет.csv";

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    StringBuilder sb = new StringBuilder();
                    //Формируем заголовки колонок (через точку с запятой)
                    string[] columnNames = table.Columns.Cast<DataColumn>().Select(column => column.ColumnName).ToArray();
                    sb.AppendLine(string.Join(";", columnNames));

                    //Пробегаемся по всем строкам с данными
                    foreach (DataRow row in table.Rows)
                    {
                        string[] fields = row.ItemArray.Select(field => field.ToString().Replace(";", ",")).ToArray();
                        sb.AppendLine(string.Join(";", fields));
                    }

                    //Сохраняем файл
                    File.WriteAllText(saveFileDialog.FileName, sb.ToString(), Encoding.UTF8);

                    MessageBox.Show("Отчёт успешно сформирован и сохранен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при сохранении отчёта: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
