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

namespace PlanPractice.UI
{
    /// <summary>
    /// Логика взаимодействия для AddRecordWindow.xaml
    /// </summary>
    public partial class AddRecordWindow : Window
    {
        private DataTable CurrentTable;
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
                    Text = column.ColumnName + ":",
                    FontWeight = FontWeights.SemiBold,
                    Margin = new Thickness(0, 5, 0, 3),
                    FontSize = 13
                };
                FieldsPanel.Children.Add(label);


                if (column.ColumnName.StartsWith("ID"))
                {
                    string tableName = GetTableNameFromExternalIdColumn(column.ColumnName);

                    try
                    {
                        ComboBox comboBox = new ComboBox
                        {
                            Height = 25,
                            Margin = new Thickness(0, 0, 0, 12),

                            ItemsSource = CurrentTable.DefaultView,

                            DisplayMemberPath = column.ColumnName,
                            SelectedValuePath = column.ColumnName
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
            string tableName = externalIdColumn.Substring(3); //Название без ID, но с маленькой буквы

            return char.ToUpper(tableName[0]) + tableName.Substring(1);
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
