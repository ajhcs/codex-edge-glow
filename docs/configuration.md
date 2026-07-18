# Configuration reference

Open the editor with:

```powershell
.\codex-edge-glow.exe --settings
```

Settings are saved to `%LOCALAPPDATA%\CodexEdgeGlow\settings.xml`. Edit through the app when possible; malformed files fall back to defaults.

## Appearance

| Setting | Range/default | Effect |
| --- | --- | --- |
| Primary | `#FF6363` | Leading glow and island accent |
| Secondary | `#FFB55C` | Mid-trail blend |
| Highlight | `#EE53B5` | Tail/accent blend |
| Pattern | `Comet` | Comet, Dual orbit, Halo pulse, or Corner spark |
| Thickness | 3–24 px; default 9 | Visible notification line width |
| Intensity | 0.40–1.35×; default 1.00× | Alpha and glow strength |
| Duration | 1.2–5.0 seconds; default 2.65 | One edge-animation pass |
| Trail length | 15–55%; default 31% | Portion of the perimeter occupied by the moving trail |
| Faint frame | On | Keeps a low-opacity outline visible during the effect |

## Displays

Every detected monitor receives a profile keyed by its Windows device name.

- **Enabled** controls whether that monitor receives notifications.
- **Insets** move individual edges inward when a bezel, crop, or unusual panel hides part of the glow.
- **Corner radius** matches the visible curve of the physical panel.
- **Calibrate on screen** opens a full-display guide with single-pixel controls.
- **Preview this display** starts the live preview only on the selected monitor.

Display coordinates use per-monitor-v2 DPI awareness. When Windows scaling changes, restart settings before recalibrating so monitor bounds are re-enumerated cleanly.

## Completion island

| Setting | Effect |
| --- | --- |
| Off | No island; edge lighting can remain enabled |
| Compact | Title-focused pill with minimal height |
| Detailed | Larger result excerpt and metadata |
| Width | 300–720 px content width |
| Distance from top | 8–80 px |
| Island duration | 4–15 seconds; always at least two seconds longer than the edge effect |
| Message detail | 60–280 characters before word-aware ellipsis |
| Show on every display | Replicates the island across enabled displays |
| Include task folder | Adds the final working-directory segment beside the title |
| Quick reply | Lets a live completion expand into a reply field when the event contains a task ID |

Hovering the island pauses automatic dismissal. Use the close action to dismiss or the pin action to keep it visible.

## Live preview

Live preview runs on a dedicated single-threaded Windows Forms message loop inside the settings process. Changes are debounced briefly, then the active overlay is rebuilt from a settings snapshot. The edge animation loops continuously; the island remains visible for inspection.

Use live preview to compare options. Use **Calibrate on screen** when the question is physical fit rather than visual style.

