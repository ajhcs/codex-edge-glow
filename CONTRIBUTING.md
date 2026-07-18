# Contributing to Codex Edge Glow

Thank you for helping make agent completion easier to notice on Windows. Contributions can be code, documentation, accessibility feedback, display testing, design references, translations, presets, or carefully reproduced bugs.

Please follow the [Code of Conduct](CODE_OF_CONDUCT.md) in every project space.

## Before you start

- Search existing issues and discussions.
- Use Discussions for early ideas and setup questions.
- Open or claim an issue before investing in a large behavior or architecture change.
- Keep each pull request focused on one coherent outcome.
- Never include task text, local usernames, private folder names, credentials, or unrelated desktop content in screenshots and logs.

## Development environment

You need Windows, PowerShell, and either Visual Studio 2022 Build Tools with the .NET Framework 4.8 targeting pack or the .NET Framework compiler included with Windows.

```powershell
git clone https://github.com/ajhcs/codex-edge-glow.git
cd codex-edge-glow
.\scripts\build.ps1 -Configuration Release
.\scripts\test.ps1
```

Build output is written under `artifacts/`, which is ignored by Git.

## Test layers

- `VisualRegressionHarness.cs` checks parent-painted island actions, per-pixel alpha, and continuous trail geometry.
- `LivePreviewStressHarness.cs` applies rapid pattern, geometry, and island updates without restarting settings.
- `DpiPreviewRegression.ps1` compares live-preview bounds with the monitor's physical pixel bounds.
- Manual checks cover focus behavior, tray lifecycle, calibration, multiple displays, and animation quality.

The DPI and live-preview checks open real topmost windows. Run them only in an interactive Windows session. CI runs the deterministic non-interactive subset.

## Pull request expectations

Every pull request should include:

1. the user problem and the chosen solution;
2. a linked issue when one exists;
3. tests run and their results;
4. before/after images or a short recording for visible changes;
5. monitor resolution, scaling, and layout for display-related changes;
6. keyboard, contrast, and reduced-motion considerations;
7. working-set measurements for rendering or lifecycle changes;
8. updated docs and changelog when behavior changes.

Avoid unrelated formatting or cleanup. Review is faster when the behavioral diff is easy to isolate.

## UI and rendering changes

- Preserve click-through behavior and avoid stealing focus.
- Test at 100%, 125%, 150%, and 200% scaling when the change touches coordinates or window bounds.
- Test at least one non-primary or offset monitor when the change touches screen selection.
- Keep partially transparent antialias pixels; do not reintroduce binary transparency keys or control-region clipping.
- Prefer a deterministic rendering seam and a regression test before changing animation geometry.
- Treat the less-than-100-MB working-set target as a product constraint.

## Commit and PR style

- Use a concise imperative title, for example `Fix preview bounds on mixed-DPI displays`.
- Explain why the behavior changed, not only which files changed.
- Let maintainers squash or reword commits during merge when needed.

## Review and decisions

Maintainers may request narrower scope, additional evidence, accessibility work, or memory measurements. A contribution may be declined when it adds disproportionate complexity, depends on unstable private interfaces, violates a platform policy, or cannot be maintained safely. The goal is a clear explanation, not a silent closure.

For vulnerabilities, do not open a public issue; follow [SECURITY.md](SECURITY.md).

