# Security policy

## Supported versions

Until version 1.0, security fixes are provided for the newest published release only.

| Version | Supported |
| --- | --- |
| Latest release | Yes |
| Older releases | No |

## Report a vulnerability privately

Use GitHub's **Report a vulnerability** button on the repository Security page when available. If private vulnerability reporting is unavailable, email `cole@colelyons.com` with the subject `Codex Edge Glow security report`.

Do not disclose an unpatched vulnerability in an Issue, Discussion, pull request, screenshot, or public fork.

Include, when possible:

- affected version and download source;
- Windows version and architecture;
- impact and realistic attack scenario;
- reproduction steps or a minimal proof of concept;
- relevant files/functions;
- suggested mitigation;
- whether the report or artifacts contain sensitive data.

You should receive acknowledgment within seven calendar days. Validation, remediation, and disclosure timing depend on severity and complexity. The maintainer will coordinate a reasonable disclosure date and credit reporters who want public acknowledgment.

## Security boundaries

The app processes notification JSON supplied by the launching Codex process, writes local settings, creates layered topmost windows, and can optionally invoke the local Codex CLI for quick reply. Reports involving command construction, task routing, local executable discovery, configuration tampering, release integrity, or unintended content exposure are in scope.

General bugs, visual artifacts, and feature requests belong in the public issue forms unless they create a security or privacy impact.

