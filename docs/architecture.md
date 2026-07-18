# Architecture

Codex Edge Glow is a small .NET Framework 4.8 Windows executable that combines WPF for the settings editor with Windows Forms/GDI+ for calibration and overlays.

## Runtime flow

```text
Codex notify event
        |
        v
Program parses the final JSON argument
        |
        +--> optional chained notifier
        |
        v
OverlaySession enumerates enabled screens
        |
        +--> four click-through EdgeStripForm windows per screen
        |
        +--> optional interactive IslandForm
        |
        v
60 FPS timer renders until each surface reaches its lifetime
```

Normal notification processes exit after their visible surfaces close. Settings mode owns a notification-area icon and exits when the settings application closes.

## Modules

- `Program.cs` — command routing, DPI initialization, notification parsing, safe chained-command quoting
- `AppSettings.cs` — versioned settings model, per-display profiles, XML persistence
- `Overlay.cs` — lifecycle, live-preview controller, layered windows, island interaction, edge geometry and painting, quick reply
- `SettingsWindow.cs` — WPF editor, tray host, live update scheduling
- `CalibrationForm.cs` — full-screen physical fit guide and single-pixel controls

## Rendering boundary

Each monitor uses four narrow edge-strip windows rather than a full-screen transparent bitmap. That keeps allocation and composition costs proportional to the perimeter instead of total screen area.

Edge strips use `UpdateLayeredWindow` with a 32-bit premultiplied-alpha surface. This preserves partially transparent antialias pixels; binary `TransparencyKey` rendering cannot. Non-interactive strips are click-through and no-activate. The island remains interactive but uses `ShowWithoutActivation` so a completion does not steal focus.

The moving trail samples the rounded monitor path densely enough to keep adjacent chords below six pixels in the baseline regression. Comet and orbit patterns draw a continuous body plus one explicit rounded head rather than a chain of individually capped segments.

## Live preview

The WPF settings dispatcher remains responsive while a dedicated STA thread runs a Windows Forms message loop. UI changes are debounced, copied into a settings snapshot, and applied by rebuilding only the preview forms. This avoids saving, shutting down, and launching a second process for every comparison.

WPF settings rendering is forced to software mode to keep the combined settings-and-preview working set under the project target on the reference system. The overlay still relies on Windows desktop composition for layered windows.

## DPI and monitor geometry

`Program.Main` enables per-monitor-v2 DPI awareness before either UI framework initializes. This ordering is essential: initializing WPF first can virtualize a 150%-scaled monitor into logical coordinates and produce a preview that covers only two-thirds of the physical display.

`DpiPreviewRegression.ps1` launches the real live-preview path, enumerates its edge windows, and asserts that their union matches Windows' physical primary-display bounds.

## Data and trust boundaries

- Notification JSON is untrusted process input and is reduced to a few optional strings.
- Settings stay in the current user's local application-data directory.
- Chained executables run only when explicitly supplied in the configured command line.
- Quick reply uses a fixed Codex CLI command shape, a routed task ID, standard input for user text, and optional working directory.
- No updater, telemetry, browser engine, service, driver, or inbound listener exists.

## Performance constraint

The less-than-100-MB working-set target is part of feature design. Prefer narrow windows, bounded sample counts, short-lived surfaces, cached immutable assets, and explicit lifecycle cleanup. Rendering changes should be measured with settings closed, settings idle, live preview active, and a standalone notification.

