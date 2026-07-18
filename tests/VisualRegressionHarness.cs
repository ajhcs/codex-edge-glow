using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using WinForms = System.Windows.Forms;

namespace CodexEdgeGlow
{
    internal static class VisualRegressionHarness
    {
        public static int Main()
        {
            var failures = 0;
            var settings = new AppSettings
            {
                IslandMode = "Detailed",
                IslandWidth = 500,
                Thickness = 9,
                TrailLength = 0.31
            };
            settings.EnsureCurrentDisplays();
            var screen = WinForms.Screen.PrimaryScreen;
            if (screen == null)
            {
                Console.WriteLine("FAIL: no primary screen");
                return 1;
            }

            using (var island = new IslandForm(screen, settings, new NotificationData { ThreadId = "qa" }, Stopwatch.StartNew(), false))
            {
                var nativeSendButtons = island.Controls.OfType<WinForms.Button>().Count(button => button.Text == "Send");
                failures += Check(nativeSendButtons == 0,
                    "island Send action is painted in the island surface (no clipped child HWND)",
                    "island still contains " + nativeSendButtons + " native Send button control(s)");
            }

            var profile = settings.ForScreen(screen);
            var strip = EdgeGeometry.Strips(screen.Bounds.Size, settings, profile).First();
            using (var edge = new EdgeStripForm(screen, strip, settings, profile, Stopwatch.StartNew(), false))
            {
                failures += Check(edge.TransparencyKey == Color.Empty,
                    "edge overlay uses per-pixel alpha instead of a binary color key",
                    "edge overlay still uses TransparencyKey " + edge.TransparencyKey);
            }

            var path = EdgeGeometry.Path(new Size(1920, 1080), settings, new DisplayProfile { CornerRadius = 34 });
            var track = new RoundedTrack(path, 34);
            var trailLength = settings.TrailLength * track.Perimeter;
            var segments = EdgePainter.SegmentCountFor(trailLength);
            var maxChord = 0.0;
            for (var index = 0; index < segments; index++)
            {
                var first = track.PointAt(trailLength * index / segments);
                var second = track.PointAt(trailLength * (index + 1) / segments);
                var dx = second.X - first.X;
                var dy = second.Y - first.Y;
                maxChord = Math.Max(maxChord, Math.Sqrt((dx * dx) + (dy * dy)));
            }
            failures += Check(maxChord <= 6.0,
                "trail geometry samples stay within 6 px for a smooth continuous curve",
                "trail chord reaches " + maxChord.ToString("0.0") + " px with only " + segments + " samples");

            Console.WriteLine(failures == 0 ? "PASS: visual regression checks" : "FAIL: " + failures + " visual regression check(s)");
            return failures == 0 ? 0 : 1;
        }

        private static int Check(bool condition, string pass, string fail)
        {
            Console.WriteLine((condition ? "PASS: " : "FAIL: ") + (condition ? pass : fail));
            return condition ? 0 : 1;
        }
    }
}
