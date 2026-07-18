# Troubleshooting

## Manual preview works, but tasks do not trigger it

- Confirm `notify` is a top-level key in `%USERPROFILE%\.codex\config.toml`.
- Use an absolute executable path with doubled backslashes.
- Restart Codex after editing its configuration.
- Test with Codex CLI; other Codex hosts may not invoke the external hook.
- If another notifier exists, use one chained command rather than defining `notify` twice.

## The glow outlines only part of the screen

Use a build with per-monitor-v2 DPI awareness, then restart the app. Open **Displays → Preview this display**. The physical-bounds regression test compares the preview union with Windows' physical monitor size and catches the common 125%/150% virtualization error.

If all four edges are present but deliberately inset, reset that display profile or run calibration.

## The glow is clipped by a bezel or rounded panel

Open **Displays**, select the monitor, and choose **Calibrate on screen**. Adjust edge inset, corner radius, and line width while watching the full-screen guide. Use the minus/plus controls or arrow keys for one-pixel changes.

## The animation is jagged or disconnected

- Confirm you are running the newest release.
- Test the Comet pattern at default thickness and 31% trail length.
- Update the graphics driver if layered-window composition is visibly corrupted.
- Include a screen recording, resolution, refresh rate, scaling percentage, and pattern settings in the bug report.

Do not capture unrelated apps, task content, usernames, or private folders.

## The island is missing

- Check that Island mode is Compact or Detailed.
- On multiple monitors, verify whether **Show the island on every enabled display** is enabled.
- Increase **Distance from top** if another top-aligned control overlaps it.
- The edge effect can be active while the island is intentionally disabled.

## Quick reply is unavailable

Preview samples do not carry a live task ID, so they display a non-interactive hint. Test with a real completion event containing `thread-id`/`thread_id`.

If the live event contains a task ID, set `CODEX_CLI_PATH` to the full `codex.exe` path or place it on `PATH`.

## The tray icon is missing

The icon exists while the settings process is running. Minimize the settings window to hide it in the notification area. Windows may place it in the overflow drawer; use the taskbar's hidden-icons control. Closing settings exits that resident process.

## Windows shows an unrecognized-app warning

Early releases are unsigned and may not have SmartScreen reputation. Download only from this repository, verify the SHA-256 manifest, and read [Installation](installation.md). Never disable SmartScreen globally.

## Report a useful bug

Use the structured [bug form](https://github.com/ajhcs/codex-edge-glow/issues/new?template=bug.yml) and include:

- app version and download source;
- Windows version/build;
- monitor resolutions, positions, and scaling;
- pattern/island settings;
- exact reproduction steps;
- whether live preview reproduces it;
- sanitized visuals and observed memory use when relevant.

