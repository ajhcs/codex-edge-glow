param(
    [string]$Executable = ".\codex-edge-glow.exe"
)

Add-Type -TypeDefinition @'
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

public static class PreviewBoundsProbe
{
    [StructLayout(LayoutKind.Sequential)]
    public struct RECT { public int Left, Top, Right, Bottom; }

    public delegate bool EnumWindowsProc(IntPtr hwnd, IntPtr value);

    [DllImport("user32.dll")] public static extern bool EnumWindows(EnumWindowsProc callback, IntPtr value);
    [DllImport("user32.dll")] public static extern bool IsWindowVisible(IntPtr hwnd);
    [DllImport("user32.dll")] public static extern bool GetWindowRect(IntPtr hwnd, out RECT rect);
    [DllImport("user32.dll")] public static extern uint GetWindowThreadProcessId(IntPtr hwnd, out uint processId);
    [DllImport("user32.dll", CharSet = CharSet.Unicode)] public static extern int GetWindowText(IntPtr hwnd, StringBuilder value, int count);
    [DllImport("user32.dll")] public static extern bool SetProcessDpiAwarenessContext(IntPtr value);
    [DllImport("user32.dll")] public static extern int GetSystemMetrics(int index);

    public static RECT[] UntitledVisibleWindows(uint processId)
    {
        var result = new List<RECT>();
        EnumWindows(delegate(IntPtr hwnd, IntPtr unused)
        {
            uint owner;
            GetWindowThreadProcessId(hwnd, out owner);
            if (owner != processId || !IsWindowVisible(hwnd)) return true;
            var text = new StringBuilder(256);
            GetWindowText(hwnd, text, text.Capacity);
            RECT rect;
            if (text.Length == 0 && GetWindowRect(hwnd, out rect) && rect.Right > rect.Left && rect.Bottom > rect.Top)
                result.Add(rect);
            return true;
        }, IntPtr.Zero);
        return result.ToArray();
    }
}
'@

[PreviewBoundsProbe]::SetProcessDpiAwarenessContext([IntPtr](-4)) | Out-Null
$physicalWidth = [PreviewBoundsProbe]::GetSystemMetrics(0)
$physicalHeight = [PreviewBoundsProbe]::GetSystemMetrics(1)
$process = Start-Process -FilePath $Executable -ArgumentList '--settings-preview-test' -PassThru
try {
    Start-Sleep -Milliseconds 1400
    $windows = [PreviewBoundsProbe]::UntitledVisibleWindows([uint32]$process.Id)
    if ($windows.Count -lt 4) { throw "Expected four edge windows; found $($windows.Count)." }

    $left = ($windows | Measure-Object Left -Minimum).Minimum
    $top = ($windows | Measure-Object Top -Minimum).Minimum
    $right = ($windows | Measure-Object Right -Maximum).Maximum
    $bottom = ($windows | Measure-Object Bottom -Maximum).Maximum
    $previewWidth = $right - $left
    $previewHeight = $bottom - $top
    $matches = $previewWidth -eq $physicalWidth -and $previewHeight -eq $physicalHeight
    "preview=${previewWidth}x${previewHeight} physical=${physicalWidth}x${physicalHeight}"
    if (-not $matches) {
        Write-Error 'FAIL: live preview outlines a DPI-scaled logical rectangle instead of the full physical screen.'
        exit 1
    }
    'PASS: live preview covers the full physical screen.'
}
finally {
    if (-not $process.HasExited) { Stop-Process -Id $process.Id -ErrorAction SilentlyContinue }
}
