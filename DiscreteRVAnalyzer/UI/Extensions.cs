using System;
using System.Drawing;
using System.Windows.Forms;

namespace DiscreteRVAnalyzer.UI.Extensions
{
    /// <summary>
    /// Расширение для цветного вывода в RichTextBox
    /// </summary>
    public static class RichTextBoxExtensions
    {
        public static void AppendText(this RichTextBox rtb, string text, Color color)
        {
            rtb.SelectionStart = rtb.TextLength;
            rtb.SelectionLength = 0;
            rtb.SelectionColor = color;
            rtb.AppendText(text);
            rtb.SelectionColor = rtb.ForeColor;
        }

        public static void AppendLineFormatted(this RichTextBox rtb, string text, Color color)
        {
            AppendText(rtb, text + Environment.NewLine, color);
        }
    }
}
