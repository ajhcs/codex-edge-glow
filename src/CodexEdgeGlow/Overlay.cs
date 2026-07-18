using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using WinForms = System.Windows.Forms;

namespace CodexEdgeGlow
{
    internal static class OverlaySession
    {
        public static void RunStandalone(AppSettings settings, NotificationData notification, string deviceFilter)
        {
            var clock = Stopwatch.StartNew();
            var forms = CreateForms(settings, notification, clock, deviceFilter, false);
            if (forms.Count == 0) return;
            foreach (var form in forms) form.Show();

            foreach (var form in forms) form.RenderFrame();

            var timer = new WinForms.Timer { Interval = 16 };
            timer.Tick += delegate
            {
                foreach (var form in forms.Where(item => !item.IsDisposed && item.Visible)) form.RenderFrame();
                if (clock.Elapsed.TotalSeconds >= settings.DurationSeconds)
                {
                    foreach (var edge in forms.OfType<EdgeStripForm>().Where(item => !item.IsDisposed && item.Visible)) edge.Close();
                }
                var islandLifetime = Math.Max(settings.IslandDurationSeconds, settings.DurationSeconds + 2.0);
                if (clock.Elapsed.TotalSeconds >= islandLifetime)
                {
                    foreach (var island in forms.OfType<IslandForm>().Where(item => !item.IsDisposed && item.Visible && !item.SuppressAutoDismiss)) island.Close();
                }
                if (forms.Any(item => !item.IsDisposed && item.Visible)) return;
                timer.Stop();
                WinForms.Application.ExitThread();
            };
            timer.Start();
            WinForms.Application.Run();
        }

        internal static List<OverlayForm> CreateForms(AppSettings settings, NotificationData notification, Stopwatch clock, string deviceFilter, bool previewLoop)
        {
            settings.EnsureCurrentDisplays();
            var forms = new List<OverlayForm>();
            var screens = WinForms.Screen.AllScreens
                .Where(screen => string.IsNullOrEmpty(deviceFilter) || string.Equals(screen.DeviceName, deviceFilter, StringComparison.OrdinalIgnoreCase))
                .Where(screen => settings.ForScreen(screen).Enabled);

            foreach (var screen in screens)
            {
                var profile = settings.ForScreen(screen);
                var strips = EdgeGeometry.Strips(screen.Bounds.Size, settings, profile);
                foreach (var strip in strips)
                {
                    forms.Add(new EdgeStripForm(screen, strip, settings, profile, clock, previewLoop));
                }

                if (!string.Equals(settings.IslandMode, "Off", StringComparison.OrdinalIgnoreCase)
                    && (screen.Primary || settings.IslandOnAllDisplays))
                {
                    forms.Add(new IslandForm(screen, settings, notification, clock, previewLoop));
                }
            }
            return forms;
        }
    }

    internal sealed class LivePreviewController : IDisposable
    {
        private readonly object _gate = new object();
        private readonly Thread _thread;
        private AppSettings _pendingSettings;
        private string _pendingDevice;
        private bool _pending;
        private bool _enabled;
        private bool _stopping;

        public LivePreviewController()
        {
            _thread = new Thread(Run) { IsBackground = true, Name = "Codex Edge Glow live preview" };
            _thread.SetApartmentState(ApartmentState.STA);
            _thread.Start();
        }

        public void Show(AppSettings settings, string deviceFilter)
        {
            lock (_gate)
            {
                _pendingSettings = CloneSettings(settings);
                _pendingDevice = deviceFilter;
                _enabled = true;
                _pending = true;
            }
        }

        public void Hide()
        {
            lock (_gate)
            {
                _enabled = false;
                _pending = true;
            }
        }

        private void Run()
        {
            var forms = new List<OverlayForm>();
            var clock = new Stopwatch();
            AppSettings activeSettings = null;
            var timer = new WinForms.Timer { Interval = 16 };
            timer.Tick += delegate
            {
                AppSettings nextSettings = null;
                string nextDevice = null;
                bool rebuild = false;
                bool enabled;
                bool stopping;
                lock (_gate)
                {
                    stopping = _stopping;
                    enabled = _enabled;
                    if (_pending)
                    {
                        rebuild = true;
                        nextSettings = _pendingSettings;
                        nextDevice = _pendingDevice;
                        _pending = false;
                    }
                }

                if (stopping)
                {
                    CloseForms(forms);
                    timer.Stop();
                    WinForms.Application.ExitThread();
                    return;
                }

                if (rebuild)
                {
                    CloseForms(forms);
                    activeSettings = nextSettings;
                    if (enabled && activeSettings != null)
                    {
                        clock.Restart();
                        forms = OverlaySession.CreateForms(activeSettings, NotificationData.Sample(), clock, nextDevice, true);
                        foreach (var form in forms) form.Show();
                    }
                }

                if (!enabled || activeSettings == null) return;
                if (clock.Elapsed.TotalSeconds >= Math.Max(0.55, activeSettings.DurationSeconds)) clock.Restart();
                foreach (var form in forms.Where(item => !item.IsDisposed && item.Visible)) form.RenderFrame();
            };
            timer.Start();
            WinForms.Application.Run();
            timer.Dispose();
            CloseForms(forms);
        }

        private static void CloseForms(List<OverlayForm> forms)
        {
            foreach (var form in forms.Where(item => !item.IsDisposed)) form.Close();
            forms.Clear();
        }

        private static AppSettings CloneSettings(AppSettings source)
        {
            return new AppSettings
            {
                Version = source.Version,
                ColorPrimary = source.ColorPrimary,
                ColorSecondary = source.ColorSecondary,
                ColorHighlight = source.ColorHighlight,
                Thickness = source.Thickness,
                Intensity = source.Intensity,
                DurationSeconds = source.DurationSeconds,
                TrailLength = source.TrailLength,
                ShowFaintFrame = source.ShowFaintFrame,
                EffectPattern = source.EffectPattern,
                IslandMode = source.IslandMode,
                IslandWidth = source.IslandWidth,
                IslandOffset = source.IslandOffset,
                IslandDurationSeconds = source.IslandDurationSeconds,
                MessageMaxLength = source.MessageMaxLength,
                IslandOnAllDisplays = source.IslandOnAllDisplays,
                ShowFolder = source.ShowFolder,
                EnableQuickReply = source.EnableQuickReply,
                Displays = source.Displays.Select(item => new DisplayProfile
                {
                    DeviceName = item.DeviceName,
                    Enabled = item.Enabled,
                    InsetLeft = item.InsetLeft,
                    InsetTop = item.InsetTop,
                    InsetRight = item.InsetRight,
                    InsetBottom = item.InsetBottom,
                    CornerRadius = item.CornerRadius
                }).ToList()
            };
        }

        public void Dispose()
        {
            lock (_gate) _stopping = true;
            if (_thread.IsAlive) _thread.Join(2000);
        }
    }

    internal abstract class OverlayForm : WinForms.Form
    {
        protected readonly WinForms.Screen ScreenInfo;
        protected readonly AppSettings Settings;
        protected readonly Stopwatch Clock;
        protected readonly bool PreviewLoop;
        protected readonly Color Primary;
        protected readonly Color Secondary;
        protected readonly Color Highlight;

        protected virtual bool Interactive { get { return false; } }
        protected override bool ShowWithoutActivation { get { return true; } }
        protected override WinForms.CreateParams CreateParams
        {
            get
            {
                var value = base.CreateParams;
                value.ExStyle |= 0x80;
                if (!Interactive) value.ExStyle |= 0x20 | 0x08000000;
                return value;
            }
        }

        protected OverlayForm(WinForms.Screen screen, AppSettings settings, Stopwatch clock, bool previewLoop)
        {
            ScreenInfo = screen;
            Settings = settings;
            Clock = clock;
            PreviewLoop = previewLoop;
            Primary = ColorTools.Parse(settings.ColorPrimary, Color.FromArgb(255, 99, 99));
            Secondary = ColorTools.Parse(settings.ColorSecondary, Color.FromArgb(255, 181, 92));
            Highlight = ColorTools.Parse(settings.ColorHighlight, Color.FromArgb(238, 83, 181));
            AutoScaleMode = WinForms.AutoScaleMode.None;
            StartPosition = WinForms.FormStartPosition.Manual;
            FormBorderStyle = WinForms.FormBorderStyle.None;
            ShowInTaskbar = false;
            TopMost = true;
            BackColor = Color.FromArgb(30, 30, 36);
            TransparencyKey = Color.Black;
            SetStyle(WinForms.ControlStyles.UserPaint | WinForms.ControlStyles.AllPaintingInWmPaint | WinForms.ControlStyles.OptimizedDoubleBuffer, true);
            UpdateStyles();
        }

        protected double Progress { get { return Clamp(Clock.Elapsed.TotalSeconds / Settings.DurationSeconds, 0, 1); } }
        protected double OpacityFactor
        {
            get
            {
                var progress = Progress;
                return EaseOutCubic(Clamp(progress / 0.07, 0, 1))
                    * SmoothStep(Clamp((1 - progress) / 0.22, 0, 1))
                    * Clamp(Settings.Intensity, 0.2, 1.5);
            }
        }

        protected override void OnPaintBackground(WinForms.PaintEventArgs e) { e.Graphics.Clear(Color.Black); }
        internal virtual void RenderFrame() { Invalidate(); }
        protected static double Clamp(double value, double min, double max) { return Math.Max(min, Math.Min(max, value)); }
        protected static double EaseOutCubic(double x) { return 1 - Math.Pow(1 - x, 3); }
        protected static double SmoothStep(double x) { return x * x * (3 - (2 * x)); }
    }

    internal sealed class EdgeStripForm : OverlayForm
    {
        private readonly RectangleF _strip;
        private readonly DisplayProfile _profile;

        public EdgeStripForm(WinForms.Screen screen, RectangleF strip, AppSettings settings, DisplayProfile profile, Stopwatch clock, bool previewLoop)
            : base(screen, settings, clock, previewLoop)
        {
            _strip = strip;
            _profile = profile;
            Bounds = new Rectangle(
                screen.Bounds.Left + (int)Math.Floor(strip.Left),
                screen.Bounds.Top + (int)Math.Floor(strip.Top),
                Math.Max(1, (int)Math.Ceiling(strip.Width)),
                Math.Max(1, (int)Math.Ceiling(strip.Height)));
            TransparencyKey = Color.Empty;
        }

        protected override WinForms.CreateParams CreateParams
        {
            get
            {
                var value = base.CreateParams;
                value.ExStyle |= 0x00080000;
                return value;
            }
        }

        internal override void RenderFrame()
        {
            if (!IsHandleCreated || IsDisposed || ClientSize.Width <= 0 || ClientSize.Height <= 0) return;
            var opacity = OpacityFactor;
            if (opacity <= 0.002) return;
            using (var surface = new Bitmap(ClientSize.Width, ClientSize.Height, System.Drawing.Imaging.PixelFormat.Format32bppPArgb))
            {
                using (var graphics = Graphics.FromImage(surface))
                {
                    graphics.CompositingMode = CompositingMode.SourceCopy;
                    graphics.Clear(Color.Transparent);
                    graphics.CompositingMode = CompositingMode.SourceOver;
                    graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    graphics.CompositingQuality = CompositingQuality.HighQuality;
                    graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    graphics.TranslateTransform(-_strip.Left, -_strip.Top);
                    EdgePainter.Draw(graphics, ScreenInfo.Bounds.Size, Settings, _profile, Primary, Secondary, Highlight, Progress, opacity);
                }
                LayeredWindowSurface.Update(Handle, surface, Bounds.Location);
            }
        }
    }

    internal static class LayeredWindowSurface
    {
        private const byte AcSrcOver = 0;
        private const byte AcSrcAlpha = 1;
        private const int UlwAlpha = 2;

        [StructLayout(LayoutKind.Sequential)]
        private struct NativePoint { public int X; public int Y; }

        [StructLayout(LayoutKind.Sequential)]
        private struct NativeSize { public int Width; public int Height; }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct BlendFunction
        {
            public byte BlendOp;
            public byte BlendFlags;
            public byte SourceConstantAlpha;
            public byte AlphaFormat;
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool UpdateLayeredWindow(IntPtr window, IntPtr destinationDc, ref NativePoint destination,
            ref NativeSize size, IntPtr sourceDc, ref NativePoint source, int colorKey, ref BlendFunction blend, int flags);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr GetDC(IntPtr window);

        [DllImport("user32.dll")]
        private static extern int ReleaseDC(IntPtr window, IntPtr dc);

        [DllImport("gdi32.dll", SetLastError = true)]
        private static extern IntPtr CreateCompatibleDC(IntPtr dc);

        [DllImport("gdi32.dll")]
        private static extern bool DeleteDC(IntPtr dc);

        [DllImport("gdi32.dll")]
        private static extern IntPtr SelectObject(IntPtr dc, IntPtr item);

        [DllImport("gdi32.dll")]
        private static extern bool DeleteObject(IntPtr item);

        public static void Update(IntPtr window, Bitmap bitmap, Point location)
        {
            var screenDc = GetDC(IntPtr.Zero);
            var memoryDc = CreateCompatibleDC(screenDc);
            var bitmapHandle = IntPtr.Zero;
            var previous = IntPtr.Zero;
            try
            {
                bitmapHandle = bitmap.GetHbitmap(Color.FromArgb(0));
                previous = SelectObject(memoryDc, bitmapHandle);
                var destination = new NativePoint { X = location.X, Y = location.Y };
                var source = new NativePoint();
                var size = new NativeSize { Width = bitmap.Width, Height = bitmap.Height };
                var blend = new BlendFunction
                {
                    BlendOp = AcSrcOver,
                    BlendFlags = 0,
                    SourceConstantAlpha = 255,
                    AlphaFormat = AcSrcAlpha
                };
                if (!UpdateLayeredWindow(window, screenDc, ref destination, ref size, memoryDc, ref source, 0, ref blend, UlwAlpha))
                {
                    throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
                }
            }
            finally
            {
                if (previous != IntPtr.Zero) SelectObject(memoryDc, previous);
                if (bitmapHandle != IntPtr.Zero) DeleteObject(bitmapHandle);
                if (memoryDc != IntPtr.Zero) DeleteDC(memoryDc);
                if (screenDc != IntPtr.Zero) ReleaseDC(IntPtr.Zero, screenDc);
            }
        }
    }

    internal sealed class IslandForm : OverlayForm
    {
        private readonly NotificationData _notification;
        private readonly float _contentWidth;
        private readonly float _collapsedHeight;
        private readonly Bitmap _logo;
        private readonly WinForms.TextBox _reply;
        private string _statusText;
        private bool _expanded;
        private bool _hovered;
        private bool _dismissHovered;
        private bool _sendHovered;
        private bool _sendPressed;
        private bool _sending;
        private const float OuterPadding = 10;
        public bool Pinned { get; private set; }
        public bool SuppressAutoDismiss { get { return Pinned || _hovered; } }

        protected override bool Interactive { get { return true; } }
        private bool CanReply
        {
            get { return Settings.EnableQuickReply && !string.IsNullOrWhiteSpace(_notification.ThreadId); }
        }

        private double IslandLifetime
        {
            get { return Math.Max(Settings.IslandDurationSeconds, Settings.DurationSeconds + 2.0); }
        }

        private double IslandProgress
        {
            get { return Clamp(Clock.Elapsed.TotalSeconds / IslandLifetime, 0, 1); }
        }

        private double IslandOpacity
        {
            get
            {
                if (PreviewLoop) return 1;
                if (Pinned) return 1;
                var elapsed = Clock.Elapsed.TotalSeconds;
                var remaining = IslandLifetime - elapsed;
                return EaseOutCubic(Clamp(elapsed / 0.22, 0, 1))
                    * SmoothStep(Clamp(remaining / 0.9, 0, 1));
            }
        }

        public IslandForm(WinForms.Screen screen, AppSettings settings, NotificationData notification, Stopwatch clock, bool previewLoop)
            : base(screen, settings, clock, previewLoop)
        {
            _notification = notification;
            Text = "Codex completion";
            TransparencyKey = Color.Black;
            _contentWidth = (float)Clamp(settings.IslandWidth, 280, screen.Bounds.Width - 60);
            _collapsedHeight = string.Equals(settings.IslandMode, "Detailed", StringComparison.OrdinalIgnoreCase) ? 88f : 54f;
            var width = (int)Math.Ceiling(_contentWidth + (OuterPadding * 2));
            var height = (int)Math.Ceiling(settings.IslandOffset + _collapsedHeight + (OuterPadding * 2));
            Bounds = new Rectangle(screen.Bounds.Left + ((screen.Bounds.Width - width) / 2), screen.Bounds.Top, width, height);
            _logo = LoadLogo();

            _reply = new WinForms.TextBox
            {
                Visible = false,
                BorderStyle = WinForms.BorderStyle.None,
                BackColor = Color.FromArgb(20, 20, 25),
                ForeColor = Color.FromArgb(245, 245, 248),
                Font = new Font("Segoe UI", 10f),
                MaxLength = 4000
            };
            _reply.KeyDown += delegate(object sender, WinForms.KeyEventArgs args)
            {
                if (args.KeyCode == WinForms.Keys.Enter && !args.Shift)
                {
                    args.SuppressKeyPress = true;
                    SendReply();
                }
                else if (args.KeyCode == WinForms.Keys.Escape)
                {
                    Close();
                }
            };
            Controls.Add(_reply);

            if (CanReply) Cursor = WinForms.Cursors.Hand;

            MouseEnter += delegate { _hovered = true; };
            MouseLeave += delegate
            {
                _hovered = false;
                _dismissHovered = false;
                _sendHovered = false;
                _sendPressed = false;
                Invalidate();
            };
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && _logo != null) _logo.Dispose();
            base.Dispose(disposing);
        }

        protected override void OnPaint(WinForms.PaintEventArgs e)
        {
            base.OnPaint(e);
            var opacity = IslandOpacity;
            if (opacity <= 0.002) return;
            var detailed = string.Equals(Settings.IslandMode, "Detailed", StringComparison.OrdinalIgnoreCase);
            var entrance = Pinned ? 1 : EaseOutCubic(Clamp(Clock.Elapsed.TotalSeconds / 0.28, 0, 1));
            var x = OuterPadding;
            var contentHeight = _expanded ? 154f : _collapsedHeight;
            var y = (float)(Settings.IslandOffset + OuterPadding - ((1 - entrance) * (_collapsedHeight + 18)));
            var rect = new RectangleF(x, y, _contentWidth, contentHeight);
            var graphics = e.Graphics;
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.CompositingQuality = CompositingQuality.HighQuality;
            graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

            using (var path = DrawingTools.RoundedPath(rect, _expanded ? 32 : rect.Height / 2))
            using (var fill = new SolidBrush(ColorTools.Scale(Color.FromArgb(30, 30, 36), opacity)))
            using (var border = new Pen(ColorTools.Scale(Color.FromArgb(76, 76, 88), opacity), 1))
            {
                graphics.FillPath(fill, path);
                graphics.DrawPath(border, path);
            }

            var iconRect = new RectangleF(rect.X + 15, rect.Y + (_expanded ? 18 : ((rect.Height - 28) / 2)), 28, 28);
            if (_logo != null)
            {
                var attributes = new System.Drawing.Imaging.ImageAttributes();
                var matrix = new System.Drawing.Imaging.ColorMatrix { Matrix33 = (float)Math.Min(1, opacity) };
                attributes.SetColorMatrix(matrix);
                graphics.DrawImage(_logo, Rectangle.Round(iconRect), 0, 0, _logo.Width, _logo.Height, GraphicsUnit.Pixel, attributes);
                attributes.Dispose();
            }

            var title = _notification.Title;
            if (Settings.ShowFolder && !string.IsNullOrWhiteSpace(_notification.Folder)) title += "  ·  " + _notification.Folder;
            var textX = rect.X + 54;
            var closeRect = DismissBounds(rect);
            DrawDismissButton(graphics, closeRect, opacity);
            var textWidth = Math.Max(80, closeRect.Left - textX - 10);
            DrawingTools.Text(graphics, title, new RectangleF(textX, rect.Y + (detailed || _expanded ? 9 : 11), textWidth, 27), 14, FontStyle.Bold, ColorTools.Scale(Color.FromArgb(244, 244, 247), opacity));
            if (detailed)
            {
                DrawingTools.TextLines(graphics, Ellipsize(_notification.Message, Settings.MessageMaxLength), new RectangleF(textX, rect.Y + 34, textWidth, 31), 12, FontStyle.Regular, ColorTools.Scale(Color.FromArgb(176, 176, 187), opacity));
            }

            if (!_expanded)
            {
                var hint = CanReply ? "Click to reply" : (Settings.EnableQuickReply ? "Reply available on live completions" : string.Empty);
                if (!string.IsNullOrEmpty(hint))
                {
                    DrawingTools.Text(graphics, hint, new RectangleF(textX, rect.Bottom - 25, textWidth, 18), 10, FontStyle.Regular, ColorTools.Scale(CanReply ? Primary : Color.FromArgb(118, 118, 130), opacity));
                }
            }
            else
            {
                var input = ReplyInputBounds(rect);
                using (var inputPath = DrawingTools.RoundedPath(input, 11))
                using (var inputFill = new SolidBrush(ColorTools.Scale(Color.FromArgb(20, 20, 25), opacity)))
                using (var inputBorder = new Pen(ColorTools.Scale(Color.FromArgb(78, 78, 91), opacity), 1))
                {
                    graphics.FillPath(inputFill, inputPath);
                    graphics.DrawPath(inputBorder, inputPath);
                }
                DrawSendButton(graphics, SendBounds(rect), opacity);
                DrawingTools.Text(graphics, string.IsNullOrEmpty(_statusText) ? "Reply to this Codex task" : _statusText,
                    new RectangleF(rect.X + 18, rect.Y + 66, rect.Width - 36, 18), 10, FontStyle.Regular,
                    ColorTools.Scale(string.IsNullOrEmpty(_statusText) ? Color.FromArgb(146, 146, 158) : Primary, opacity));
            }
        }

        protected override void OnMouseClick(WinForms.MouseEventArgs e)
        {
            base.OnMouseClick(e);
            if (DismissBounds(CurrentContentBounds()).Contains(e.Location))
            {
                Close();
                return;
            }
            if (_expanded && SendBounds(CurrentContentBounds()).Contains(e.Location))
            {
                if (!_sending) SendReply();
                return;
            }
            if (!_expanded && CanReply) ExpandReply();
        }

        protected override void OnMouseMove(WinForms.MouseEventArgs e)
        {
            base.OnMouseMove(e);
            _hovered = true;
            var current = CurrentContentBounds();
            var overDismiss = DismissBounds(current).Contains(e.Location);
            var overSend = _expanded && SendBounds(current).Contains(e.Location);
            if (_dismissHovered != overDismiss)
            {
                _dismissHovered = overDismiss;
                Invalidate();
            }
            if (_sendHovered != overSend)
            {
                _sendHovered = overSend;
                Invalidate();
            }
            Cursor = overDismiss || (overSend && !_sending) || (!_expanded && CanReply) ? WinForms.Cursors.Hand : WinForms.Cursors.Default;
        }

        protected override void OnMouseDown(WinForms.MouseEventArgs e)
        {
            base.OnMouseDown(e);
            _sendPressed = _expanded && !_sending && SendBounds(CurrentContentBounds()).Contains(e.Location);
            if (_sendPressed) Invalidate();
        }

        protected override void OnMouseUp(WinForms.MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (_sendPressed)
            {
                _sendPressed = false;
                Invalidate();
            }
        }

        protected override bool ProcessCmdKey(ref WinForms.Message msg, WinForms.Keys keyData)
        {
            if (keyData == WinForms.Keys.Escape)
            {
                Close();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void ExpandReply()
        {
            _expanded = true;
            Pinned = true;
            Cursor = WinForms.Cursors.Default;
            Height = (int)Math.Ceiling(Settings.IslandOffset + 154 + (OuterPadding * 2));
            var rect = CurrentContentBounds();
            var input = ReplyInputBounds(rect);
            _reply.SetBounds((int)input.X + 12, (int)input.Y + 10, Math.Max(80, (int)input.Width - 24), 20);
            _reply.Visible = true;
            _reply.Focus();
            Invalidate();
        }

        private RectangleF CurrentContentBounds()
        {
            var entrance = Pinned ? 1 : EaseOutCubic(Clamp(Clock.Elapsed.TotalSeconds / 0.28, 0, 1));
            var height = _expanded ? 154f : _collapsedHeight;
            var y = (float)(Settings.IslandOffset + OuterPadding - ((1 - entrance) * (_collapsedHeight + 18)));
            return new RectangleF(OuterPadding, y, _contentWidth, height);
        }

        private static RectangleF DismissBounds(RectangleF island)
        {
            return new RectangleF(island.Right - 41, island.Top + 10, 28, 28);
        }

        private static RectangleF ReplyInputBounds(RectangleF island)
        {
            return new RectangleF(island.X + 18, island.Y + 94, island.Width - 126, 40);
        }

        private static RectangleF SendBounds(RectangleF island)
        {
            return new RectangleF(island.Right - 90, island.Y + 94, 72, 40);
        }

        private void DrawSendButton(Graphics graphics, RectangleF rect, double opacity)
        {
            var baseColor = _sending ? Color.FromArgb(79, 79, 90) : (_sendPressed ? ColorTools.Lerp(Primary, Color.Black, 0.12) : (_sendHovered ? ColorTools.Lerp(Primary, Color.White, 0.08) : Primary));
            using (var path = DrawingTools.RoundedPath(rect, 10))
            using (var fill = new SolidBrush(ColorTools.Scale(baseColor, opacity)))
            using (var border = new Pen(ColorTools.Scale(ColorTools.Lerp(baseColor, Color.White, 0.18), opacity), 1))
            {
                graphics.FillPath(fill, path);
                graphics.DrawPath(border, path);
            }
            DrawingTools.Text(graphics, _sending ? "Sending…" : "Send", rect, 12, FontStyle.Bold, ColorTools.Scale(Color.White, opacity));
        }

        private void DrawDismissButton(Graphics graphics, RectangleF rect, double opacity)
        {
            using (var path = DrawingTools.RoundedPath(rect, rect.Height / 2))
            using (var fill = new SolidBrush(ColorTools.Scale(_dismissHovered ? Color.FromArgb(72, 72, 84) : Color.FromArgb(43, 43, 51), opacity)))
            {
                graphics.FillPath(fill, path);
            }
            DrawingTools.Icon(graphics, "\uE711", rect, 10, ColorTools.Scale(Color.FromArgb(205, 205, 214), opacity));
        }

        private void SendReply()
        {
            var prompt = _reply.Text.Trim();
            if (prompt.Length == 0)
            {
                _statusText = "Type a message first";
                Invalidate();
                return;
            }

            _reply.Enabled = false;
            _sending = true;
            _statusText = "Sending to the originating task…";
            Invalidate();

            ThreadPool.QueueUserWorkItem(delegate
            {
                try
                {
                    CodexReplySender.Send(_notification, prompt);
                    BeginInvoke((Action)delegate
                    {
                        _statusText = "Sent to this Codex task";
                        Invalidate();
                        var closeTimer = new WinForms.Timer { Interval = 900 };
                        closeTimer.Tick += delegate
                        {
                            closeTimer.Stop();
                            closeTimer.Dispose();
                            Close();
                        };
                        closeTimer.Start();
                    });
                }
                catch (Exception exception)
                {
                    BeginInvoke((Action)delegate
                    {
                        _statusText = "Could not send: " + ShortError(exception.Message);
                        _reply.Enabled = true;
                        _sending = false;
                        _reply.Focus();
                        Invalidate();
                    });
                }
            });
        }

        private static string ShortError(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return "unknown error";
            value = value.Replace("\r", " ").Replace("\n", " ").Trim();
            return value.Length <= 58 ? value : value.Substring(0, 57) + "…";
        }

        private static Bitmap LoadLogo()
        {
            try
            {
                using (var icon = Icon.ExtractAssociatedIcon(WinForms.Application.ExecutablePath)) return icon.ToBitmap();
            }
            catch { return null; }
        }

        private static string Ellipsize(string text, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(text)) return "Task completed successfully.";
            maxLength = Math.Max(30, maxLength);
            if (text.Length <= maxLength) return text;
            var shortened = text.Substring(0, maxLength - 1).TrimEnd();
            var lastSpace = shortened.LastIndexOf(' ');
            if (lastSpace > shortened.Length * 0.68) shortened = shortened.Substring(0, lastSpace);
            return shortened + "…";
        }
    }

    internal static class CodexReplySender
    {
        public static void Send(NotificationData notification, string prompt)
        {
            if (string.IsNullOrWhiteSpace(notification.ThreadId)) throw new InvalidOperationException("missing task routing id");
            var executable = FindCodexExecutable();
            var startInfo = new ProcessStartInfo
            {
                FileName = executable,
                Arguments = "exec resume --skip-git-repo-check " + Program.QuoteArgument(notification.ThreadId) + " -",
                UseShellExecute = false,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                RedirectStandardInput = true
            };
            if (!string.IsNullOrWhiteSpace(notification.WorkingDirectory) && Directory.Exists(notification.WorkingDirectory))
            {
                startInfo.WorkingDirectory = notification.WorkingDirectory;
            }

            var process = Process.Start(startInfo);
            if (process == null) throw new InvalidOperationException("Codex did not start");
            process.StandardInput.Write(prompt);
            process.StandardInput.Close();
        }

        private static string FindCodexExecutable()
        {
            var configured = Environment.GetEnvironmentVariable("CODEX_CLI_PATH");
            if (!string.IsNullOrWhiteSpace(configured) && File.Exists(configured)) return configured;
            var root = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "OpenAI", "Codex", "bin");
            if (Directory.Exists(root))
            {
                var found = Directory.GetFiles(root, "codex.exe", SearchOption.AllDirectories)
                    .OrderByDescending(File.GetLastWriteTimeUtc)
                    .FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(found)) return found;
            }
            return "codex.exe";
        }
    }

    internal static class EdgeGeometry
    {
        public static RectangleF Path(Size screen, AppSettings settings, DisplayProfile profile)
        {
            var thickness = Clamp(settings.Thickness, 3, 24);
            var safety = (float)((thickness * 2.1) + 5);
            return new RectangleF(
                safety + (float)Math.Max(0, profile.InsetLeft),
                safety + (float)Math.Max(0, profile.InsetTop),
                Math.Max(40, screen.Width - (2 * safety) - (float)Math.Max(0, profile.InsetLeft) - (float)Math.Max(0, profile.InsetRight)),
                Math.Max(40, screen.Height - (2 * safety) - (float)Math.Max(0, profile.InsetTop) - (float)Math.Max(0, profile.InsetBottom)));
        }

        public static List<RectangleF> Strips(Size screen, AppSettings settings, DisplayProfile profile)
        {
            var path = Path(screen, settings, profile);
            var radius = (float)Clamp(profile.CornerRadius, 0.1, Math.Min(path.Width, path.Height) / 2);
            var band = (float)Math.Ceiling((Clamp(settings.Thickness, 3, 24) * 4.2) + 10);
            var half = band / 2;
            var bounds = new RectangleF(0, 0, screen.Width, screen.Height);
            var candidates = new[]
            {
                new RectangleF(path.Left - half, path.Top - half, path.Width + band, radius + band),
                new RectangleF(path.Left - half, path.Bottom - radius - half, path.Width + band, radius + band),
                new RectangleF(path.Left - half, path.Top + radius, band, Math.Max(1, path.Height - (2 * radius))),
                new RectangleF(path.Right - half, path.Top + radius, band, Math.Max(1, path.Height - (2 * radius)))
            };
            return candidates.Select(candidate => RectangleF.Intersect(candidate, bounds)).Where(item => item.Width >= 1 && item.Height >= 1).ToList();
        }

        private static double Clamp(double value, double min, double max) { return Math.Max(min, Math.Min(max, value)); }
    }

    internal static class EdgePainter
    {
        public static void Draw(Graphics graphics, Size screen, AppSettings settings, DisplayProfile profile, Color primary, Color secondary, Color highlight, double progress, double opacity)
        {
            var rect = EdgeGeometry.Path(screen, settings, profile);
            var radius = (float)Clamp(profile.CornerRadius, 0.1, Math.Min(rect.Width, rect.Height) / 2);
            var track = new RoundedTrack(rect, radius);
            var thickness = Clamp(settings.Thickness, 3, 24);
            if (settings.ShowFaintFrame)
            {
                using (var path = DrawingTools.RoundedPath(rect, radius))
                using (var pen = new Pen(ColorTools.WithAlpha(primary, 0.17 * opacity), (float)Math.Max(1.5, thickness * 0.28))) graphics.DrawPath(pen, path);
            }

            if (string.Equals(settings.EffectPattern, "HaloPulse", StringComparison.OrdinalIgnoreCase))
            {
                DrawHaloPulse(graphics, rect, radius, thickness, progress, opacity, primary, secondary, highlight);
                return;
            }

            var head = (0.02 + (1.72 * EaseInOutCubic(Math.Min(1, progress / 0.88)))) * track.Perimeter;
            var tailLength = Clamp(settings.TrailLength, 0.10, 0.70) * track.Perimeter;
            if (string.Equals(settings.EffectPattern, "DualOrbit", StringComparison.OrdinalIgnoreCase))
            {
                tailLength *= 0.64;
                DrawLayeredComet(graphics, track, head, tailLength, thickness, opacity, primary, secondary, highlight);
                DrawLayeredComet(graphics, track, head + (track.Perimeter / 2), tailLength, thickness, opacity, highlight, primary, secondary);
                return;
            }
            if (string.Equals(settings.EffectPattern, "CornerSpark", StringComparison.OrdinalIgnoreCase))
            {
                DrawCornerSparks(graphics, track, thickness, progress, opacity, primary, secondary, highlight);
                return;
            }

            DrawLayeredComet(graphics, track, head, tailLength, thickness, opacity, primary, secondary, highlight);
        }

        private static void DrawLayeredComet(Graphics graphics, RoundedTrack track, double head, double tailLength, double thickness, double opacity, Color primary, Color secondary, Color highlight)
        {
            DrawComet(graphics, track, head, tailLength, thickness * 3.7, 0.10 * opacity, primary, secondary, highlight);
            DrawComet(graphics, track, head, tailLength, thickness * 2.1, 0.22 * opacity, primary, secondary, highlight);
            DrawComet(graphics, track, head, tailLength, thickness, 0.62 * opacity, primary, secondary, highlight);
            DrawComet(graphics, track, head, tailLength, Math.Max(2.5, thickness * 0.42), opacity, primary, secondary, highlight);
        }

        private static void DrawHaloPulse(Graphics graphics, RectangleF rect, float radius, double thickness, double progress, double opacity, Color primary, Color secondary, Color highlight)
        {
            var wave = 0.48 + (0.52 * Math.Sin(Math.PI * Clamp(progress / 0.82, 0, 1)));
            var colorPhase = Clamp(progress * 2.2, 0, 2);
            var color = colorPhase < 1 ? ColorTools.Lerp(primary, highlight, colorPhase) : ColorTools.Lerp(highlight, secondary, colorPhase - 1);
            using (var path = DrawingTools.RoundedPath(rect, radius))
            using (var outer = new Pen(ColorTools.WithAlpha(color, opacity * wave * 0.16), (float)(thickness * 4.2)))
            using (var middle = new Pen(ColorTools.WithAlpha(color, opacity * wave * 0.36), (float)(thickness * 2.2)))
            using (var core = new Pen(ColorTools.WithAlpha(color, opacity * wave), (float)Math.Max(2.5, thickness * 0.72)))
            {
                outer.LineJoin = middle.LineJoin = core.LineJoin = LineJoin.Round;
                graphics.DrawPath(outer, path);
                graphics.DrawPath(middle, path);
                graphics.DrawPath(core, path);
            }
        }

        private static void DrawCornerSparks(Graphics graphics, RoundedTrack track, double thickness, double progress, double opacity, Color primary, Color secondary, Color highlight)
        {
            var local = Clamp(progress / 0.78, 0, 1);
            var halfLength = track.Perimeter * (0.015 + (0.085 * EaseInOutCubic(local)));
            var sparkOpacity = opacity * Math.Sin(Math.PI * local);
            var colors = new[] { primary, secondary, highlight, ColorTools.Lerp(primary, secondary, 0.5) };
            for (var index = 0; index < track.CornerCenters.Length; index++)
            {
                var center = track.CornerCenters[index];
                DrawSpark(graphics, track, center, halfLength, thickness * 3.0, sparkOpacity * 0.14, colors[index]);
                DrawSpark(graphics, track, center, halfLength, thickness * 1.65, sparkOpacity * 0.34, colors[index]);
                DrawSpark(graphics, track, center, halfLength, Math.Max(2.5, thickness * 0.62), sparkOpacity, colors[index]);
            }
        }

        private static void DrawSpark(Graphics graphics, RoundedTrack track, double center, double halfLength, double thickness, double opacity, Color color)
        {
            var segments = Math.Max(48, SegmentCountFor(halfLength * 2) / 2);
            using (var pen = new Pen(Color.White, (float)thickness))
            {
                pen.StartCap = LineCap.Square;
                pen.EndCap = LineCap.Square;
                pen.LineJoin = LineJoin.Round;
                for (var index = 0; index < segments; index++)
                {
                    var t0 = -1.0 + ((2.0 * index) / segments);
                    var t1 = -1.0 + ((2.0 * (index + 1)) / segments);
                    pen.Color = ColorTools.WithAlpha(color, opacity * Math.Pow(1 - Math.Abs((t0 + t1) / 2), 1.8));
                    graphics.DrawLine(pen, track.PointAt(center + (halfLength * t0)), track.PointAt(center + (halfLength * t1)));
                }
            }
        }

        private static void DrawComet(Graphics graphics, RoundedTrack track, double head, double tailLength, double thickness, double layerOpacity, Color primary, Color secondary, Color highlight)
        {
            var segments = SegmentCountFor(tailLength);
            using (var pen = new Pen(Color.White, (float)thickness))
            {
                pen.StartCap = LineCap.Square;
                pen.EndCap = LineCap.Square;
                pen.LineJoin = LineJoin.Round;
                for (var i = 0; i < segments; i++)
                {
                    var t0 = (double)i / segments;
                    var t1 = (double)(i + 1) / segments;
                    var p0 = track.PointAt(head - tailLength + (tailLength * t0));
                    var p1 = track.PointAt(head - tailLength + (tailLength * t1));
                    var color = t1 < 0.55 ? ColorTools.Lerp(secondary, highlight, t1 / 0.55) : ColorTools.Lerp(highlight, primary, (t1 - 0.55) / 0.45);
                    pen.Color = ColorTools.WithAlpha(color, layerOpacity * Math.Pow(t1, 2.15));
                    graphics.DrawLine(pen, p0, p1);
                }
            }
            var headPoint = track.PointAt(head);
            using (var brush = new SolidBrush(ColorTools.WithAlpha(primary, layerOpacity)))
            {
                graphics.FillEllipse(brush, (float)(headPoint.X - (thickness / 2)), (float)(headPoint.Y - (thickness / 2)), (float)thickness, (float)thickness);
            }
        }

        internal static int SegmentCountFor(double tailLength)
        {
            return Math.Max(64, Math.Min(1400, (int)Math.Ceiling(Math.Max(1, tailLength) / 3.5)));
        }

        private static double Clamp(double value, double min, double max) { return Math.Max(min, Math.Min(max, value)); }
        private static double EaseInOutCubic(double x) { return x < 0.5 ? 4 * x * x * x : 1 - (Math.Pow(-2 * x + 2, 3) / 2); }
    }

    internal static class DrawingTools
    {
        public static GraphicsPath RoundedPath(RectangleF rect, float radius)
        {
            radius = Math.Max(0.1f, Math.Min(radius, Math.Min(rect.Width, rect.Height) / 2));
            var diameter = radius * 2;
            var path = new GraphicsPath();
            path.AddArc(rect.Left, rect.Top, diameter, diameter, 180, 90);
            path.AddArc(rect.Right - diameter, rect.Top, diameter, diameter, 270, 90);
            path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(rect.Left, rect.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();
            return path;
        }

        public static void Text(Graphics graphics, string text, RectangleF rect, float size, FontStyle style, Color color)
        {
            using (var font = new Font("Segoe UI", size, style, GraphicsUnit.Pixel))
            using (var brush = new SolidBrush(color))
            using (var format = new StringFormat { Trimming = StringTrimming.EllipsisCharacter, FormatFlags = StringFormatFlags.NoWrap, LineAlignment = StringAlignment.Center })
            {
                graphics.DrawString(text, font, brush, rect, format);
            }
        }

        public static void TextLines(Graphics graphics, string text, RectangleF rect, float size, FontStyle style, Color color)
        {
            using (var font = new Font("Segoe UI", size, style, GraphicsUnit.Pixel))
            using (var brush = new SolidBrush(color))
            using (var format = new StringFormat { Trimming = StringTrimming.EllipsisCharacter, FormatFlags = StringFormatFlags.NoWrap, LineAlignment = StringAlignment.Center })
            {
                var words = (text ?? string.Empty).Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                var first = string.Empty;
                var nextWord = 0;
                while (nextWord < words.Length)
                {
                    var candidate = first.Length == 0 ? words[nextWord] : first + " " + words[nextWord];
                    if (first.Length > 0 && graphics.MeasureString(candidate, font, int.MaxValue, StringFormat.GenericTypographic).Width > rect.Width) break;
                    first = candidate;
                    nextWord++;
                }
                if (first.Length == 0 && words.Length > 0)
                {
                    first = words[0];
                    nextWord = 1;
                }

                var second = nextWord < words.Length ? string.Join(" ", words.Skip(nextWord).ToArray()) : string.Empty;
                var lineHeight = rect.Height / 2;
                graphics.DrawString(first, font, brush, new RectangleF(rect.X, rect.Y, rect.Width, lineHeight), format);
                if (second.Length > 0)
                {
                    graphics.DrawString(second, font, brush, new RectangleF(rect.X, rect.Y + lineHeight, rect.Width, lineHeight), format);
                }
            }
        }

        public static void Icon(Graphics graphics, string glyph, RectangleF rect, float size, Color color)
        {
            using (var font = new Font("Segoe Fluent Icons", size, FontStyle.Regular, GraphicsUnit.Pixel))
            using (var brush = new SolidBrush(color))
            using (var format = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
            {
                graphics.DrawString(glyph, font, brush, rect, format);
            }
        }
    }

    internal static class ColorTools
    {
        public static Color Parse(string value, Color fallback)
        {
            try { return ColorTranslator.FromHtml(value); }
            catch { return fallback; }
        }

        public static Color Scale(Color color, double factor)
        {
            factor = Math.Max(0.02, Math.Min(1, factor));
            return Color.FromArgb(Math.Max(1, (int)(color.R * factor)), Math.Max(1, (int)(color.G * factor)), Math.Max(1, (int)(color.B * factor)));
        }

        public static Color WithAlpha(Color color, double factor)
        {
            factor = Math.Max(0, Math.Min(1, factor));
            return Color.FromArgb((int)Math.Round(255 * factor), color.R, color.G, color.B);
        }

        public static Color Lerp(Color from, Color to, double t)
        {
            return Color.FromArgb((int)(from.R + ((to.R - from.R) * t)), (int)(from.G + ((to.G - from.G) * t)), (int)(from.B + ((to.B - from.B) * t)));
        }
    }

    internal sealed class RoundedTrack
    {
        private readonly RectangleF _rect;
        private readonly double _radius;
        private readonly double _top;
        private readonly double _side;
        private readonly double _quarterArc;
        public double Perimeter { get; private set; }
        public double[] CornerCenters { get; private set; }

        public RoundedTrack(RectangleF rect, double radius)
        {
            _rect = rect;
            _radius = Math.Max(0.001, Math.Min(radius, Math.Min(rect.Width, rect.Height) / 2));
            _top = rect.Width - (2 * _radius);
            _side = rect.Height - (2 * _radius);
            _quarterArc = Math.PI * _radius / 2;
            Perimeter = (2 * _top) + (2 * _side) + (4 * _quarterArc);
            CornerCenters = new[]
            {
                (_top / 2) + (_quarterArc / 2),
                (_top / 2) + _quarterArc + _side + (_quarterArc / 2),
                (_top / 2) + _quarterArc + _side + _quarterArc + _top + (_quarterArc / 2),
                (_top / 2) + _quarterArc + _side + _quarterArc + _top + _quarterArc + _side + (_quarterArc / 2)
            };
        }

        public PointF PointAt(double distance)
        {
            var d = distance % Perimeter;
            if (d < 0) d += Perimeter;
            var topHalf = _top / 2;
            if (d <= topHalf) return new PointF((float)(_rect.Left + (_rect.Width / 2) + d), _rect.Top);
            d -= topHalf;
            if (d <= _quarterArc) return Arc(_rect.Right - _radius, _rect.Top + _radius, -Math.PI / 2, d);
            d -= _quarterArc;
            if (d <= _side) return new PointF(_rect.Right, (float)(_rect.Top + _radius + d));
            d -= _side;
            if (d <= _quarterArc) return Arc(_rect.Right - _radius, _rect.Bottom - _radius, 0, d);
            d -= _quarterArc;
            if (d <= _top) return new PointF((float)(_rect.Right - _radius - d), _rect.Bottom);
            d -= _top;
            if (d <= _quarterArc) return Arc(_rect.Left + _radius, _rect.Bottom - _radius, Math.PI / 2, d);
            d -= _quarterArc;
            if (d <= _side) return new PointF(_rect.Left, (float)(_rect.Bottom - _radius - d));
            d -= _side;
            if (d <= _quarterArc) return Arc(_rect.Left + _radius, _rect.Top + _radius, Math.PI, d);
            d -= _quarterArc;
            return new PointF((float)(_rect.Left + _radius + d), _rect.Top);
        }

        private PointF Arc(double cx, double cy, double startAngle, double arcDistance)
        {
            var angle = startAngle + (arcDistance / _radius);
            return new PointF((float)(cx + (_radius * Math.Cos(angle))), (float)(cy + (_radius * Math.Sin(angle))));
        }
    }
}
