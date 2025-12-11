using System;
using System.Windows.Forms;

namespace DiscreteRVAnalyzer
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            try
            {
                Application.Run(new DiscreteRVAnalyzer.UI.MainForm());
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Критическая ошибка:\n{ex.Message}\n\n{ex.StackTrace}",
                    "Ошибка приложения",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
    }
}
