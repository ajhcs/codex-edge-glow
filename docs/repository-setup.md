# GitHub repository setup

This is the maintainer checklist for `ajhcs/codex-edge-glow`.

## About panel

**Description**

> Customizable Windows edge lighting and a floating task-completion island for Codex agents.

**Website**

```text
https://github.com/ajhcs/codex-edge-glow/releases/latest
```

**Topics**

```text
windows
windows-11
csharp
winforms
desktop-app
notifications
edge-lighting
floating-island
multi-monitor
per-monitor-dpi
productivity
codex
open-source
```

Upload `assets/social-preview.png` under **Settings → General → Social preview**. It is already 1280 × 640 and below GitHub's 1 MB limit.

## Features

Enable:

- Issues
- Discussions
- Preserve this repository (when available)
- Private vulnerability reporting
- Immutable releases

Disable Wiki unless it gains a clear ownership model; project documentation belongs in versioned `docs/` files.

Recommended Discussion categories:

- Announcements — announcement format, maintainer posts only
- Q&A — question/answer format
- Ideas — open-ended
- Show and tell — open-ended
- Themes and patterns — open-ended
- General — open-ended

Pin the welcome post from [launch-copy.md](launch-copy.md).

## Labels

Create or normalize:

| Label | Purpose |
| --- | --- |
| `bug` | Confirmed or credible defect |
| `enhancement` | Bounded product improvement |
| `feature` | New user-facing capability |
| `needs-triage` | Maintainer has not classified the report |
| `good first issue` | Genuinely bounded newcomer task |
| `help wanted` | Scoped work where outside help is useful |
| `accessibility` | Keyboard, assistive technology, contrast, motion |
| `performance` | Memory, CPU, allocation, animation smoothness |
| `design` | Visual/interaction behavior |
| `documentation` | README/docs/examples |
| `dependencies` | Dependency maintenance |
| `github-actions` | Workflow maintenance |
| `security` | Public hardening work only; vulnerabilities stay private |
| `skip-changelog` | Exclude from generated release notes |

## Default branch

Create a `main` ruleset that:

- requires a pull request before merge;
- requires the `build-and-test` status check;
- requires conversation resolution;
- blocks force pushes and deletion;
- allows the sole maintainer to merge without an impossible second-review requirement until another active maintainer exists.

Add CodeQL as a required check only after the first successful default-branch run confirms its stable check name.

## Security and release settings

- Enable private vulnerability reporting before the public announcement.
- Enable Dependabot alerts and Actions version updates.
- Review Actions permissions and keep the default token read-only.
- Enable immutable releases before publishing `v0.1.0`.
- Create the release as a draft, verify the downloaded candidate, then publish.
- Do not upload a replacement asset under an already published version.

## Profile discovery

Pin the repository on `@ajhcs` after the first release exists. Add the profile snippet from [launch-copy.md](launch-copy.md) if a profile README is created later.

## Launch readiness gate

- [ ] License confirmed and present at repository root
- [ ] CI and CodeQL green on `main`
- [ ] First draft release downloads and runs on a clean Windows account or VM
- [ ] SHA-256 manifest and attestations verify
- [ ] Unsigned/SmartScreen disclosure is visible
- [ ] Social preview uploaded
- [ ] Discussions and private vulnerability reporting enabled
- [ ] At least five scoped issues exist, including two honest `good first issue` tasks
- [ ] Repository pinned only after visitors can download a real release

