# Launch copy

Use these as starting points. Update version numbers, hashes, and known limitations from the final release candidate.

## Repository description

> Customizable Windows edge lighting and a floating task-completion island for Codex agents.

## One-line pitch

> Know when your agent is done—even when Codex is behind another window or on another monitor.

## Release title

```text
Codex Edge Glow v0.1.0 — first public preview
```

## Release body

```markdown
Codex Edge Glow turns a finished Codex task into a smooth animation around the physical edges of your Windows display, plus an optional top-center island with the result and quick reply.

### Included

- Four edge patterns with customizable color, thickness, intensity, timing, and trail length
- Compact and detailed island modes with dismiss, pin, overflow handling, and optional quick reply
- Per-display enablement, insets, corner-radius calibration, multi-monitor support, and per-monitor DPI awareness
- Persistent live preview that updates while controls change
- Notification-area resident with open, preview, and exit actions

### Compatibility

- Tested on Windows 11
- Portable .NET Framework 4.8 executable
- Automatic completion events use the Codex CLI external `notify` hook
- Codex Desktop/app-server compatibility can vary by release

### Trust and verification

This preview is unsigned, so Windows SmartScreen may show an unrecognized-app warning. Download only from this release and verify `SHA256SUMS.txt`. GitHub provenance attestations are attached to the executable and portable archive.

### Performance observations

On the reference Windows 11 system, the standalone overlay peaked at 51.9 MB working set and settings with live preview peaked at 87.8 MB. Results vary by system.

### Feedback requested

Please include your Windows build, monitor resolutions/positions, scaling percentages, chosen pattern, and whether the Codex completion hook fired. Remove private task and desktop content from captures.

See the README for installation, known limitations, troubleshooting, and contribution paths.
```

## Announcement Discussion

**Title**

```text
Codex Edge Glow v0.1.0 is ready for Windows testers
```

**Body**

```markdown
I built Codex Edge Glow because I kept missing the moment a background agent finished. A normal toast was too easy to lose, but a full interruption was too much.

The result is a lightweight Windows utility with two independent surfaces: ambient edge lighting for awareness, and an optional completion island for detail and quick reply.

The first public preview is ready. I am especially looking for reports from:

- mixed-DPI or multi-monitor setups;
- non-standard monitor positions and rounded panels;
- high-contrast, keyboard-only, and reduced-motion users;
- people who already chain another Codex notification command.

Please share the Windows build, display layout/scaling, pattern, island mode, and exact behavior. Sanitize screenshots and recordings before posting.

Download: https://github.com/ajhcs/codex-edge-glow/releases/latest
```

## Pinned welcome Discussion

**Title**

```text
Welcome — where to ask, report, and contribute
```

**Body**

```markdown
Welcome to Codex Edge Glow.

- Use **Q&A** for installation, integration, and display setup.
- Use **Ideas** for concepts that are not ready to become issues.
- Use **Show and tell** or **Themes and patterns** for sanitized setups and presets.
- Use structured **Issues** for reproducible bugs and scoped work.
- Report vulnerabilities privately through the Security tab.

Search before posting, follow the Code of Conduct, and remove private task text, usernames, folder paths, credentials, and unrelated desktop content from attachments.

New contributors can start with the `good first issue` label or ask for context in the relevant issue.
```

## GitHub profile README snippet

```markdown
### Codex Edge Glow

[Codex Edge Glow](https://github.com/ajhcs/codex-edge-glow) is a lightweight Windows utility that adds customizable edge lighting and a floating completion island when Codex agents finish. It supports live preview, multi-monitor calibration, per-monitor DPI, and optional quick reply.
```

## Ethical promotion checklist

- Link the canonical repository or release—not a re-upload.
- Post only where the tool directly solves the topic under discussion.
- Ask for concrete display/integration feedback rather than stars.
- Do not create promotional issues or comments in unrelated repositories.
- Do not buy, trade, automate, or reward stars/follows.
- Credit early reporters and contributors in release notes.
- Turn repeated questions into better documentation.

