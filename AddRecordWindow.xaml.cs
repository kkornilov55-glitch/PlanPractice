using PlanPractice.Logic;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
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

namespace PlanPractice.UI
{
    /// <summary>
    /// Логика взаимодействия для AddRecordWindow.xaml
    /// </summary>
    public partial class AddRecordWindow : Window
    {
        private DataTable CurrentTable;
        private DataRow CurrentRow;

        private Dictionary<string, Control> RawData = new Dictionary<string, Control>();
        public Dictionary<string, string> ResultData = new Dictionary<string, string>();
        public AddRecordWindow(DataTable table, DataRow row = null)
        {
            InitializeComponent();
            CurrentTable = table;
            CurrentRow = row;
            GenerateInterface();
        }
        private void GenerateInterface()
        {
            if (CurrentTable == null) return;

            for (int i = 1; i < CurrentTable.Columns.Count; i++)
            {
                DataColumn column = CurrentTable.Columns[i];
                string colName = column.ColumnName;

                TextBlock label = new TextBlock
                {
                    Text = colName.StartsWith("ID")? GetTableNameFromExternalIdColumn(colName) : colName,
                    FontWeight = FontWeights.SemiBold,
                    Margin = new Thickness(0, 5, 0, 3),
                    FontSize = 13
                };
                FieldsPanel.Children.Add(label);


                if (colName.StartsWith("ID"))
                {
                    string foreignTableName = GetTableNameFromExternalIdColumn(colName);

                    DataTable foreignTable = Db.GetDataTable(foreignTableName);

                    try
                    {
                        ComboBox comboBox = new ComboBox
                        {
                            Height = 25,
                            Margin = new Thickness(0, 0, 0, 12),

                            ItemsSource = foreignTable.DefaultView,

                            DisplayMemberPath = foreignTable.Columns[1].ColumnName,
                            SelectedValuePath = foreignTable.Columns[0].ColumnName
                        };

                        FieldsPanel.Children.Add(comboBox);
                        RawData[colName] = comboBox;

                        if (CurrentRow != null)
                        {
                            comboBox.SelectedValue = Convert.ToInt32(CurrentRow[colName]);
                        }

                        continue;
                    }
                    catch
                    {
                        //Логировать
                    }
                }

                TextBox textBox = new TextBox
                {
                    Height = 25,
                    Margin = new Thickness(0, 0, 0, 12),
                    Padding = new Thickness(4, 2, 4, 2)
                };

                FieldsPanel.Children.Add(textBox);
                RawData[colName] = textBox;

                if (CurrentRow != null)
                {
                    textBox.Text = CurrentRow[colName].ToString();
                }
            }
        }
        private string GetTableNameFromExternalIdColumn(string externalIdColumn)
        {
            //Словарь внешних ключей и названий таблиц с которыми они связаны
            Dictionary<string, string> tableMapper = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "ID роли", "Роли" },
                { "ID подразделения", "Подразделения" },
                { "ID продукции", "Продукция" },
                { "ID сырья", "Сырье" },
                { "ID энергоресурса", "Энергоресурсы" }
            };

            //string tableName = externalIdColumn.Substring(3); //Название без ID, но с маленькой буквы

            //return char.ToUpper(tableName[0]) + tableName.Substring(1);

            return tableMapper[externalIdColumn];
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            GetResultData();
            DialogResult = true;
        }
        private void GetResultData()
        {
            //Пробегаемся по полям записи
            foreach (string col in RawData.Keys)
            {
                //Определив какой Control в этом поле, получаем значение для сохранения в словарь строк(поле-данные)
                string value = string.Empty;
                if (RawData[col] is ComboBox cb)
                {
                    value = cb.SelectedValue.ToString();
                }
                else if (RawData[col] is TextBox tb)
                {
                    value = tb.Text;
                }

                //Сохраняем итоговую запись в словарь
                ResultData[col] = value;
            }
        }
    }
}
