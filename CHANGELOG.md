# Changelog

All notable changes to Codex Edge Glow are recorded here. The project follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/) and intends to use [Semantic Versioning](https://semver.org/spec/v2.0.0.html) after the first public release.

## [Unreleased]

### Planned

- First public packaging and release automation
- Clean-machine installation verification
- Wider multi-monitor and mixed-DPI testing

## [0.1.0] - 2026-07-17

### Added

- Per-monitor edge-light notification renderer with four animation patterns
- Compact and detailed completion-island modes
- Dismiss, pin, text overflow handling, and optional quick reply
- Three-color palette and detailed timing/geometry controls
- Per-display enablement, insets, corner-radius calibration, and DPI awareness
- Persistent live preview with in-place setting updates
- Notification-area resident with open, preview, and exit actions
- Local XML settings with migration support
- Regression harnesses for rendering structure, preview stress, and physical screen bounds

### Changed

- Replaced binary transparency-key rendering with premultiplied per-pixel alpha
- Rebuilt the island send action into the parent-drawn surface for smoother edges
- Increased trail sampling density to eliminate disconnected segment artifacts

[Unreleased]: https://github.com/ajhcs/codex-edge-glow/compare/v0.1.0...HEAD
[0.1.0]: https://github.com/ajhcs/codex-edge-glow/releases/tag/v0.1.0

