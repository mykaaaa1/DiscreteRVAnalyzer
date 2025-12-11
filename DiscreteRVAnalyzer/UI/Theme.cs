using System.Drawing;
using System.Windows.Forms;

namespace DiscreteRVAnalyzer.UI
{
    internal enum ThemeMode { Light, Dark }

    internal static class Theme
    {
        // Light palette
        private static readonly Color LightBackground = Color.FromArgb(250, 251, 253);
        private static readonly Color LightPanel = Color.FromArgb(245, 247, 250);
        private static readonly Color LightPrimary = Color.FromArgb(30, 41, 59);
        private static readonly Color LightAccent = Color.FromArgb(14, 165, 233);
        private static readonly Color LightSuccess = Color.FromArgb(34, 197, 94);
        private static readonly Color LightMuted = Color.FromArgb(99, 102, 110);

        // Dark palette
        private static readonly Color DarkBackground = Color.FromArgb(15, 23, 42);
        private static readonly Color DarkPanel = Color.FromArgb(20, 28, 48);
        private static readonly Color DarkPrimary = Color.FromArgb(226, 232, 240);
        private static readonly Color DarkAccent = Color.FromArgb(56, 189, 248);
        private static readonly Color DarkSuccess = Color.FromArgb(34, 197, 94);
        private static readonly Color DarkMuted = Color.FromArgb(148, 163, 184);

        public static void Apply(Form form)
        {
            Apply(form, ThemeMode.Light);
        }

        public static void Apply(Form form, ThemeMode mode)
        {
            if (form == null) return;

            var isDark = mode == ThemeMode.Dark;
            var bg = isDark ? DarkBackground : LightBackground;
            var panel = isDark ? DarkPanel : LightPanel;
            var primary = isDark ? DarkPrimary : LightPrimary;
            var accent = isDark ? DarkAccent : LightAccent;
            var success = isDark ? DarkSuccess : LightSuccess;
            var muted = isDark ? DarkMuted : LightMuted;

            form.SuspendLayout();
            form.BackColor = bg;
            form.ForeColor = primary;
            form.Font = new Font("Segoe UI", 9F);

            foreach (Control c in form.Controls)
            {
                // MenuStrip
                if (c is MenuStrip ms)
                {
                    ms.BackColor = panel;
                    ms.ForeColor = primary;
                    ms.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
                }

                // ToolStrip
                if (c is ToolStrip ts)
                {
                    ts.BackColor = panel;
                    ts.ForeColor = primary;
                    ts.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
                    foreach (ToolStripItem item in ts.Items)
                    {
                        item.ForeColor = primary;
                        if (item is ToolStripButton b)
                        {
                            b.BackColor = Color.Transparent;
                            b.Padding = new Padding(6, 2, 6, 2);
                        }
                        if (item is ToolStripComboBox cb && cb.ComboBox != null)
                        {
                            cb.ComboBox.BackColor = panel;
                            cb.ComboBox.ForeColor = primary;
                            cb.ComboBox.Font = new Font("Segoe UI", 9F);
                        }
                    }
                }

                if (c is StatusStrip ss)
                {
                    ss.BackColor = panel;
                    ss.ForeColor = muted;
                }

                if (c.HasChildren)
                    ApplyToChildren(c, panel, primary, accent, success, muted, isDark);
            }

            form.ResumeLayout();
        }

        private static void ApplyToChildren(Control parent, Color panel, Color primary, Color accent, Color success, Color muted, bool isDark)
        {
            foreach (Control ctrl in parent.Controls)
            {
                // panels and group boxes
                if (ctrl is GroupBox gb)
                {
                    gb.BackColor = panel;
                    gb.ForeColor = primary;
                    gb.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
                }
                else if (ctrl is Panel pnl)
                {
                    pnl.BackColor = Color.Transparent;
                }
                else if (ctrl is Button btn)
                {
                    btn.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
                    btn.ForeColor = Color.White;
                    // use accent for light mode, success for calculate, but keep readable in dark
                    btn.BackColor = accent;
                    btn.FlatStyle = FlatStyle.Flat;
                    btn.FlatAppearance.BorderSize = 0;
                    btn.Padding = new Padding(8, 6, 8, 6);
                }
                else if (ctrl is TextBox tb)
                {
                    // use panel background for dark mode so text remains readable
                    tb.BackColor = isDark ? panel : Color.White;
                    tb.ForeColor = primary;
                    tb.BorderStyle = BorderStyle.FixedSingle;
                    tb.Font = new Font("Segoe UI", 9F);
                }
                else if (ctrl is ListView lv)
                {
                    lv.BackColor = panel;
                    lv.ForeColor = primary;
                    lv.Font = new Font("Segoe UI", 9F);
                    lv.FullRowSelect = true;
                    lv.GridLines = false;
                    lv.View = View.Details;
                    lv.BorderStyle = BorderStyle.None;
                    lv.HideSelection = false;
                }
                else if (ctrl is DataGridView dgv)
                {
                    dgv.BackgroundColor = Color.White;
                    dgv.EnableHeadersVisualStyles = false;
                    dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(240, 242, 245);
                    dgv.ColumnHeadersDefaultCellStyle.ForeColor = primary;
                    dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
                    dgv.DefaultCellStyle.BackColor = Color.White;
                    dgv.DefaultCellStyle.ForeColor = primary;
                    dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(250, 251, 253);
                    dgv.RowHeadersVisible = false;
                    dgv.BorderStyle = BorderStyle.None;
                }
                else if (ctrl is TabControl tc)
                {
                    tc.Font = new Font("Segoe UI", 9F);
                }

                if (ctrl.HasChildren)
                    ApplyToChildren(ctrl, panel, primary, accent, success, muted, isDark);
            }
        }
    }
}
