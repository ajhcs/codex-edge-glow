using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Web.Script.Serialization;

namespace CodexEdgeGlow
{
    internal static class Program
    {
        [STAThread]
        private static void Main(string[] args)
        {
            DpiAwareness.EnablePerMonitorV2();
            ForwardToChainedNotifier(args);
            WaitForParentIfRequested(args);

            var settings = SettingsStore.Load();
            if (args.Any(arg => string.Equals(arg, "--calibrate", StringComparison.OrdinalIgnoreCase)))
            {
                CalibrationMode.Run(settings, ValueAfter(args, "--device"));
                ReopenSettingsIfRequested(args);
                return;
            }
            var settingsPreviewTest = args.Any(arg => string.Equals(arg, "--settings-preview-test", StringComparison.OrdinalIgnoreCase));
            if (settingsPreviewTest || args.Any(arg => string.Equals(arg, "--settings", StringComparison.OrdinalIgnoreCase)))
            {
                SettingsMode.Run(settings, settingsPreviewTest);
                return;
            }

            var previewHold = args.Any(arg => string.Equals(arg, "--preview-hold", StringComparison.OrdinalIgnoreCase));
            var preview = previewHold || args.Any(arg => string.Equals(arg, "--preview", StringComparison.OrdinalIgnoreCase));
            if (previewHold) settings.DurationSeconds = 8;
            var notification = preview
                ? NotificationData.Sample()
                : NotificationData.FromArguments(args);
            var device = ValueAfter(args, "--device");
            OverlaySession.RunStandalone(settings, notification, device);
            ReopenSettingsIfRequested(args);
        }

        private static void ReopenSettingsIfRequested(string[] args)
        {
            if (!args.Any(arg => string.Equals(arg, "--reopen-settings", StringComparison.OrdinalIgnoreCase))) return;
            Process.Start(new ProcessStartInfo
            {
                FileName = Process.GetCurrentProcess().MainModule.FileName,
                Arguments = "--settings",
                UseShellExecute = true
            });
        }

        private static void WaitForParentIfRequested(string[] args)
        {
            var value = ValueAfter(args, "--wait-for-pid");
            int processId;
            if (!int.TryParse(value, out processId)) return;
            try
            {
                using (var process = Process.GetProcessById(processId)) process.WaitForExit(10000);
            }
            catch
            {
                // The parent may already have exited.
            }
        }

        private static string ValueAfter(string[] args, string name)
        {
            for (var index = 0; index < args.Length - 1; index++)
            {
                if (string.Equals(args[index], name, StringComparison.OrdinalIgnoreCase)) return args[index + 1];
            }
            return null;
        }

        private static void ForwardToChainedNotifier(string[] args)
        {
            var chainIndex = Array.IndexOf(args, "--chain");
            if (chainIndex < 0 || chainIndex + 1 >= args.Length)
            {
                return;
            }

            var executable = args[chainIndex + 1];
            if (!File.Exists(executable))
            {
                return;
            }

            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = executable,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    Arguments = string.Join(" ", args.Skip(chainIndex + 2).Select(QuoteArgument))
                };
                Process.Start(startInfo);
            }
            catch
            {
                // A visual notification must never interfere with task completion.
            }
        }

        internal static string QuoteArgument(string argument)
        {
            if (argument.Length > 0 && argument.All(c => !char.IsWhiteSpace(c) && c != '"'))
            {
                return argument;
            }

            var quoted = new StringBuilder("\"");
            var backslashes = 0;
            foreach (var character in argument)
            {
                if (character == '\\')
                {
                    backslashes++;
                    continue;
                }
                if (character == '"')
                {
                    quoted.Append('\\', (backslashes * 2) + 1);
                    quoted.Append('"');
                    backslashes = 0;
                    continue;
                }
                quoted.Append('\\', backslashes);
                quoted.Append(character);
                backslashes = 0;
            }
            quoted.Append('\\', backslashes * 2);
            quoted.Append('"');
            return quoted.ToString();
        }
    }

    internal static class DpiAwareness
    {
        [DllImport("user32.dll", EntryPoint = "SetProcessDpiAwarenessContext")]
        private static extern bool SetProcessDpiAwarenessContext(IntPtr value);

        [DllImport("shcore.dll", EntryPoint = "SetProcessDpiAwareness")]
        private static extern int SetProcessDpiAwareness(int value);

        [DllImport("user32.dll", EntryPoint = "SetProcessDPIAware")]
        private static extern bool SetProcessDpiAware();

        public static void EnablePerMonitorV2()
        {
            try
            {
                if (SetProcessDpiAwarenessContext(new IntPtr(-4))) return;
            }
            catch (EntryPointNotFoundException) { }
            catch (DllNotFoundException) { }

            try
            {
                if (SetProcessDpiAwareness(2) == 0) return;
            }
            catch (EntryPointNotFoundException) { }
            catch (DllNotFoundException) { }

            try { SetProcessDpiAware(); }
            catch (EntryPointNotFoundException) { }
            catch (DllNotFoundException) { }
        }
    }

    internal sealed class NotificationData
    {
        public string Title { get; set; }
        public string Message { get; set; }
        public string Folder { get; set; }
        public string WorkingDirectory { get; set; }
        public string ThreadId { get; set; }

        public NotificationData()
        {
            Title = "Codex finished";
            Message = "Your agent completed the task.";
            Folder = string.Empty;
            WorkingDirectory = string.Empty;
            ThreadId = string.Empty;
        }

        public static NotificationData Sample()
        {
            return new NotificationData
            {
                Title = "Codex finished",
                Message = "The requested changes are complete. All verification checks passed, the updated files are ready, and a concise summary is available in the originating task.",
                Folder = "Preview task",
                ThreadId = string.Empty
            };
        }

        public static NotificationData FromArguments(string[] args)
        {
            var result = new NotificationData();
            var json = args.LastOrDefault(arg => arg.TrimStart().StartsWith("{", StringComparison.Ordinal));
            if (string.IsNullOrWhiteSpace(json))
            {
                return result;
            }

            try
            {
                var serializer = new JavaScriptSerializer();
                var payload = serializer.Deserialize<Dictionary<string, object>>(json);
                object value;
                if (payload.TryGetValue("last-assistant-message", out value) && value != null)
                {
                    result.Message = Clean(value.ToString());
                }
                else if (payload.TryGetValue("last_assistant_message", out value) && value != null)
                {
                    result.Message = Clean(value.ToString());
                }

                if (payload.TryGetValue("cwd", out value) && value != null)
                {
                    var path = value.ToString();
                    result.WorkingDirectory = path;
                    result.Folder = Path.GetFileName(path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
                }

                if (payload.TryGetValue("thread-id", out value) && value != null)
                {
                    result.ThreadId = value.ToString();
                }
                else if (payload.TryGetValue("thread_id", out value) && value != null)
                {
                    result.ThreadId = value.ToString();
                }
            }
            catch
            {
                // Unknown payloads still receive a useful generic completion message.
            }
            return result;
        }

        private static string Clean(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return "Your agent completed the task.";
            }
            var cleaned = text.Replace("\r", " ").Replace("\n", " ").Replace("**", string.Empty).Trim();
            while (cleaned.Contains("  "))
            {
                cleaned = cleaned.Replace("  ", " ");
            }
            return cleaned;
        }
    }
}
