using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace DiscreteRVAnalyzer.UI
{
    // Guided overlay form. Shows message and highlights a target rectangle.
    public class FirstRunCoachForm : Form
    {
        private Label messageLabel;
        private Button nextButton;
        private Button skipButton;
        private Rectangle highlightRect;

        private static bool _isRunning = false; // prevent concurrent sequences

        public FirstRunCoachForm(string message, Rectangle targetBounds)
        {
            FormBorderStyle = FormBorderStyle.None;
            BackColor = Color.Black;
            Opacity = 1.0; // draw custom translucent background so child controls stay opaque
            StartPosition = FormStartPosition.Manual;
            ShowInTaskbar = false;
            TopMost = true;

            var screen = Screen.FromPoint(new Point(targetBounds.Left, targetBounds.Top));
            Bounds = screen.Bounds;

            highlightRect = targetBounds;

            messageLabel = new Label
            {
                AutoSize = false,
                Size = new Size(420, 120),
                Location = PlaceMessageNear(targetBounds, screen.Bounds),
                BackColor = Color.White,
                ForeColor = Color.Black,
                Padding = new Padding(12),
                Text = message,
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                BorderStyle = BorderStyle.FixedSingle,
                TextAlign = ContentAlignment.MiddleLeft
            };

            nextButton = new Button
            {
                Text = "Далее",
                AutoSize = true,
                BackColor = Color.White,
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Standard
            };
            nextButton.Click += (s, e) => Close();

            skipButton = new Button
            {
                Text = "Пропустить",
                AutoSize = true,
                BackColor = Color.White,
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Standard,
                Location = new Point(messageLabel.Left + 10, messageLabel.Bottom - 35)
            };
            skipButton.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };

            // position next button
            nextButton.Location = new Point(messageLabel.Right - 90, messageLabel.Bottom - 35);

            Controls.Add(messageLabel);
            Controls.Add(nextButton);
            Controls.Add(skipButton);

            // ensure overlay controls are on top
            messageLabel.BringToFront();
            nextButton.BringToFront();
            skipButton.BringToFront();

            // allow Esc to cancel
            KeyPreview = true;
            KeyDown += (s, e) => { if (e.KeyCode == Keys.Escape) { DialogResult = DialogResult.Cancel; Close(); } };

            // Make form layered so we can draw and allow click-through behavior via WndProc
            SetStyle(ControlStyles.Opaque, true);
        }

        private Point PlaceMessageNear(Rectangle target, Rectangle screen)
        {
            // Try to place to the right of target, otherwise left, otherwise above
            int margin = 12;
            int width = 420;
            int height = 120;
            int x = target.Right + margin;
            int y = target.Top;
            if (x + width > screen.Right)
            {
                x = target.Left - margin - width;
                if (x < screen.Left) x = screen.Left + 20;
            }
            if (y + height > screen.Bottom)
            {
                y = screen.Bottom - height - 20;
            }
            if (y < screen.Top) y = screen.Top + 20;
            return new Point(x, y);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            // Draw translucent background over entire client area (less dark)
            using (var brush = new SolidBrush(Color.FromArgb(120, 0, 0, 0)))
            {
                e.Graphics.FillRectangle(brush, ClientRectangle);
            }

            // Draw highlight rectangle (bright border)
            using (var pen = new Pen(Color.Yellow, 3))
            {
                e.Graphics.DrawRectangle(pen, highlightRect);
            }

            // Draw an arrow from message box to rectangle (simple line)
            var start = new Point(messageLabel.Left + 20, messageLabel.Top + messageLabel.Height / 2);
            var end = new Point(highlightRect.Left + highlightRect.Width / 2, highlightRect.Top + highlightRect.Height / 2);
            using (var pen = new Pen(Color.White, 2))
            {
                e.Graphics.DrawLine(pen, start, end);
            }
        }

        // Intercept hit-test to allow clicks to pass through overlay outside the interactive area
        protected override void WndProc(ref Message m)
        {
            const int WM_NCHITTEST = 0x84;
            const int HTTRANSPARENT = -1;

            if (m.Msg == WM_NCHITTEST)
            {
                // lParam contains screen coordinates
                int lParam = m.LParam.ToInt32();
                int x = (short)(lParam & 0xFFFF);
                int y = (short)((lParam >> 16) & 0xFFFF);
                var clientPt = PointToClient(new Point(x, y));

                // If the point is inside messageLabel or buttons, let the form handle it
                if (messageLabel.Bounds.Contains(clientPt) || nextButton.Bounds.Contains(clientPt) || skipButton.Bounds.Contains(clientPt))
                {
                    base.WndProc(ref m);
                    return;
                }

                // Otherwise make it transparent so underlying window receives the mouse
                m.Result = new IntPtr(HTTRANSPARENT);
                return;
            }

            base.WndProc(ref m);
        }

        // Show a sequence of steps. Each step is a message + target (Control or ToolStripItem). Returns true if completed, false if skipped.
        public static bool RunSequence(Form owner, IEnumerable<(string message, object target)> steps)
        {
            if (_isRunning) return false;
            _isRunning = true;

            try
            {
                foreach (var step in steps)
                {
                    Rectangle screenRect;
                    if (step.target == null)
                        continue;

                    if (step.target is Control ctrl)
                    {
                        if (ctrl.IsDisposed) continue;
                        screenRect = ctrl.RectangleToScreen(new Rectangle(0, 0, ctrl.Width, ctrl.Height));
                    }
                    else if (step.target is ToolStripItem tsi)
                    {
                        var ownerStrip = tsi.Owner as ToolStrip;
                        if (ownerStrip == null) continue;
                        if (ownerStrip.IsDisposed) continue;
                        var bounds = tsi.Bounds; // relative to ownerStrip
                        var location = ownerStrip.PointToScreen(bounds.Location);
                        screenRect = new Rectangle(location, bounds.Size);
                    }
                    else
                    {
                        continue;
                    }

                    using (var overlay = new FirstRunCoachForm(step.message, screenRect))
                    {
                        // Attach interaction handlers so overlay closes automatically when user interacts with the target
                        Action detach = null;

                        void CloseOverlaySafe()
                        {
                            try { if (overlay.Visible) overlay.BeginInvoke(new Action(() => overlay.Close())); }
                            catch { }
                        }

                        try
                        {
                            if (step.target is Control targetCtrl)
                            {
                                if (targetCtrl is TabControl tc)
                                {
                                    EventHandler h = (s, e) => CloseOverlaySafe();
                                    tc.SelectedIndexChanged += h;
                                    detach = () => tc.SelectedIndexChanged -= h;
                                }
                                else if (targetCtrl is DataGridView dgv)
                                {
                                    DataGridViewCellEventHandler h = (s, e) => CloseOverlaySafe();
                                    DataGridViewCellEventHandler h2 = (s, e) => CloseOverlaySafe();
                                    dgv.CellClick += h;
                                    dgv.CellValueChanged += h2;
                                    detach = () => { dgv.CellClick -= h; dgv.CellValueChanged -= h2; };
                                }
                                else if (targetCtrl is TextBox tb)
                                {
                                    // close when user focuses or clicks the textbox so they can type
                                    EventHandler focus = (s, e) => CloseOverlaySafe();
                                    EventHandler click = (s, e) => CloseOverlaySafe();
                                    tb.GotFocus += focus;
                                    tb.Click += click;
                                    detach = () => { tb.GotFocus -= focus; tb.Click -= click; };
                                }
                                else if (targetCtrl is ComboBox cb)
                                {
                                    // close when dropdown opens so it shows above overlay
                                    EventHandler opened = (s, e) => CloseOverlaySafe();
                                    cb.DropDown += opened;
                                    detach = () => cb.DropDown -= opened;
                                }
                                else
                                {
                                    // generic click
                                    MouseEventHandler h = (s, e) => CloseOverlaySafe();
                                    targetCtrl.MouseDown += h;
                                    detach = () => targetCtrl.MouseDown -= h;
                                }
                            }
                            else if (step.target is ToolStripItem tsi2)
                            {
                                if (tsi2 is ToolStripComboBox tsc)
                                {
                                    EventHandler h = (s, e) => CloseOverlaySafe();
                                    if (tsc.ComboBox != null)
                                    {
                                        // close when dropdown opens so it shows above overlay
                                        tsc.ComboBox.DropDown += h;
                                        detach = () => tsc.ComboBox.DropDown -= h;
                                    }
                                }
                                else if (tsi2 is ToolStripButton tsb)
                                {
                                    EventHandler h = (s, e) => CloseOverlaySafe();
                                    tsb.Click += h;
                                    detach = () => tsb.Click -= h;
                                }
                                else
                                {
                                    EventHandler h = (s, e) => CloseOverlaySafe();
                                    tsi2.Click += h;
                                    detach = () => tsi2.Click -= h;
                                }
                            }

                            // show non-modal so underlying controls remain interactive
                            overlay.Show(owner);

                            // wait until overlay closed
                            while (overlay.Visible)
                            {
                                Application.DoEvents();
                                Thread.Sleep(10);
                            }
                        }
                        finally
                        {
                            // detach handlers
                            try { detach?.Invoke(); } catch { }
                        }

                        if (overlay.DialogResult == DialogResult.Cancel)
                            return false; // user skipped
                    }
                }
            }
            finally
            {
                _isRunning = false;
            }

            return true;
        }
    }
}
