using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using WinForms = System.Windows.Forms;

namespace CodexEdgeGlow
{
    internal static class CalibrationMode
    {
        public static void Run(AppSettings settings, string deviceName)
        {
            settings.EnsureCurrentDisplays();
            var screen = WinForms.Screen.AllScreens.FirstOrDefault(item =>
                string.Equals(item.DeviceName, deviceName, StringComparison.OrdinalIgnoreCase))
                ?? WinForms.Screen.PrimaryScreen;
            if (screen == null) return;
            WinForms.Application.EnableVisualStyles();
            WinForms.Application.SetCompatibleTextRenderingDefault(false);
            WinForms.Application.Run(new CalibrationForm(settings, screen));
        }
    }

    internal sealed class CalibrationForm : WinForms.Form
    {
        private readonly AppSettings _settings;
        private readonly WinForms.Screen _screen;
        private readonly DisplayProfile _profile;
        private readonly TrackBarRow _radius;
        private readonly TrackBarRow _inset;
        private readonly TrackBarRow _thickness;
        private readonly Rectangle _panelBounds;
        private readonly Color _accent;

        public CalibrationForm(AppSettings settings, WinForms.Screen screen)
        {
            _settings = settings;
            _screen = screen;
            _profile = settings.ForScreen(screen);
            _accent = ColorTools.Parse(settings.ColorPrimary, Color.FromArgb(255, 99, 99));

            Text = "Codex Edge Glow Calibration";
            AutoScaleMode = WinForms.AutoScaleMode.None;
            StartPosition = WinForms.FormStartPosition.Manual;
            Bounds = screen.Bounds;
            FormBorderStyle = WinForms.FormBorderStyle.None;
            ShowInTaskbar = true;
            TopMost = true;
            KeyPreview = true;
            BackColor = Color.FromArgb(8, 8, 12);
            Opacity = 0.965;
            Font = new Font("Segoe UI", 9f);
            SetStyle(WinForms.ControlStyles.UserPaint | WinForms.ControlStyles.AllPaintingInWmPaint | WinForms.ControlStyles.OptimizedDoubleBuffer, true);

            var panelWidth = 560;
            var panelHeight = 420;
            _panelBounds = new Rectangle((ClientSize.Width - panelWidth) / 2, (ClientSize.Height - panelHeight) / 2, panelWidth, panelHeight);

            var title = Label("Fit the glow to this display", 20f, FontStyle.Bold, Color.White);
            title.SetBounds(_panelBounds.Left + 34, _panelBounds.Top + 28, panelWidth - 68, 32);
            Controls.Add(title);

            var subtitle = Label("Adjust while watching the live outline. Use − / + or arrow keys for single-pixel precision.", 10f, FontStyle.Regular, Color.FromArgb(171, 171, 184));
            subtitle.SetBounds(_panelBounds.Left + 34, _panelBounds.Top + 66, panelWidth - 68, 38);
            Controls.Add(subtitle);

            var averageInset = (int)Math.Round((_profile.InsetLeft + _profile.InsetTop + _profile.InsetRight + _profile.InsetBottom) / 4.0);
            _radius = AddRow("Corner radius", "Match the physical curve of the panel", _panelBounds.Top + 118, 0, 180, (int)Math.Round(_profile.CornerRadius), "px");
            _inset = AddRow("Edge inset", "Move the full guide away from the bezel", _panelBounds.Top + 202, 0, 100, averageInset, "px");
            _thickness = AddRow("Line width", "See the actual notification thickness", _panelBounds.Top + 286, 3, 24, (int)Math.Round(_settings.Thickness), "px");

            _radius.ValueChanged += UpdateCalibration;
            _inset.ValueChanged += UpdateCalibration;
            _thickness.ValueChanged += UpdateCalibration;

            var cancel = Button("Cancel", false);
            cancel.SetBounds(_panelBounds.Right - 224, _panelBounds.Bottom - 58, 88, 36);
            cancel.Click += delegate { Close(); };
            Controls.Add(cancel);

            var apply = Button("Apply fit", true);
            apply.SetBounds(_panelBounds.Right - 126, _panelBounds.Bottom - 58, 92, 36);
            apply.Click += delegate
            {
                ApplyLiveValues();
                SettingsStore.Save(_settings);
                Close();
            };
            Controls.Add(apply);

            KeyDown += delegate(object sender, WinForms.KeyEventArgs args)
            {
                if (args.KeyCode == WinForms.Keys.Escape) Close();
            };
        }

        private TrackBarRow AddRow(string title, string help, int top, int min, int max, int value, string suffix)
        {
            var row = new TrackBarRow(_accent, title, help, min, max, value, suffix);
            row.SetBounds(_panelBounds.Left + 34, top, _panelBounds.Width - 68, 74);
            Controls.Add(row);
            return row;
        }

        private void UpdateCalibration(object sender, EventArgs args)
        {
            ApplyLiveValues();
            Invalidate();
        }

        private void ApplyLiveValues()
        {
            _profile.CornerRadius = _radius.Value;
            _profile.InsetLeft = _inset.Value;
            _profile.InsetTop = _inset.Value;
            _profile.InsetRight = _inset.Value;
            _profile.InsetBottom = _inset.Value;
            _settings.Thickness = _thickness.Value;
        }

        protected override void OnPaint(WinForms.PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

            var guide = EdgeGeometry.Path(ClientSize, _settings, _profile);
            var radius = (float)Math.Max(0.1, Math.Min(_profile.CornerRadius, Math.Min(guide.Width, guide.Height) / 2));
            using (var path = DrawingTools.RoundedPath(guide, radius))
            using (var glow = new Pen(Color.FromArgb(70, _accent), (float)Math.Max(8, _settings.Thickness * 2.2)))
            using (var line = new Pen(_accent, (float)Math.Max(3, _settings.Thickness)))
            {
                e.Graphics.DrawPath(glow, path);
                e.Graphics.DrawPath(line, path);
            }

            DrawCornerTarget(e.Graphics, guide.Left, guide.Top, 1, 1);
            DrawCornerTarget(e.Graphics, guide.Right, guide.Top, -1, 1);
            DrawCornerTarget(e.Graphics, guide.Left, guide.Bottom, 1, -1);
            DrawCornerTarget(e.Graphics, guide.Right, guide.Bottom, -1, -1);

            using (var shadow = DrawingTools.RoundedPath(new RectangleF(_panelBounds.X - 8, _panelBounds.Y - 8, _panelBounds.Width + 16, _panelBounds.Height + 16), 26))
            using (var shadowBrush = new SolidBrush(Color.FromArgb(90, 0, 0, 0))) e.Graphics.FillPath(shadowBrush, shadow);
            using (var panel = DrawingTools.RoundedPath(_panelBounds, 20))
            using (var fill = new SolidBrush(Color.FromArgb(245, 24, 24, 30)))
            using (var border = new Pen(Color.FromArgb(74, 74, 88), 1))
            {
                e.Graphics.FillPath(fill, panel);
                e.Graphics.DrawPath(border, panel);
            }
        }

        private void DrawCornerTarget(Graphics graphics, float x, float y, int xDirection, int yDirection)
        {
            const float length = 36;
            using (var pen = new Pen(Color.FromArgb(230, _accent), 3))
            {
                pen.StartCap = LineCap.Round;
                pen.EndCap = LineCap.ArrowAnchor;
                graphics.DrawLine(pen, x + (xDirection * 8), y, x + (xDirection * length), y);
                graphics.DrawLine(pen, x, y + (yDirection * 8), x, y + (yDirection * length));
            }
        }

        private static WinForms.Label Label(string text, float size, FontStyle style, Color color)
        {
            return new WinForms.Label
            {
                Text = text,
                Font = new Font("Segoe UI", size, style),
                ForeColor = color,
                BackColor = Color.FromArgb(24, 24, 30),
                AutoEllipsis = true
            };
        }

        private WinForms.Button Button(string text, bool accent)
        {
            var button = new WinForms.Button
            {
                Text = text,
                FlatStyle = WinForms.FlatStyle.Flat,
                Cursor = WinForms.Cursors.Hand,
                BackColor = accent ? _accent : Color.FromArgb(38, 38, 46),
                ForeColor = Color.White,
                TabStop = true
            };
            button.FlatAppearance.BorderColor = accent ? ColorTools.Lerp(_accent, Color.White, 0.16) : Color.FromArgb(70, 70, 82);
            button.FlatAppearance.BorderSize = 1;
            return button;
        }
    }

    internal sealed class TrackBarRow : WinForms.UserControl
    {
        private readonly WinForms.TrackBar _track;
        private readonly WinForms.Label _value;
        public event EventHandler ValueChanged;
        public int Value { get { return _track.Value; } }

        public TrackBarRow(Color accent, string title, string help, int min, int max, int value, string suffix)
        {
            BackColor = Color.FromArgb(24, 24, 30);
            var titleLabel = new WinForms.Label { Text = title, ForeColor = Color.FromArgb(238, 238, 242), Font = new Font("Segoe UI", 9.5f, FontStyle.Bold), BackColor = BackColor };
            titleLabel.SetBounds(0, 0, 126, 22);
            Controls.Add(titleLabel);
            var helpLabel = new WinForms.Label { Text = help, ForeColor = Color.FromArgb(137, 137, 150), Font = new Font("Segoe UI", 8.5f), BackColor = BackColor };
            helpLabel.SetBounds(0, 24, 180, 32);
            Controls.Add(helpLabel);

            var minus = SmallButton("−", accent);
            minus.SetBounds(192, 12, 34, 32);
            Controls.Add(minus);
            _track = new WinForms.TrackBar { Minimum = min, Maximum = max, Value = Math.Max(min, Math.Min(max, value)), TickStyle = WinForms.TickStyle.None, BackColor = BackColor, SmallChange = 1, LargeChange = 5 };
            _track.SetBounds(232, 8, 174, 42);
            Controls.Add(_track);
            var plus = SmallButton("+", accent);
            plus.SetBounds(412, 12, 34, 32);
            Controls.Add(plus);
            _value = new WinForms.Label { TextAlign = ContentAlignment.MiddleRight, ForeColor = Color.FromArgb(210, 210, 218), BackColor = BackColor, Font = new Font("Segoe UI", 9.5f, FontStyle.Bold) };
            _value.SetBounds(452, 12, 40, 32);
            Controls.Add(_value);

            minus.Click += delegate { if (_track.Value > min) _track.Value--; };
            plus.Click += delegate { if (_track.Value < max) _track.Value++; };
            _track.ValueChanged += delegate
            {
                _value.Text = _track.Value + suffix;
                if (ValueChanged != null) ValueChanged(this, EventArgs.Empty);
            };
            _value.Text = _track.Value + suffix;
        }

        private static WinForms.Button SmallButton(string text, Color accent)
        {
            var button = new WinForms.Button
            {
                Text = text,
                FlatStyle = WinForms.FlatStyle.Flat,
                BackColor = Color.FromArgb(42, 42, 50),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                Cursor = WinForms.Cursors.Hand,
                TabStop = false
            };
            button.FlatAppearance.BorderColor = Color.FromArgb(74, 74, 88);
            return button;
        }
    }
}
