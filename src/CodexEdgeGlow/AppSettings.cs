using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using WinForms = System.Windows.Forms;

namespace CodexEdgeGlow
{
    [Serializable]
    public sealed class AppSettings
    {
        public int Version { get; set; }
        public string ColorPrimary { get; set; }
        public string ColorSecondary { get; set; }
        public string ColorHighlight { get; set; }
        public double Thickness { get; set; }
        public double Intensity { get; set; }
        public double DurationSeconds { get; set; }
        public double TrailLength { get; set; }
        public bool ShowFaintFrame { get; set; }
        public string EffectPattern { get; set; }
        public string IslandMode { get; set; }
        public double IslandWidth { get; set; }
        public double IslandOffset { get; set; }
        public double IslandDurationSeconds { get; set; }
        public int MessageMaxLength { get; set; }
        public bool IslandOnAllDisplays { get; set; }
        public bool ShowFolder { get; set; }
        public bool EnableQuickReply { get; set; }
        public List<DisplayProfile> Displays { get; set; }

        public AppSettings()
        {
            Version = 3;
            ColorPrimary = "#FF6363";
            ColorSecondary = "#FFB55C";
            ColorHighlight = "#EE53B5";
            Thickness = 9;
            Intensity = 1;
            DurationSeconds = 2.65;
            TrailLength = 0.31;
            ShowFaintFrame = true;
            EffectPattern = "Comet";
            IslandMode = "Detailed";
            IslandWidth = 500;
            IslandOffset = 18;
            IslandDurationSeconds = 7.5;
            MessageMaxLength = 150;
            IslandOnAllDisplays = false;
            ShowFolder = true;
            EnableQuickReply = true;
            Displays = new List<DisplayProfile>();
        }

        public void EnsureCurrentDisplays()
        {
            if (Displays == null)
            {
                Displays = new List<DisplayProfile>();
            }
            foreach (var screen in WinForms.Screen.AllScreens)
            {
                if (Displays.All(item => !string.Equals(item.DeviceName, screen.DeviceName, StringComparison.OrdinalIgnoreCase)))
                {
                    Displays.Add(new DisplayProfile { DeviceName = screen.DeviceName, Enabled = true });
                }
            }
        }

        public DisplayProfile ForScreen(WinForms.Screen screen)
        {
            EnsureCurrentDisplays();
            return Displays.First(item => string.Equals(item.DeviceName, screen.DeviceName, StringComparison.OrdinalIgnoreCase));
        }
    }

    [Serializable]
    public sealed class DisplayProfile
    {
        public string DeviceName { get; set; }
        public bool Enabled { get; set; }
        public double InsetLeft { get; set; }
        public double InsetTop { get; set; }
        public double InsetRight { get; set; }
        public double InsetBottom { get; set; }
        public double CornerRadius { get; set; }

        public DisplayProfile()
        {
            DeviceName = string.Empty;
            Enabled = true;
            CornerRadius = 34;
        }
    }

    internal static class SettingsStore
    {
        private static readonly string DirectoryPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "CodexEdgeGlow");
        public static readonly string SettingsPath = Path.Combine(DirectoryPath, "settings.xml");

        public static AppSettings Load()
        {
            try
            {
                if (File.Exists(SettingsPath))
                {
                    using (var stream = File.OpenRead(SettingsPath))
                    {
                        var loaded = (AppSettings)new XmlSerializer(typeof(AppSettings)).Deserialize(stream);
                        if (loaded.Version < 2)
                        {
                            loaded.IslandDurationSeconds = 7.5;
                            loaded.EnableQuickReply = true;
                        }
                        if (loaded.Version < 3 || string.IsNullOrWhiteSpace(loaded.EffectPattern))
                        {
                            loaded.EffectPattern = "Comet";
                        }
                        loaded.Version = 3;
                        loaded.EnsureCurrentDisplays();
                        return loaded;
                    }
                }
            }
            catch
            {
                // A corrupt settings file falls back to safe defaults.
            }

            var settings = new AppSettings();
            settings.EnsureCurrentDisplays();
            return settings;
        }

        public static void Save(AppSettings settings)
        {
            Directory.CreateDirectory(DirectoryPath);
            var temporary = SettingsPath + ".tmp";
            using (var stream = File.Create(temporary))
            {
                new XmlSerializer(typeof(AppSettings)).Serialize(stream, settings);
            }
            if (File.Exists(SettingsPath))
            {
                File.Replace(temporary, SettingsPath, SettingsPath + ".bak", true);
            }
            else
            {
                File.Move(temporary, SettingsPath);
            }
        }
    }
}
