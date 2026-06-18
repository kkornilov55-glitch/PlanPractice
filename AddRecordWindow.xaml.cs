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
        private OleDbConnection Connection;
        public Dictionary<string, string> ResultData = new Dictionary<string, string>();
        public AddRecordWindow(DataTable table)
        {
            InitializeComponent();
            CurrentTable = table;
            GenerateInterface();
        }
        private void GenerateInterface()
        {
            if (CurrentTable == null) return;

            for (int i = 1; i < CurrentTable.Columns.Count; i++)
            {
                DataColumn column = CurrentTable.Columns[i];

                TextBlock label = new TextBlock
                {
                    Text = column.ColumnName.StartsWith("ID")? GetTableNameFromExternalIdColumn(column.ColumnName) : column.ColumnName + ":",
                    FontWeight = FontWeights.SemiBold,
                    Margin = new Thickness(0, 5, 0, 3),
                    FontSize = 13
                };
                FieldsPanel.Children.Add(label);


                if (column.ColumnName.StartsWith("ID"))
                {
                    string foreignTableName = GetTableNameFromExternalIdColumn(column.ColumnName);

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
            DialogResult = false;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
