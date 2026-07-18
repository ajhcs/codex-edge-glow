# Installation and verification

## Requirements

- Windows 11 is the currently tested operating system.
- Windows 10 may work because the renderer uses supported Win32 and .NET Framework APIs, but it is not yet part of the release test matrix.
- Automatic completion events require a Codex surface that invokes the external `notify` command. Codex CLI supports this hook; other hosts may differ.

## Install the portable build

1. Open the [latest release](https://github.com/ajhcs/codex-edge-glow/releases/latest).
2. Download the versioned Windows executable and `SHA256SUMS.txt`.
3. Create `%LOCALAPPDATA%\CodexEdgeGlow`.
4. Move the executable there and optionally rename it to `codex-edge-glow.exe`.
5. Verify the download before running it.

```powershell
$app = "$env:LOCALAPPDATA\CodexEdgeGlow\codex-edge-glow.exe"
Get-FileHash $app -Algorithm SHA256
```

Compare the result with `SHA256SUMS.txt` on the same release. When release attestations are available, GitHub CLI users can also verify provenance:

```powershell
gh attestation verify $app -R ajhcs/codex-edge-glow
```

## Windows SmartScreen

Early releases are unsigned. Windows may display an unrecognized-app warning because a new unsigned executable has no established publisher reputation. This warning is not proof of malware, but it is a reason to verify the source and hash carefully.

Never download copies from reposting sites, disable SmartScreen globally, or run a binary whose hash does not match the release manifest.

## Open and preview

```powershell
& "$env:LOCALAPPDATA\CodexEdgeGlow\codex-edge-glow.exe" --settings
```

Use **Start live preview**. Confirm the glow reaches all four physical edges of the intended display and that the island is not hidden behind a taskbar or top-aligned system control.

## Connect Codex

Open `%USERPROFILE%\.codex\config.toml` and add:

```toml
notify = ["C:\\Users\\YOUR_NAME\\AppData\\Local\\CodexEdgeGlow\\codex-edge-glow.exe"]
```

Use doubled backslashes inside the TOML string. Restart Codex CLI, run a short task, and let the turn complete.

If the file already contains `notify`, do not add a second key. Read [Integrations](integrations.md) for chaining guidance.

## Uninstall

1. Exit the app from its notification-area menu.
2. Remove or restore the `notify` entry in `%USERPROFILE%\.codex\config.toml`.
3. Delete `%LOCALAPPDATA%\CodexEdgeGlow` if you no longer need the executable or settings.

The app does not install a service, driver, browser extension, scheduled task, or system-wide registry entry.

