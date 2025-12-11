using System;
using System.IO;
using System.Windows.Forms;

namespace DiscreteRVAnalyzer.Utils
{
    /// <summary>
    /// Центральный обработчик ошибок с логированием
    /// </summary>
    public static class ErrorHandler
    {
        private const string LogFileName = "error_log.txt";

        static ErrorHandler()
        {
            // Глобальный обработчик необработанных исключений
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
            Application.ThreadException += OnThreadException;
        }

        private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = e.ExceptionObject as Exception;
            LogError(ex, "Необработанное исключение");
        }

        private static void OnThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            LogError(e.Exception, "Ошибка в потоке UI");
        }

        public static void LogError(Exception ex, string context = "Ошибка")
        {
            try
            {
                string message = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {context}\n" +
                                 $"Тип: {ex?.GetType().Name}\n" +
                                 $"Сообщение: {ex?.Message}\n" +
                                 $"Стек: {ex?.StackTrace}\n\n";

                File.AppendAllText(LogFileName, message);
            }
            catch
            {
                // Игнорируем ошибки логирования
            }
        }

        public static void ShowUserError(string message, string title = "Ошибка")
        {
            MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public static void ShowUserWarning(string message, string title = "Предупреждение")
        {
            MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        public static void ShowUserInfo(string message, string title = "Информация")
        {
            MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
