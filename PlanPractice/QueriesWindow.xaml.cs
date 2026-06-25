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
using System.Linq;
using System.IO;
using Word = Microsoft.Office.Interop.Word;
using Excel = Microsoft.Office.Interop.Excel;

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
        //private void ExportReport_Button_Click(object sender, RoutedEventArgs e)
        //{
        //    DataView dataView = (DataView)QueryResultGrid.ItemsSource;

        //    //Проверяем, есть ли вообще данные в таблице
        //    if (QueryResultGrid.ItemsSource == null)
        //    {
        //        MessageBox.Show("Сначала выполните запрос, чтобы получить данные для отчёта!", "Внимание", MessageBoxButton.OK, MessageBoxImage.Information);
        //        return;
        //    }

        //    DataTable table = dataView.Table;
        //    if (table.Rows.Count == 0)
        //    {
        //        MessageBox.Show("Таблица пуста, нечего экспортировать.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
        //        return;
        //    }

        //    //Открываем окно сохранения файла
        //    SaveFileDialog saveFileDialog = new SaveFileDialog();
        //    saveFileDialog.Filter = "CSV файл|*.csv";
        //    saveFileDialog.Title = "Сохранить отчёт";
        //    saveFileDialog.FileName = "Отчет.csv";

        //    if (saveFileDialog.ShowDialog() == true)
        //    {
        //        try
        //        {
        //            StringBuilder sb = new StringBuilder();
        //            //Формируем заголовки колонок (через точку с запятой)
        //            string[] columnNames = table.Columns.Cast<DataColumn>().Select(column => column.ColumnName).ToArray();
        //            sb.AppendLine(string.Join(";", columnNames));

        //            //Пробегаемся по всем строкам с данными
        //            foreach (DataRow row in table.Rows)
        //            {
        //                string[] fields = row.ItemArray.Select(field => field.ToString().Replace(";", ",")).ToArray();
        //                sb.AppendLine(string.Join(";", fields));
        //            }

        //            //Сохраняем файл
        //            File.WriteAllText(saveFileDialog.FileName, sb.ToString(), Encoding.UTF8);

        //            MessageBox.Show("Отчёт успешно сформирован и сохранен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

        //        }
        //        catch (Exception ex)
        //        {
        //            MessageBox.Show("Ошибка при сохранении отчёта: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        //        }
        //    }
        //}
        private void ExportReport_Button_Click(object sender, RoutedEventArgs e)
        {
            if (QueryResultGrid.ItemsSource == null)
            {
                MessageBox.Show("Нет данных для экспорта!", "Внимание", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            DataView dataView = (DataView)QueryResultGrid.ItemsSource;
            DataTable table = dataView.Table;

            if (table.Rows.Count == 0)
            {
                MessageBox.Show("Таблица пуста, нечего экспортировать.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Title = "Сохранить отчёт";
            // Изменили CSV на нормальный XLSX
            saveFileDialog.Filter = "Книга Excel (*.xlsx)|*.xlsx|Документ Word (*.docx)|*.docx";
            saveFileDialog.FileName = "Отчет";

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    string extension = System.IO.Path.GetExtension(saveFileDialog.FileName).ToLower();

                    if (extension == ".xlsx")
                    {
                        ExportToExcel(table, saveFileDialog.FileName);
                    }
                    else if (extension == ".docx")
                    {
                        ExportToWord(table, saveFileDialog.FileName);
                    }

                    MessageBox.Show("Отчёт успешно сформирован и сохранен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при сохранении отчёта: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // --- Алгоритм экспорта в Excel (CSV) ---
        private void ExportToExcel(DataTable table, string filePath)
        {
            Excel.Application excelApp = new Excel.Application();
            excelApp.Visible = false;
            excelApp.DisplayAlerts = false; // Отключаем лишние предупреждения

            Excel.Workbook workbook = excelApp.Workbooks.Add();
            Excel.Worksheet worksheet = (Excel.Worksheet)workbook.ActiveSheet;

            // 1. Заполняем заголовки
            for (int i = 0; i < table.Columns.Count; i++)
            {
                worksheet.Cells[1, i + 1] = table.Columns[i].ColumnName;
                worksheet.Cells[1, i + 1].Font.Bold = true; // Делаем жирным
            }

            // 2. Быстрое заполнение данных через двумерный массив
            object[,] dataArr = new object[table.Rows.Count, table.Columns.Count];
            for (int i = 0; i < table.Rows.Count; i++)
            {
                for (int j = 0; j < table.Columns.Count; j++)
                {
                    dataArr[i, j] = table.Rows[i][j].ToString();
                }
            }

            // Вставляем массив разом (это в разы быстрее)
            Excel.Range startCell = (Excel.Range)worksheet.Cells[2, 1];
            Excel.Range endCell = (Excel.Range)worksheet.Cells[table.Rows.Count + 1, table.Columns.Count];
            Excel.Range writeRange = worksheet.Range[startCell, endCell];
            writeRange.Value2 = dataArr;

            // 3. Автоматическая ширина колонок (AutoFit)
            worksheet.Columns.AutoFit();

            // 4. Рисуем границы таблицы
            Excel.Range allRange = worksheet.Range[worksheet.Cells[1, 1], endCell];
            allRange.Borders.LineStyle = Excel.XlLineStyle.xlContinuous;

            // Сохраняем и закрываем
            workbook.SaveAs(filePath, Excel.XlFileFormat.xlOpenXMLWorkbook);
            workbook.Close();
            excelApp.Quit();
        }

        // --- Алгоритм экспорта в Word (DOCX) ---
        private void ExportToWord(DataTable table, string filePath)
        {
            //Запускаем фоновый процесс Word
            Word.Application wordApp = new Word.Application();
            wordApp.Visible = false; // Не показываем окно Word пользователю во время генерации

            //Создаем новый пустой документ
            Word.Document wordDoc = wordApp.Documents.Add();

            //Добавляем заголовок с датой
            Word.Paragraph para = wordDoc.Content.Paragraphs.Add();
            para.Range.Text = $"Отчет от {DateTime.Now.ToString("dd.MM.yyyy HH:mm")}";
            para.Range.InsertParagraphAfter();

            //Создаем таблицу в документе
            Word.Range tableRange = wordDoc.Bookmarks.get_Item("\\endofdoc").Range;
            Word.Table wordTable = wordDoc.Tables.Add(tableRange, table.Rows.Count + 1, table.Columns.Count);

            //Красивый стиль таблицы (сетка)
            wordTable.Borders.Enable = 1;

            //Заполняем заголовки
            for (int i = 0; i < table.Columns.Count; i++)
            {
                wordTable.Cell(1, i + 1).Range.Text = table.Columns[i].ColumnName;
                wordTable.Cell(1, i + 1).Range.Bold = 1; //Делаем заголовки жирным
            }

            //Заполняем данные
            for (int m = 0; m < table.Rows.Count; m++)
            {
                for (int n = 0; n < table.Columns.Count; n++)
                {
                    wordTable.Cell(m + 2, n + 1).Range.Text = table.Rows[m][n].ToString();
                }
            }

            //Сохраняем и закрываем
            wordDoc.SaveAs2(filePath);
            wordDoc.Close();
            wordApp.Quit();
        }
    }
}
