using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WinForms = System.Windows.Forms;
using Drawing = System.Drawing;

namespace CodexEdgeGlow
{
    internal static class SettingsMode
    {
        public static void Run(AppSettings settings, bool previewTest)
        {
            RenderOptions.ProcessRenderMode = RenderMode.SoftwareOnly;
            var app = new Application { ShutdownMode = ShutdownMode.OnExplicitShutdown };
            var window = new SettingsWindow(settings);
            app.MainWindow = window;
            app.ShutdownMode = ShutdownMode.OnMainWindowClose;
            window.Show();
            if (previewTest)
            {
                var previewTimer = new System.Windows.Threading.DispatcherTimer { Interval = TimeSpan.FromMilliseconds(600) };
                previewTimer.Tick += delegate
                {
                    previewTimer.Stop();
                    window.RunPreviewForTest();
                };
                previewTimer.Start();
            }
            using (var tray = new TrayIconHost(app, window))
            {
                app.Run();
            }
        }
    }

    internal sealed class TrayIconHost : IDisposable
    {
        private readonly Application _application;
        private readonly SettingsWindow _window;
        private readonly WinForms.NotifyIcon _icon;
        private readonly WinForms.ContextMenuStrip _menu;
        private readonly Drawing.Icon _drawingIcon;

        public TrayIconHost(Application application, SettingsWindow window)
        {
            _application = application;
            _window = window;
            try { _drawingIcon = Drawing.Icon.ExtractAssociatedIcon(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName); }
            catch { _drawingIcon = Drawing.SystemIcons.Application; }

            _menu = new WinForms.ContextMenuStrip();
            var open = new WinForms.ToolStripMenuItem("Open settings");
            open.Click += delegate { Restore(); };
            var preview = new WinForms.ToolStripMenuItem("Toggle live preview");
            preview.Click += delegate { _window.Dispatcher.BeginInvoke((Action)delegate { _window.PreviewFromTray(); }); };
            var exit = new WinForms.ToolStripMenuItem("Exit");
            exit.Click += delegate { _window.Dispatcher.BeginInvoke((Action)delegate { _application.Shutdown(); }); };
            _menu.Items.Add(open);
            _menu.Items.Add(preview);
            _menu.Items.Add(new WinForms.ToolStripSeparator());
            _menu.Items.Add(exit);

            _icon = new WinForms.NotifyIcon
            {
                Icon = _drawingIcon,
                Text = "Codex Edge Glow",
                ContextMenuStrip = _menu,
                Visible = true
            };
            _icon.MouseClick += delegate(object sender, WinForms.MouseEventArgs args)
            {
                if (args.Button == WinForms.MouseButtons.Left) Restore();
            };
            _window.StateChanged += delegate
            {
                if (_window.WindowState == WindowState.Minimized) _window.Hide();
            };
        }

        private void Restore()
        {
            _window.Dispatcher.BeginInvoke((Action)delegate
            {
                _window.Show();
                _window.WindowState = WindowState.Normal;
                _window.Activate();
            });
        }

        public void Dispose()
        {
            _icon.Visible = false;
            _icon.Dispose();
            _menu.Dispose();
            if (!ReferenceEquals(_drawingIcon, Drawing.SystemIcons.Application)) _drawingIcon.Dispose();
        }
    }

    internal sealed class SettingsWindow : Window
    {
        private AppSettings _settings;
        private bool _loading;
        private TextBlock _status;
        private Button _primaryButton;
        private Button _secondaryButton;
        private Button _highlightButton;
        private Slider _thickness;
        private Slider _intensity;
        private Slider _duration;
        private Slider _trail;
        private TextBlock _thicknessValue;
        private TextBlock _intensityValue;
        private TextBlock _durationValue;
        private TextBlock _trailValue;
        private CheckBox _frame;
        private Button _patternComet;
        private Button _patternOrbit;
        private Button _patternPulse;
        private Button _patternCorners;
        private ListBox _displayList;
        private CheckBox _displayEnabled;
        private TextBox _insetLeft;
        private TextBox _insetTop;
        private TextBox _insetRight;
        private TextBox _insetBottom;
        private Slider _cornerRadius;
        private TextBlock _cornerValue;
        private Button _islandOff;
        private Button _islandCompact;
        private Button _islandDetailed;
        private Slider _islandWidth;
        private Slider _islandOffset;
        private Slider _islandDuration;
        private Slider _messageLength;
        private TextBlock _islandWidthValue;
        private TextBlock _islandOffsetValue;
        private TextBlock _islandDurationValue;
        private TextBlock _messageLengthValue;
        private CheckBox _islandAllDisplays;
        private CheckBox _showFolder;
        private CheckBox _quickReply;
        private ContentControl _pageHost;
        private Button _appearanceNav;
        private Button _displaysNav;
        private Button _islandNav;
        private Button _livePreviewButton;
        private UIElement _appearancePage;
        private UIElement _displaysPage;
        private UIElement _islandPage;
        private readonly List<WinForms.Screen> _screens;
        private readonly System.Windows.Threading.DispatcherTimer _previewDebounce;
        private LivePreviewController _livePreview;
        private bool _livePreviewEnabled;
        private string _previewDevice;

        public SettingsWindow(AppSettings settings)
        {
            _settings = settings;
            _settings.EnsureCurrentDisplays();
            _screens = WinForms.Screen.AllScreens.ToList();
            _previewDebounce = new System.Windows.Threading.DispatcherTimer { Interval = TimeSpan.FromMilliseconds(90) };
            _previewDebounce.Tick += delegate
            {
                _previewDebounce.Stop();
                PushLivePreview();
            };

            Title = "Codex Edge Glow";
            Width = 820;
            Height = 720;
            MinWidth = 720;
            MinHeight = 620;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            Background = Brush("#0E0E12");
            Foreground = Brushes.White;
            FontFamily = new FontFamily("Segoe UI");
            Icon = LoadAppIcon();

            ConfigureStyles();
            Content = BuildLayout();
            LoadAllControls();
            Closed += delegate
            {
                _previewDebounce.Stop();
                if (_livePreview != null) _livePreview.Dispose();
            };
        }

        private void ConfigureStyles()
        {
            Resources.Add("ButtonStyle", CreateButtonStyle(false));
            Resources.Add("AccentButtonStyle", CreateButtonStyle(true));
            Resources.Add("NavButtonStyle", CreateNavButtonStyle());

            var textBoxStyle = new Style(typeof(TextBox));
            textBoxStyle.Setters.Add(new Setter(TextBox.ForegroundProperty, Brushes.White));
            textBoxStyle.Setters.Add(new Setter(TextBox.CaretBrushProperty, Brush("#FF8A82")));
            textBoxStyle.Setters.Add(new Setter(TextBox.BackgroundProperty, Brush("#19191F")));
            textBoxStyle.Setters.Add(new Setter(TextBox.BorderBrushProperty, Brush("#383842")));
            textBoxStyle.Setters.Add(new Setter(TextBox.BorderThicknessProperty, new Thickness(1)));
            textBoxStyle.Setters.Add(new Setter(TextBox.PaddingProperty, new Thickness(10, 7, 10, 7)));
            textBoxStyle.Setters.Add(new Setter(TextBox.TemplateProperty, CreateTextBoxTemplate()));
            Resources.Add(typeof(TextBox), textBoxStyle);

            var checkStyle = new Style(typeof(CheckBox));
            checkStyle.Setters.Add(new Setter(CheckBox.ForegroundProperty, Brush("#E8E8EC")));
            checkStyle.Setters.Add(new Setter(CheckBox.MarginProperty, new Thickness(0, 7, 0, 7)));
            Resources.Add(typeof(CheckBox), checkStyle);
        }

        private UIElement BuildLayout()
        {
            var root = new DockPanel();
            root.Children.Add(BuildHeader());
            root.Children.Add(BuildFooter());

            _appearancePage = BuildAppearanceTab();
            _displaysPage = BuildDisplaysTab();
            _islandPage = BuildIslandTab();

            var body = new Grid { Margin = new Thickness(18, 12, 18, 10) };
            body.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            body.RowDefinitions.Add(new RowDefinition());

            var navShell = new Border
            {
                Background = Brush("#15151B"),
                BorderBrush = Brush("#2D2D36"),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(12),
                Padding = new Thickness(4),
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(0, 0, 0, 10)
            };
            var nav = new StackPanel { Orientation = Orientation.Horizontal };
            _appearanceNav = NavButton("Appearance");
            _displaysNav = NavButton("Displays");
            _islandNav = NavButton("Island");
            _appearanceNav.Click += delegate { ShowPage(_appearancePage, _appearanceNav); };
            _displaysNav.Click += delegate { ShowPage(_displaysPage, _displaysNav); };
            _islandNav.Click += delegate { ShowPage(_islandPage, _islandNav); };
            nav.Children.Add(_appearanceNav);
            nav.Children.Add(_displaysNav);
            nav.Children.Add(_islandNav);
            navShell.Child = nav;
            body.Children.Add(navShell);

            var pageShell = new Border
            {
                Background = Brush("#141419"),
                BorderBrush = Brush("#2B2B34"),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(14),
                ClipToBounds = true
            };
            Grid.SetRow(pageShell, 1);
            _pageHost = new ContentControl();
            pageShell.Child = _pageHost;
            body.Children.Add(pageShell);
            root.Children.Add(body);
            ShowPage(_appearancePage, _appearanceNav);
            return root;
        }

        private UIElement BuildHeader()
        {
            var border = new Border
            {
                BorderBrush = Brush("#282830"),
                BorderThickness = new Thickness(0, 0, 0, 1),
                Padding = new Thickness(24, 20, 24, 18)
            };
            DockPanel.SetDock(border, Dock.Top);
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(56) });
            grid.ColumnDefinitions.Add(new ColumnDefinition());

            var mark = new Image
            {
                Width = 42,
                Height = 42,
                Source = LoadAppIcon(),
                Stretch = Stretch.Uniform,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center
            };
            grid.Children.Add(mark);
            var copy = new StackPanel();
            Grid.SetColumn(copy, 1);
            copy.Children.Add(new TextBlock { Text = "Codex Edge Glow", FontSize = 22, FontWeight = FontWeights.SemiBold });
            copy.Children.Add(new TextBlock { Text = "Completion lighting tuned for every display", Foreground = Brush("#9B9BA6"), FontSize = 12.5, Margin = new Thickness(0, 3, 0, 0) });
            grid.Children.Add(copy);
            border.Child = grid;
            return border;
        }

        private UIElement BuildFooter()
        {
            var border = new Border
            {
                BorderBrush = Brush("#282830"),
                BorderThickness = new Thickness(0, 1, 0, 0),
                Padding = new Thickness(20, 13, 20, 13)
            };
            DockPanel.SetDock(border, Dock.Bottom);
            var dock = new DockPanel();

            _status = new TextBlock
            {
                Text = "Settings are stored locally. The renderer exits after every notification.",
                Foreground = Brush("#92929C"),
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = 11.5
            };
            DockPanel.SetDock(_status, Dock.Left);
            dock.Children.Add(_status);

            var actions = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
            _livePreviewButton = MakeButton("Start live preview", false);
            _livePreviewButton.Click += delegate { ToggleLivePreview(PrimaryDeviceName()); };
            var reset = MakeButton("Reset", false);
            reset.Margin = new Thickness(8, 0, 0, 0);
            reset.Click += delegate { ResetSettings(); };
            var save = MakeButton("Save", true);
            save.Margin = new Thickness(8, 0, 0, 0);
            save.Click += delegate { SaveSettings(); };
            actions.Children.Add(_livePreviewButton);
            actions.Children.Add(reset);
            actions.Children.Add(save);
            DockPanel.SetDock(actions, Dock.Right);
            dock.Children.Add(actions);
            border.Child = dock;
            return border;
        }

        private UIElement BuildAppearanceTab()
        {
            var panel = PagePanel();
            panel.Children.Add(Heading("Color palette", "Choose three colors for the animated comet. The first color also accents the island."));
            var colors = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 12, 0, 24) };
            _primaryButton = ColorButton("Primary", delegate { PickColor("primary"); });
            _secondaryButton = ColorButton("Secondary", delegate { PickColor("secondary"); });
            _highlightButton = ColorButton("Highlight", delegate { PickColor("highlight"); });
            _secondaryButton.Margin = new Thickness(10, 0, 0, 0);
            _highlightButton.Margin = new Thickness(10, 0, 0, 0);
            colors.Children.Add(_primaryButton);
            colors.Children.Add(_secondaryButton);
            colors.Children.Add(_highlightButton);
            panel.Children.Add(colors);

            panel.Children.Add(Heading("Edge pattern", "Choose how completion energy travels around the display."));
            var patternRow = new Border
            {
                Background = Brush("#19191F"),
                BorderBrush = Brush("#34343E"),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(10),
                Padding = new Thickness(3),
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(0, 12, 0, 24)
            };
            var patternButtons = new StackPanel { Orientation = Orientation.Horizontal };
            _patternComet = NavButton("Comet");
            _patternOrbit = NavButton("Dual orbit");
            _patternPulse = NavButton("Halo pulse");
            _patternCorners = NavButton("Corner spark");
            _patternComet.Click += delegate { SetEffectPattern("Comet"); };
            _patternOrbit.Click += delegate { SetEffectPattern("DualOrbit"); };
            _patternPulse.Click += delegate { SetEffectPattern("HaloPulse"); };
            _patternCorners.Click += delegate { SetEffectPattern("CornerSpark"); };
            patternButtons.Children.Add(_patternComet);
            patternButtons.Children.Add(_patternOrbit);
            patternButtons.Children.Add(_patternPulse);
            patternButtons.Children.Add(_patternCorners);
            patternRow.Child = patternButtons;
            panel.Children.Add(patternRow);

            panel.Children.Add(Heading("Animation", "Tune the edge thickness, brightness, timing, and trail length."));
            AddSlider(panel, "Edge thickness", 3, 24, out _thickness, out _thicknessValue, delegate(double value) { _settings.Thickness = value; return value.ToString("0") + " px"; });
            AddSlider(panel, "Intensity", 0.4, 1.35, out _intensity, out _intensityValue, delegate(double value) { _settings.Intensity = value; return value.ToString("0.00") + "×"; });
            AddSlider(panel, "Duration", 1.2, 5.0, out _duration, out _durationValue, delegate(double value) { _settings.DurationSeconds = value; return value.ToString("0.0") + " sec"; });
            AddSlider(panel, "Trail length", 0.15, 0.55, out _trail, out _trailValue, delegate(double value) { _settings.TrailLength = value; return Math.Round(value * 100) + "%"; });
            _frame = new CheckBox { Content = "Keep a faint outline around the full display" };
            _frame.Checked += delegate { if (!_loading) { _settings.ShowFaintFrame = true; ScheduleLivePreview(); } };
            _frame.Unchecked += delegate { if (!_loading) { _settings.ShowFaintFrame = false; ScheduleLivePreview(); } };
            panel.Children.Add(_frame);
            return Scroller(panel);
        }

        private UIElement BuildDisplaysTab()
        {
            var grid = new Grid { Margin = new Thickness(22) };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(230) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1) });
            grid.ColumnDefinitions.Add(new ColumnDefinition());

            var left = new StackPanel();
            left.Children.Add(Heading("Detected displays", "Select a screen to fit its edge path."));
            _displayList = new ListBox
            {
                Margin = new Thickness(0, 14, 18, 0),
                Background = Brush("#111116"),
                Foreground = Brushes.White,
                BorderBrush = Brush("#34343D"),
                MinHeight = 270,
                ItemContainerStyle = CreateListItemStyle()
            };
            _displayList.SelectionChanged += delegate { LoadSelectedDisplay(); };
            left.Children.Add(_displayList);
            grid.Children.Add(left);

            var divider = new Border { Background = Brush("#303038"), Width = 1 };
            Grid.SetColumn(divider, 1);
            grid.Children.Add(divider);

            var right = new StackPanel { Margin = new Thickness(24, 0, 0, 0) };
            Grid.SetColumn(right, 2);
            right.Children.Add(Heading("Screen fit", "Insets move the glow inward when a bezel, crop, or rounded panel hides part of the effect."));
            _displayEnabled = new CheckBox { Content = "Show notifications on this display", Margin = new Thickness(0, 14, 0, 12) };
            _displayEnabled.Checked += delegate { SetDisplayEnabled(true); };
            _displayEnabled.Unchecked += delegate { SetDisplayEnabled(false); };
            right.Children.Add(_displayEnabled);

            var insetGrid = new Grid { Margin = new Thickness(0, 4, 0, 14) };
            for (var i = 0; i < 4; i++) insetGrid.ColumnDefinitions.Add(new ColumnDefinition());
            _insetLeft = InsetField(insetGrid, 0, "Left");
            _insetTop = InsetField(insetGrid, 1, "Top");
            _insetRight = InsetField(insetGrid, 2, "Right");
            _insetBottom = InsetField(insetGrid, 3, "Bottom");
            right.Children.Add(insetGrid);

            AddSlider(right, "Corner radius", 0, 120, out _cornerRadius, out _cornerValue, delegate(double value)
            {
                var profile = SelectedProfile();
                if (profile != null) profile.CornerRadius = value;
                return value.ToString("0") + " px";
            });

            var displayActions = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 18, 0, 0) };
            var calibrate = MakeButton("Calibrate on screen", true);
            calibrate.Click += delegate
            {
                var screen = SelectedScreen();
                if (screen != null) LaunchCalibrationAndClose(screen.DeviceName);
            };
            displayActions.Children.Add(calibrate);
            var preview = MakeButton("Preview this display", false);
            preview.Margin = new Thickness(8, 0, 0, 0);
            preview.Click += delegate
            {
                var screen = SelectedScreen();
                if (screen != null) StartLivePreview(screen.DeviceName);
            };
            displayActions.Children.Add(preview);
            right.Children.Add(displayActions);
            grid.Children.Add(right);
            return grid;
        }

        private UIElement BuildIslandTab()
        {
            var panel = PagePanel();
            panel.Children.Add(Heading("Completion island", "A top-center pill can show completion status and an excerpt from the agent's final response."));

            var modeRow = new Grid { Margin = new Thickness(0, 16, 0, 20) };
            modeRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(150) });
            modeRow.ColumnDefinitions.Add(new ColumnDefinition());
            modeRow.Children.Add(new TextBlock { Text = "Style", VerticalAlignment = VerticalAlignment.Center, Foreground = Brush("#D8D8DE") });
            var modes = new Border
            {
                Background = Brush("#19191F"),
                BorderBrush = Brush("#34343E"),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(10),
                Padding = new Thickness(3)
            };
            var modeButtons = new StackPanel { Orientation = Orientation.Horizontal };
            _islandOff = NavButton("Off");
            _islandCompact = NavButton("Compact");
            _islandDetailed = NavButton("Detailed");
            _islandOff.Click += delegate { SetIslandMode("Off"); };
            _islandCompact.Click += delegate { SetIslandMode("Compact"); };
            _islandDetailed.Click += delegate { SetIslandMode("Detailed"); };
            modeButtons.Children.Add(_islandOff);
            modeButtons.Children.Add(_islandCompact);
            modeButtons.Children.Add(_islandDetailed);
            modes.Child = modeButtons;
            Grid.SetColumn(modes, 1);
            modeRow.Children.Add(modes);
            panel.Children.Add(modeRow);

            AddSlider(panel, "Island width", 300, 720, out _islandWidth, out _islandWidthValue, delegate(double value) { _settings.IslandWidth = value; return value.ToString("0") + " px"; });
            AddSlider(panel, "Distance from top", 8, 80, out _islandOffset, out _islandOffsetValue, delegate(double value) { _settings.IslandOffset = value; return value.ToString("0") + " px"; });
            AddSlider(panel, "Island duration", 4, 15, out _islandDuration, out _islandDurationValue, delegate(double value) { _settings.IslandDurationSeconds = value; return value.ToString("0.0") + " sec"; });
            AddSlider(panel, "Message detail", 60, 280, out _messageLength, out _messageLengthValue, delegate(double value) { _settings.MessageMaxLength = (int)value; return ((int)value) + " chars"; });

            _islandAllDisplays = new CheckBox { Content = "Show the island on every enabled display" };
            _islandAllDisplays.Checked += delegate { if (!_loading) { _settings.IslandOnAllDisplays = true; ScheduleLivePreview(); } };
            _islandAllDisplays.Unchecked += delegate { if (!_loading) { _settings.IslandOnAllDisplays = false; ScheduleLivePreview(); } };
            panel.Children.Add(_islandAllDisplays);
            _showFolder = new CheckBox { Content = "Include the task folder beside the completion title" };
            _showFolder.Checked += delegate { if (!_loading) { _settings.ShowFolder = true; ScheduleLivePreview(); } };
            _showFolder.Unchecked += delegate { if (!_loading) { _settings.ShowFolder = false; ScheduleLivePreview(); } };
            panel.Children.Add(_showFolder);
            _quickReply = new CheckBox { Content = "Let me click the island and reply to the originating Codex task" };
            _quickReply.Checked += delegate { if (!_loading) { _settings.EnableQuickReply = true; ScheduleLivePreview(); } };
            _quickReply.Unchecked += delegate { if (!_loading) { _settings.EnableQuickReply = false; ScheduleLivePreview(); } };
            panel.Children.Add(_quickReply);

            var preview = MakeButton("Show live island", true);
            preview.HorizontalAlignment = HorizontalAlignment.Left;
            preview.Margin = new Thickness(0, 20, 0, 0);
            preview.Click += delegate { StartLivePreview(PrimaryDeviceName()); };
            panel.Children.Add(preview);
            return Scroller(panel);
        }

        private void LoadAllControls()
        {
            _loading = true;
            SetColorButton(_primaryButton, _settings.ColorPrimary, "Primary");
            SetColorButton(_secondaryButton, _settings.ColorSecondary, "Secondary");
            SetColorButton(_highlightButton, _settings.ColorHighlight, "Highlight");
            SetSlider(_thickness, _thicknessValue, _settings.Thickness, _settings.Thickness.ToString("0") + " px");
            SetSlider(_intensity, _intensityValue, _settings.Intensity, _settings.Intensity.ToString("0.00") + "×");
            SetSlider(_duration, _durationValue, _settings.DurationSeconds, _settings.DurationSeconds.ToString("0.0") + " sec");
            SetSlider(_trail, _trailValue, _settings.TrailLength, Math.Round(_settings.TrailLength * 100) + "%");
            _frame.IsChecked = _settings.ShowFaintFrame;
            UpdateEffectPatternButtons();
            UpdateIslandModeButtons();
            SetSlider(_islandWidth, _islandWidthValue, _settings.IslandWidth, _settings.IslandWidth.ToString("0") + " px");
            SetSlider(_islandOffset, _islandOffsetValue, _settings.IslandOffset, _settings.IslandOffset.ToString("0") + " px");
            SetSlider(_islandDuration, _islandDurationValue, _settings.IslandDurationSeconds, _settings.IslandDurationSeconds.ToString("0.0") + " sec");
            SetSlider(_messageLength, _messageLengthValue, _settings.MessageMaxLength, _settings.MessageMaxLength + " chars");
            _islandAllDisplays.IsChecked = _settings.IslandOnAllDisplays;
            _showFolder.IsChecked = _settings.ShowFolder;
            _quickReply.IsChecked = _settings.EnableQuickReply;

            _displayList.Items.Clear();
            foreach (var screen in _screens)
            {
                _displayList.Items.Add(FriendlyName(screen));
            }
            if (_displayList.Items.Count > 0) _displayList.SelectedIndex = 0;
            _loading = false;
            LoadSelectedDisplay();
        }

        private void LoadSelectedDisplay()
        {
            var profile = SelectedProfile();
            if (profile == null) return;
            _loading = true;
            _displayEnabled.IsChecked = profile.Enabled;
            _insetLeft.Text = profile.InsetLeft.ToString("0", CultureInfo.InvariantCulture);
            _insetTop.Text = profile.InsetTop.ToString("0", CultureInfo.InvariantCulture);
            _insetRight.Text = profile.InsetRight.ToString("0", CultureInfo.InvariantCulture);
            _insetBottom.Text = profile.InsetBottom.ToString("0", CultureInfo.InvariantCulture);
            SetSlider(_cornerRadius, _cornerValue, profile.CornerRadius, profile.CornerRadius.ToString("0") + " px");
            _loading = false;
        }

        private void SaveInsets()
        {
            if (_loading) return;
            var profile = SelectedProfile();
            if (profile == null) return;
            profile.InsetLeft = ParseInset(_insetLeft.Text);
            profile.InsetTop = ParseInset(_insetTop.Text);
            profile.InsetRight = ParseInset(_insetRight.Text);
            profile.InsetBottom = ParseInset(_insetBottom.Text);
            ScheduleLivePreview();
        }

        internal void RunPreviewForTest()
        {
            StartLivePreview(PrimaryDeviceName());
            var stopTimer = new System.Windows.Threading.DispatcherTimer { Interval = TimeSpan.FromSeconds(2.5) };
            stopTimer.Tick += delegate
            {
                stopTimer.Stop();
                StopLivePreview();
                Application.Current.Shutdown();
            };
            stopTimer.Start();
        }

        internal void PreviewFromTray()
        {
            ToggleLivePreview(PrimaryDeviceName());
        }

        private void ToggleLivePreview(string deviceName)
        {
            if (_livePreviewEnabled)
            {
                StopLivePreview();
                return;
            }
            StartLivePreview(deviceName);
        }

        private void StartLivePreview(string deviceName)
        {
            SaveInsets();
            if (_livePreview == null) _livePreview = new LivePreviewController();
            _livePreviewEnabled = true;
            _previewDevice = deviceName;
            _livePreviewButton.Content = "Stop preview";
            _livePreviewButton.Style = (Style)Resources["AccentButtonStyle"];
            PushLivePreview();
            _status.Text = string.IsNullOrEmpty(deviceName)
                ? "Live preview is running — changes update as you drag."
                : "Live preview is running on the selected display.";
        }

        private void StopLivePreview()
        {
            _previewDebounce.Stop();
            _livePreviewEnabled = false;
            if (_livePreview != null) _livePreview.Hide();
            if (_livePreviewButton != null)
            {
                _livePreviewButton.Content = "Start live preview";
                _livePreviewButton.Style = (Style)Resources["ButtonStyle"];
            }
            _status.Text = "Preview stopped. Your edits are still ready to save.";
        }

        private void ScheduleLivePreview()
        {
            if (!_livePreviewEnabled || _loading) return;
            _previewDebounce.Stop();
            _previewDebounce.Start();
        }

        private void PushLivePreview()
        {
            if (!_livePreviewEnabled || _livePreview == null) return;
            _livePreview.Show(_settings, _previewDevice);
        }

        private static string PrimaryDeviceName()
        {
            return WinForms.Screen.PrimaryScreen == null ? null : WinForms.Screen.PrimaryScreen.DeviceName;
        }

        private void LaunchCalibrationAndClose(string deviceName)
        {
            SaveInsets();
            try
            {
                SettingsStore.Save(_settings);
                var executable = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
                var arguments = "--wait-for-pid " + System.Diagnostics.Process.GetCurrentProcess().Id
                    + " --calibrate --device \"" + deviceName.Replace("\"", "\\\"") + "\" --reopen-settings";
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = executable,
                    Arguments = arguments,
                    UseShellExecute = true
                });
                Application.Current.Shutdown();
            }
            catch (Exception exception)
            {
                _status.Text = "Could not start calibration: " + exception.Message;
            }
        }

        private void SaveSettings()
        {
            SaveInsets();
            try
            {
                SettingsStore.Save(_settings);
                _status.Text = "Saved. Future Codex completions will use these settings.";
            }
            catch (Exception exception)
            {
                _status.Text = "Could not save: " + exception.Message;
            }
        }

        private void ResetSettings()
        {
            _settings = new AppSettings();
            _settings.EnsureCurrentDisplays();
            LoadAllControls();
            ScheduleLivePreview();
            _status.Text = "Defaults restored in the editor. Press Save to keep them.";
        }

        private void PickColor(string slot)
        {
            var current = slot == "primary" ? _settings.ColorPrimary : slot == "secondary" ? _settings.ColorSecondary : _settings.ColorHighlight;
            var parsed = ParseMediaColor(current, Colors.Coral);
            using (var dialog = new WinForms.ColorDialog())
            {
                dialog.FullOpen = true;
                dialog.Color = Drawing.Color.FromArgb(parsed.R, parsed.G, parsed.B);
                if (dialog.ShowDialog() != WinForms.DialogResult.OK) return;
                var hex = string.Format("#{0:X2}{1:X2}{2:X2}", dialog.Color.R, dialog.Color.G, dialog.Color.B);
                if (slot == "primary") { _settings.ColorPrimary = hex; SetColorButton(_primaryButton, hex, "Primary"); }
                else if (slot == "secondary") { _settings.ColorSecondary = hex; SetColorButton(_secondaryButton, hex, "Secondary"); }
                else { _settings.ColorHighlight = hex; SetColorButton(_highlightButton, hex, "Highlight"); }
                ScheduleLivePreview();
            }
        }

        private void SetDisplayEnabled(bool enabled)
        {
            if (_loading) return;
            var profile = SelectedProfile();
            if (profile != null)
            {
                profile.Enabled = enabled;
                ScheduleLivePreview();
            }
        }

        private DisplayProfile SelectedProfile()
        {
            var screen = SelectedScreen();
            return screen == null ? null : _settings.ForScreen(screen);
        }

        private WinForms.Screen SelectedScreen()
        {
            var index = _displayList == null ? -1 : _displayList.SelectedIndex;
            return index >= 0 && index < _screens.Count ? _screens[index] : null;
        }

        private TextBox InsetField(Grid grid, int column, string label)
        {
            var stack = new StackPanel { Margin = new Thickness(column == 0 ? 0 : 6, 0, 0, 0) };
            stack.Children.Add(new TextBlock { Text = label, FontSize = 11, Foreground = Brush("#9B9BA6"), Margin = new Thickness(0, 0, 0, 5) });
            var box = new TextBox { Text = "0" };
            box.TextChanged += delegate { SaveInsets(); };
            stack.Children.Add(box);
            Grid.SetColumn(stack, column);
            grid.Children.Add(stack);
            return box;
        }

        private void AddSlider(StackPanel parent, string label, double min, double max, out Slider slider, out TextBlock valueText, Func<double, string> changed)
        {
            var row = new Grid { Margin = new Thickness(0, 10, 0, 12) };
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(150) });
            row.ColumnDefinitions.Add(new ColumnDefinition());
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(75) });
            row.Children.Add(new TextBlock { Text = label, VerticalAlignment = VerticalAlignment.Center, Foreground = Brush("#D8D8DE") });
            slider = new Slider { Minimum = min, Maximum = max, Margin = new Thickness(12, 0, 14, 0), VerticalAlignment = VerticalAlignment.Center };
            Grid.SetColumn(slider, 1);
            row.Children.Add(slider);
            valueText = new TextBlock { TextAlignment = TextAlignment.Right, VerticalAlignment = VerticalAlignment.Center, Foreground = Brush("#AFAFBA") };
            Grid.SetColumn(valueText, 2);
            row.Children.Add(valueText);
            var capturedSlider = slider;
            var capturedText = valueText;
            slider.ValueChanged += delegate
            {
                if (_loading) return;
                capturedText.Text = changed(capturedSlider.Value);
                ScheduleLivePreview();
            };
            parent.Children.Add(row);
        }

        private static void SetSlider(Slider slider, TextBlock value, double number, string label)
        {
            slider.Value = number;
            value.Text = label;
        }

        private static double ParseInset(string text)
        {
            double value;
            return double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out value) ? Math.Max(0, Math.Min(500, value)) : 0;
        }

        private static string FriendlyName(WinForms.Screen screen)
        {
            var number = new string(screen.DeviceName.Where(char.IsDigit).ToArray());
            var label = (screen.Primary ? "Primary  ·  " : string.Empty) + "Display " + (string.IsNullOrEmpty(number) ? "" : number);
            return label.TrimEnd() + "    " + screen.Bounds.Width + " × " + screen.Bounds.Height;
        }

        private static StackPanel PagePanel()
        {
            return new StackPanel { Margin = new Thickness(26, 22, 26, 28) };
        }

        private static ScrollViewer Scroller(UIElement child)
        {
            return new ScrollViewer { VerticalScrollBarVisibility = ScrollBarVisibility.Auto, Content = child };
        }

        private static StackPanel Heading(string title, string description)
        {
            var panel = new StackPanel();
            panel.Children.Add(new TextBlock { Text = title, FontSize = 16, FontWeight = FontWeights.SemiBold });
            panel.Children.Add(new TextBlock { Text = description, Foreground = Brush("#92929D"), FontSize = 12, Margin = new Thickness(0, 5, 0, 0), TextWrapping = TextWrapping.Wrap });
            return panel;
        }

        private void ShowPage(UIElement page, Button active)
        {
            if (_pageHost == null) return;
            _pageHost.Content = page;
            SetNavState(_appearanceNav, active == _appearanceNav);
            SetNavState(_displaysNav, active == _displaysNav);
            SetNavState(_islandNav, active == _islandNav);
        }

        private static void SetNavState(Button button, bool active)
        {
            if (button == null) return;
            if (active)
            {
                button.Background = Brush("#2C2326");
                button.BorderBrush = Brush("#5A3538");
                button.Foreground = Brush("#FFB0AA");
            }
            else
            {
                button.ClearValue(Control.BackgroundProperty);
                button.ClearValue(Control.BorderBrushProperty);
                button.ClearValue(Control.ForegroundProperty);
            }
        }

        private void SetIslandMode(string mode)
        {
            if (!_loading) _settings.IslandMode = mode;
            UpdateIslandModeButtons();
            ScheduleLivePreview();
        }

        private void SetEffectPattern(string pattern)
        {
            if (!_loading) _settings.EffectPattern = pattern;
            UpdateEffectPatternButtons();
            ScheduleLivePreview();
        }

        private void UpdateEffectPatternButtons()
        {
            SetNavState(_patternComet, string.Equals(_settings.EffectPattern, "Comet", StringComparison.OrdinalIgnoreCase));
            SetNavState(_patternOrbit, string.Equals(_settings.EffectPattern, "DualOrbit", StringComparison.OrdinalIgnoreCase));
            SetNavState(_patternPulse, string.Equals(_settings.EffectPattern, "HaloPulse", StringComparison.OrdinalIgnoreCase));
            SetNavState(_patternCorners, string.Equals(_settings.EffectPattern, "CornerSpark", StringComparison.OrdinalIgnoreCase));
        }

        private void UpdateIslandModeButtons()
        {
            SetNavState(_islandOff, string.Equals(_settings.IslandMode, "Off", StringComparison.OrdinalIgnoreCase));
            SetNavState(_islandCompact, string.Equals(_settings.IslandMode, "Compact", StringComparison.OrdinalIgnoreCase));
            SetNavState(_islandDetailed, string.Equals(_settings.IslandMode, "Detailed", StringComparison.OrdinalIgnoreCase));
        }

        private Button NavButton(string text)
        {
            return new Button
            {
                Content = text,
                Style = (Style)Resources["NavButtonStyle"]
            };
        }

        private Button MakeButton(string text, bool accent)
        {
            return new Button
            {
                Content = text,
                Style = (Style)Resources[accent ? "AccentButtonStyle" : "ButtonStyle"]
            };
        }

        private Button ColorButton(string label, RoutedEventHandler click)
        {
            var button = new Button
            {
                Width = 170,
                Height = 58,
                HorizontalContentAlignment = HorizontalAlignment.Left,
                Style = (Style)Resources["ButtonStyle"]
            };
            button.Click += click;
            button.Content = label;
            return button;
        }

        private static void SetColorButton(Button button, string hex, string label)
        {
            var row = new StackPanel { Orientation = Orientation.Horizontal };
            row.Children.Add(new Border
            {
                Width = 24,
                Height = 24,
                CornerRadius = new CornerRadius(12),
                Background = new SolidColorBrush(ParseMediaColor(hex, Colors.Coral)),
                Margin = new Thickness(0, 0, 10, 0)
            });
            var copy = new StackPanel();
            copy.Children.Add(new TextBlock { Text = label, Foreground = Brush("#F1F1F4"), FontWeight = FontWeights.SemiBold, FontSize = 12.5 });
            copy.Children.Add(new TextBlock { Text = hex.ToUpperInvariant(), Foreground = Brush("#888894"), FontSize = 10.5, Margin = new Thickness(0, 2, 0, 0) });
            row.Children.Add(copy);
            button.Content = row;
        }

        private static Style CreateButtonStyle(bool accent)
        {
            var style = new Style(typeof(Button));
            style.Setters.Add(new Setter(Control.ForegroundProperty, Brushes.White));
            style.Setters.Add(new Setter(Control.BackgroundProperty, Brush(accent ? "#FF6363" : "#222228")));
            style.Setters.Add(new Setter(Control.BorderBrushProperty, Brush(accent ? "#FF7D76" : "#3A3A44")));
            style.Setters.Add(new Setter(Control.BorderThicknessProperty, new Thickness(1)));
            style.Setters.Add(new Setter(Control.PaddingProperty, new Thickness(16, 9, 16, 9)));
            style.Setters.Add(new Setter(Control.CursorProperty, System.Windows.Input.Cursors.Hand));
            style.Setters.Add(new Setter(Control.TemplateProperty, CreateRoundedButtonTemplate(9)));
            var hover = new Trigger { Property = UIElement.IsMouseOverProperty, Value = true };
            hover.Setters.Add(new Setter(Control.BackgroundProperty, Brush(accent ? "#FF746D" : "#2C2C34")));
            hover.Setters.Add(new Setter(Control.BorderBrushProperty, Brush(accent ? "#FF938D" : "#4A4A56")));
            style.Triggers.Add(hover);
            var pressed = new Trigger { Property = Button.IsPressedProperty, Value = true };
            pressed.Setters.Add(new Setter(Control.BackgroundProperty, Brush(accent ? "#E95656" : "#1B1B20")));
            style.Triggers.Add(pressed);
            return style;
        }

        private static Style CreateNavButtonStyle()
        {
            var style = new Style(typeof(Button));
            style.Setters.Add(new Setter(Control.ForegroundProperty, Brush("#A6A6B0")));
            style.Setters.Add(new Setter(Control.BackgroundProperty, Brushes.Transparent));
            style.Setters.Add(new Setter(Control.BorderBrushProperty, Brushes.Transparent));
            style.Setters.Add(new Setter(Control.BorderThicknessProperty, new Thickness(1)));
            style.Setters.Add(new Setter(Control.PaddingProperty, new Thickness(17, 8, 17, 8)));
            style.Setters.Add(new Setter(FrameworkElement.MinWidthProperty, 88.0));
            style.Setters.Add(new Setter(Control.CursorProperty, System.Windows.Input.Cursors.Hand));
            style.Setters.Add(new Setter(Control.TemplateProperty, CreateRoundedButtonTemplate(8)));
            var hover = new Trigger { Property = UIElement.IsMouseOverProperty, Value = true };
            hover.Setters.Add(new Setter(Control.BackgroundProperty, Brush("#222229")));
            hover.Setters.Add(new Setter(Control.ForegroundProperty, Brush("#EEEEF2")));
            style.Triggers.Add(hover);
            return style;
        }

        private static Style CreateListItemStyle()
        {
            var style = new Style(typeof(ListBoxItem));
            style.Setters.Add(new Setter(Control.ForegroundProperty, Brush("#B9B9C2")));
            style.Setters.Add(new Setter(Control.BackgroundProperty, Brushes.Transparent));
            style.Setters.Add(new Setter(Control.PaddingProperty, new Thickness(11, 10, 11, 10)));
            style.Setters.Add(new Setter(Control.MarginProperty, new Thickness(4, 3, 4, 0)));
            style.Setters.Add(new Setter(Control.HorizontalContentAlignmentProperty, HorizontalAlignment.Stretch));
            style.Setters.Add(new Setter(Control.TemplateProperty, CreateRoundedListItemTemplate()));
            var selected = new Trigger { Property = ListBoxItem.IsSelectedProperty, Value = true };
            selected.Setters.Add(new Setter(Control.BackgroundProperty, Brush("#2C2326")));
            selected.Setters.Add(new Setter(Control.ForegroundProperty, Brush("#FFB0AA")));
            style.Triggers.Add(selected);
            var hover = new Trigger { Property = UIElement.IsMouseOverProperty, Value = true };
            hover.Setters.Add(new Setter(Control.BackgroundProperty, Brush("#202026")));
            style.Triggers.Add(hover);
            return style;
        }

        private static ControlTemplate CreateRoundedListItemTemplate()
        {
            var border = new FrameworkElementFactory(typeof(Border));
            border.SetValue(Border.CornerRadiusProperty, new CornerRadius(8));
            border.SetValue(Border.BackgroundProperty, new TemplateBindingExtension(Control.BackgroundProperty));
            var presenter = new FrameworkElementFactory(typeof(ContentPresenter));
            presenter.SetValue(ContentPresenter.ContentProperty, new TemplateBindingExtension(ContentControl.ContentProperty));
            presenter.SetValue(ContentPresenter.MarginProperty, new TemplateBindingExtension(Control.PaddingProperty));
            border.AppendChild(presenter);
            return new ControlTemplate(typeof(ListBoxItem)) { VisualTree = border };
        }

        private static ControlTemplate CreateRoundedButtonTemplate(double radius)
        {
            var border = new FrameworkElementFactory(typeof(Border));
            border.SetValue(Border.CornerRadiusProperty, new CornerRadius(radius));
            border.SetValue(Border.BackgroundProperty, new TemplateBindingExtension(Control.BackgroundProperty));
            border.SetValue(Border.BorderBrushProperty, new TemplateBindingExtension(Control.BorderBrushProperty));
            border.SetValue(Border.BorderThicknessProperty, new TemplateBindingExtension(Control.BorderThicknessProperty));
            var presenter = new FrameworkElementFactory(typeof(ContentPresenter));
            presenter.SetValue(ContentPresenter.ContentProperty, new TemplateBindingExtension(ContentControl.ContentProperty));
            presenter.SetValue(ContentPresenter.ContentTemplateProperty, new TemplateBindingExtension(ContentControl.ContentTemplateProperty));
            presenter.SetValue(ContentPresenter.MarginProperty, new TemplateBindingExtension(Control.PaddingProperty));
            presenter.SetValue(FrameworkElement.HorizontalAlignmentProperty, HorizontalAlignment.Center);
            presenter.SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center);
            border.AppendChild(presenter);
            return new ControlTemplate(typeof(Button)) { VisualTree = border };
        }

        private static ControlTemplate CreateTextBoxTemplate()
        {
            var border = new FrameworkElementFactory(typeof(Border));
            border.SetValue(Border.CornerRadiusProperty, new CornerRadius(8));
            border.SetValue(Border.BackgroundProperty, new TemplateBindingExtension(Control.BackgroundProperty));
            border.SetValue(Border.BorderBrushProperty, new TemplateBindingExtension(Control.BorderBrushProperty));
            border.SetValue(Border.BorderThicknessProperty, new TemplateBindingExtension(Control.BorderThicknessProperty));
            border.SetValue(Border.PaddingProperty, new TemplateBindingExtension(Control.PaddingProperty));
            var host = new FrameworkElementFactory(typeof(ScrollViewer));
            host.Name = "PART_ContentHost";
            border.AppendChild(host);
            return new ControlTemplate(typeof(TextBox)) { VisualTree = border };
        }

        private static ImageSource LoadAppIcon()
        {
            try
            {
                using (var icon = Drawing.Icon.ExtractAssociatedIcon(Process.GetCurrentProcess().MainModule.FileName))
                {
                    var source = Imaging.CreateBitmapSourceFromHIcon(icon.Handle, Int32Rect.Empty, BitmapSizeOptions.FromWidthAndHeight(96, 96));
                    source.Freeze();
                    return source;
                }
            }
            catch { return null; }
        }

        private static SolidColorBrush Brush(string hex)
        {
            return new SolidColorBrush(ParseMediaColor(hex, Colors.Transparent));
        }

        private static System.Windows.Media.Color ParseMediaColor(string value, System.Windows.Media.Color fallback)
        {
            try { return (System.Windows.Media.Color)ColorConverter.ConvertFromString(value); }
            catch { return fallback; }
        }
    }
}
