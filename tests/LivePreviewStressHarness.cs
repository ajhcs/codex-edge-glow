using System;
using System.Diagnostics;
using System.Threading;
using WinForms = System.Windows.Forms;

namespace CodexEdgeGlow
{
    internal static class LivePreviewStressHarness
    {
        public static int Main()
        {
            var settings = new AppSettings { IslandMode = "Detailed" };
            settings.EnsureCurrentDisplays();
            var primary = WinForms.Screen.PrimaryScreen;
            if (primary == null) return 1;

            var patterns = new[] { "Comet", "DualOrbit", "HaloPulse", "CornerSpark" };
            var stopwatch = Stopwatch.StartNew();
            using (var preview = new LivePreviewController())
            {
                for (var index = 0; index < 24; index++)
                {
                    settings.EffectPattern = patterns[index % patterns.Length];
                    settings.Thickness = 3 + (index % 12);
                    settings.Intensity = 0.55 + ((index % 8) * 0.1);
                    settings.TrailLength = 0.15 + ((index % 9) * 0.04);
                    settings.IslandMode = index % 3 == 0 ? "Compact" : "Detailed";
                    settings.IslandWidth = 320 + ((index % 8) * 40);
                    preview.Show(settings, primary.DeviceName);
                    Thread.Sleep(25);
                }
                Thread.Sleep(750);
                preview.Hide();
                Thread.Sleep(100);
            }

            Console.WriteLine("PASS: 24 live preview updates applied without restarting the settings process in "
                + stopwatch.ElapsedMilliseconds + " ms");
            return 0;
        }
    }
}
