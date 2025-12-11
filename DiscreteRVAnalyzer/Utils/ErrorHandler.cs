using System;
using System.IO;
using System.Windows.Forms;

namespace DiscreteRVAnalyzer.Utils
{
    /// <summary>
    /// Центральний обробник помилок з логуванням
    /// </summary>
    public static class ErrorHandler
    {
        private const string LogFileName = "error_log.txt";

        static ErrorHandler()
        {
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
            Application.ThreadException += OnThreadException;
        }

        private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = e.ExceptionObject as Exception;
            LogError(ex, "Неперехоплена виняткова ситуація");
        }

        private static void OnThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            LogError(e.Exception, "Помилка в UI-потоці");
        }

        public static void LogError(Exception? ex, string context = "Помилка")
        {
            try
            {
                string message = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {context}\n" +
                                 $"Тип: {ex?.GetType().Name ?? "-"}\n" +
                                 $"Повідомлення: {ex?.Message ?? "-"}\n" +
                                 $"Стек: {ex?.StackTrace ?? "-"}\n\n";

                File.AppendAllText(LogFileName, message);
            }
            catch
            {
                // Ігноруємо помилки логування
            }
        }

        public static void ShowUserError(string message, string title = "Помилка")
        {
            MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public static void ShowUserWarning(string message, string title = "Попередження")
        {
            MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        public static void ShowUserInfo(string message, string title = "Інформація")
        {
            MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
