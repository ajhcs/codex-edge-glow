# Seed issues for launch

Create these only after confirming the acceptance criteria still match the released code. Apply `good first issue` conservatively.

## 1. Document a mixed-DPI test matrix

**Labels:** `documentation`, `good first issue`, `help wanted`

Create a reusable table covering 100%, 125%, 150%, and 200% scaling across primary/secondary monitor positions. Document setup, expected physical bounds, commands, and sanitized evidence requirements.

**Acceptance criteria**

- A new `docs/testing-displays.md` explains the matrix.
- It covers left/right/above monitor offsets and mixed scaling.
- It links the physical-bounds regression and manual calibration flow.
- It contains no private desktop captures.

## 2. Add explicit accessible names to settings sliders

**Labels:** `accessibility`, `good first issue`

Associate each WPF slider with an accessible name that includes its label and expose the current value without changing visual layout.

**Acceptance criteria**

- Narrator announces purpose and value for every slider.
- Keyboard behavior is unchanged.
- The contribution includes the Windows/Narrator version and manual results.

## 3. Add a high-contrast preset

**Labels:** `accessibility`, `design`, `help wanted`

Define a user-selectable preset with high-luminance separation that preserves pattern readability without relying only on color.

**Acceptance criteria**

- Preset is visible in settings and updates live preview.
- Existing custom palettes remain unchanged.
- Contrast rationale and before/after screenshots are documented.
- Working set remains under the target on the reference flow.

## 4. Add reduced-motion notification mode

**Labels:** `accessibility`, `enhancement`, `help wanted`

Design a completion mode that fades a static perimeter/island in and out without an object traveling around the display.

**Acceptance criteria**

- The mode can be selected independently of color and island style.
- Live preview reflects it.
- Timing avoids abrupt flashing.
- Regression coverage distinguishes reduced-motion from animated patterns.

## 5. Add localization-ready UI string resources

**Labels:** `enhancement`, `help wanted`

Move visible settings, tray, calibration, and island status strings into a maintainable resource boundary without translating them yet.

**Acceptance criteria**

- No visible source string is silently missed.
- Default English behavior and layout remain unchanged.
- Contributor documentation explains how to add a locale later.

## 6. Add a conservative notification-hook installer

**Labels:** `enhancement`, `security`, `help wanted`

Design an installer/helper that adds Codex Edge Glow to `config.toml` without destroying an existing notifier. Existing configuration must be backed up and ambiguous input must stop for user review.

**Acceptance criteria**

- No existing `notify` command is overwritten silently.
- Direct and chained configurations have tests with spaces/quotes.
- A dry-run prints the proposed change.
- Uninstall restores the previous value.

## 7. Add a secondary-monitor physical-bounds regression

**Labels:** `bug`, `performance`, `help wanted`

Extend the DPI regression beyond the primary monitor, including negative virtual-desktop coordinates.

**Acceptance criteria**

- The harness accepts a target device.
- It validates the selected display's physical rectangle, not global metrics.
- It handles monitors positioned left or above the primary.
- It skips clearly when the requested test topology is unavailable.

## 8. Investigate sustainable Authenticode signing

**Labels:** `security`, `documentation`, `help wanted`

Compare maintainable code-signing paths for an open-source Windows utility, including identity, key protection, timestamping, recurring cost, CI integration, and contributor trust boundaries.

**Acceptance criteria**

- Findings cite Microsoft and provider primary sources.
- No private key is stored in repository secrets or ordinary workflow logs.
- The recommendation includes ongoing ownership and cost.
- Documentation does not tell users to bypass SmartScreen.

