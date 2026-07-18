# Release process

This document is for maintainers.

## Prepare

1. Confirm CI is green on `main`.
2. Update `CHANGELOG.md` and assembly versions.
3. Run the complete interactive suite on Windows:

   ```powershell
   .\scripts\test.ps1
   ```

4. Test settings, live preview, tray lifecycle, a real Codex completion, quick reply, calibration, and at least one mixed-DPI layout.
5. Confirm the working-set target and record the test configuration.
6. Confirm documentation, screenshots, signed/unsigned status, and known limitations.

## Tag and build

Create a signed annotated tag when possible:

```powershell
git tag -s v0.1.0 -m "Codex Edge Glow v0.1.0"
git push origin v0.1.0
```

The release workflow builds on a GitHub-hosted Windows runner, creates versioned assets and `SHA256SUMS.txt`, generates provenance attestations, and opens a **draft** GitHub Release.

## Verify the draft

- Download every draft asset.
- Verify hashes and provenance.
- Run the exact downloaded executable on a clean Windows account or VM.
- Review generated notes, contributors, compatibility, install steps, and known issues.
- Add screenshots only when they contain no private desktop or task content.

Publish only after the asset set is final. If immutable releases are enabled, never silently replace a published executable; create a new patch release.

## After publishing

- Create the associated announcement Discussion.
- Thank contributors by name/handle.
- Pin the release when appropriate.
- Ask for specific feedback: Windows build, monitor layout/scaling, chosen pattern, and whether the notification hook fired.
- Watch early issues and turn repeated questions into documentation.

