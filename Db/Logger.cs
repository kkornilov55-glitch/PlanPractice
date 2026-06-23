using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plan.Logic
{
    public class Logger
    {
        private const string ErrorFile = "error.log";
        private const string InfoFile = "info.log";
        public static void ErrorLog(Exception ex)
        {
            string logText = $"{DateTime.Now:G} Сообщение: {ex.Message}\n" +
                             $"Стек: {ex.StackTrace}\n" +
                             new string('-', 40) + '\n';
            File.AppendAllText(ErrorFile, logText);
        }
        public static void InfoLog(string message)
        {
            string logText = $"{DateTime.Now:G} Инфо: {message}\n";
            File.AppendAllText(InfoFile, logText);
        }
    }
}
