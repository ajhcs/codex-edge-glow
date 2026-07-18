# GitHub launch research for Codex Edge Glow

Research date: 2026-07-17

## Scope and method

This note translates first-party guidance from GitHub, the GitHub-maintained Open Source Guides, Microsoft, OSI, and SPDX into a launch plan for a small open-source Windows utility. It covers presentation, discoverability, releases, contributor onboarding, community health, automation, security, and ethical GitHub-native promotion. Recommendations specific to Codex Edge Glow are labeled as recommendations; platform behavior is cited directly.

## Executive recommendation

Treat the repository as three products at once:

1. A **trustworthy download page** for Windows users.
2. A **clear technical project** that developers can understand and build.
3. A **welcoming contribution funnel** that turns users into reporters, contributors, and eventually maintainers.

The strongest launch is not a badge-heavy README or a one-time announcement. It is a polished repository landing page, a verifiable release, a short path from problem to install, several genuinely approachable issues, and visible, kind maintenance after launch. GitHub's own Open Source Guides describe documentation, an open-source license, contribution guidelines, and a code of conduct as core launch materials, and emphasize that users become contributors because a project solves a real problem for them. [Starting an Open Source Project](https://opensource.guide/starting-a-project/) [Building Welcoming Communities](https://opensource.guide/building-community/)

For Codex Edge Glow, the launch message should be direct:

> Get an unmistakable edge-light notification and an interactive floating island when an agent finishes a task on Windows.

Use a factual independence statement near the first third of the README:

> Codex Edge Glow is an independent, community-built utility and is not affiliated with or endorsed by OpenAI, Microsoft, Samsung, Apple, or Xiaomi.

This avoids any implication of official association. GitHub prohibits impersonation and fraudulent misrepresentation of identity or purpose. [GitHub Acceptable Use Policies](https://docs.github.com/en/site-policy/acceptable-use-policies/github-acceptable-use-policies)

## 1. Repository identity and discoverability

### Recommended repository metadata

**Repository name:** `codex-edge-glow`

**Description:**

> Customizable Windows edge lighting and a floating task-completion island for Codex agents.

Keep the description factual and short enough to remain readable in search results, profile pins, and link cards. Avoid unverified superlatives such as “the fastest” or “the best.”

**Website field:** Initially use the stable latest-release page, then replace it with a project site only if one is maintained:

`https://github.com/OWNER/codex-edge-glow/releases/latest`

GitHub provides a permanent `/releases/latest` URL and a `/releases/latest/download/ASSET-NAME` pattern for manually uploaded assets. [Linking to releases](https://docs.github.com/en/repositories/releasing-projects-on-github/linking-to-releases)

**Recommended topics:**

- `windows`
- `windows-11`
- `csharp`
- `winforms`
- `desktop-app`
- `notifications`
- `edge-lighting`
- `floating-island`
- `multi-monitor`
- `per-monitor-dpi`
- `productivity`
- `codex`
- `open-source`

Topics help people find related solutions and contribution opportunities. GitHub permits at most 20 topics; names must be lowercase, no longer than 50 characters, and use letters, numbers, and hyphens. Topic names are public. [Classifying your repository with topics](https://docs.github.com/en/repositories/managing-your-repositorys-settings-and-features/customizing-your-repository/classifying-your-repository-with-topics)

Use `codex` because it describes a real integration, but pair it with the independence disclosure. Prefer `floating-island` over a product-brand term in metadata unless there is a concrete compatibility reason to use the latter.

### Social preview

Create a dedicated **1280 × 640** image under 1 MB with:

- the app logo and name;
- the one-sentence outcome;
- a single clean Windows desktop crop showing the edge glow and island;
- a solid dark background that works predictably when shared into light and dark interfaces.

GitHub recommends 1280 × 640 for best display, accepts PNG/JPG/GIF under 1 MB, and recommends a solid background when cross-platform transparency behavior is uncertain. [Customizing a repository's social media preview](https://docs.github.com/en/repositories/managing-your-repositorys-settings-and-features/customizing-your-repository/customizing-your-repositorys-social-media-preview)

Do not reuse a dense settings screenshot as the social card. The card's job is recognition and comprehension at small size, not documentation.

### Profile placement

Pin the repository to the maintainer's public GitHub profile and mention it briefly in the profile README. GitHub supports up to six pinned repositories/gists, and pins are explicitly intended to help visitors quickly find a person's best work. [Pinning items to your profile](https://docs.github.com/en/account-and-profile/how-tos/profile-customization/pinning-items-to-your-profile) [About your profile](https://docs.github.com/en/account-and-profile/concepts/personal-profile)

## 2. README information architecture

GitHub says a README is often the first thing a visitor sees and should explain what the project does, why it is useful, how to start, where to get help, and who maintains it. GitHub automatically surfaces a recognized README and recommends keeping longer documentation outside it. README content beyond 500 KiB is truncated. [About repository READMEs](https://docs.github.com/en/repositories/managing-your-repositorys-settings-and-features/customizing-your-repository/about-readmes)

### Recommended order

1. **Logo, project name, and one-sentence value proposition**
2. **Hero screenshot** with meaningful alt text
3. **Primary actions:** Download latest, View releases, Read installation guide
4. **At-a-glance compatibility:** supported Windows versions, portable/install format, measured memory target
5. **What it does:** concise feature list
6. **Two distinct concepts:** edge lighting versus floating island
7. **Quick start:** download, verify, run, configure, preview
8. **Customization gallery:** edge patterns, colors, displays, island modes
9. **Privacy and permissions:** what data stays local, what the app reads, what it sends; only make claims verified in code
10. **Windows download warning:** signed/unsigned status and honest SmartScreen expectations
11. **Troubleshooting:** DPI/display fit, tray icon, preview, startup, agent integration
12. **Resource use and compatibility:** how the memory figure was measured and on what system
13. **Roadmap and known limitations**
14. **Contributing:** one inviting paragraph plus links to `CONTRIBUTING.md` and good first issues
15. **Support, security, code of conduct, and license**
16. **Independence and inspiration acknowledgements**

### Above-the-fold example

```markdown
# Codex Edge Glow

Customizable Windows edge lighting and a floating task-completion island for Codex agents.

![Codex Edge Glow illuminating all four edges of a Windows desktop while a compact completion island appears at the top center.](docs/assets/hero.png)

[Download the latest release](https://github.com/OWNER/codex-edge-glow/releases/latest) · [Installation](docs/installation.md) · [Contributing](CONTRIBUTING.md)

Windows 11 · Multi-monitor and per-monitor DPI aware · Designed to remain under 100 MB RAM
```

If the 100 MB claim is retained, identify it as a design target and publish the measured build/test conditions. A reproducible, bounded statement earns more trust than an absolute marketing claim.

### Screenshot and motion strategy

Use one static hero screenshot, then a small gallery of purposeful static images. A short linked demo video can show motion better than a large autoplaying GIF. Essential instructions and feature descriptions must exist as text, not only inside images or animation.

GitHub Markdown supports image alt text and relative repository paths. GitHub defines alt text as a short text equivalent of an image's information and recommends relative links for repository-owned images. [Basic writing and formatting syntax](https://docs.github.com/en/get-started/writing-on-github/getting-started-with-writing-and-formatting-on-github/basic-writing-and-formatting-syntax) [About repository READMEs](https://docs.github.com/en/repositories/managing-your-repositorys-settings-and-features/customizing-your-repository/about-readmes)

For accessibility:

- use one `#` title followed by sequential `##` and `###` headings;
- write descriptive link text instead of “click here”;
- give every meaningful image concise alt text that explains its purpose;
- do not communicate required steps only through screenshots;
- use plain language and define specialized terms;
- keep emoji decorative and sparse;
- show a static fallback for any animation.

These practices follow GitHub's first-party accessible README guidance. [5 tips for making your GitHub profile page accessible](https://github.blog/developer-skills/github/5-tips-for-making-your-github-profile-page-accessible/) GitHub also notes that screenshots should not be the only source of procedural information. [Creating screenshots](https://docs.github.com/en/contributing/writing-for-github-docs/creating-screenshots)

### Documentation split

Keep the README fast to scan and move depth into:

- `docs/installation.md`
- `docs/configuration.md`
- `docs/integrations.md`
- `docs/troubleshooting.md`
- `docs/architecture.md`
- `docs/releasing.md`
- `docs/accessibility.md`

Use relative links so navigation works both on GitHub and in a clone. GitHub automatically rewrites relative paths for the current branch. [About repository READMEs](https://docs.github.com/en/repositories/managing-your-repositorys-settings-and-features/customizing-your-repository/about-readmes)

## 3. Releases and Windows download trust

### Release contents

GitHub Releases are designed to package deployable software, release notes, and binary files. [About releases](https://docs.github.com/en/repositories/releasing-projects-on-github/about-releases)

Every production release should include:

- a versioned executable or installer with a predictable name, for example `CodexEdgeGlow-1.0.0-win-x64.exe`;
- optionally a portable ZIP if it is genuinely supported;
- `SHA256SUMS.txt`;
- exact Windows and architecture requirements;
- installation and update steps;
- a “What's changed” section organized into Added, Improved, Fixed, and Known issues;
- upgrade or configuration migration notes when relevant;
- a link to the full changelog;
- contributor acknowledgements.

Do not call an asset `setup.exe` without a version or architecture: predictable versioned names make support, mirroring, and verification easier.

### Draft, verify, then publish immutably

Enable **immutable releases** and follow this order:

1. Create a draft release.
2. Build and test from the tagged commit.
3. Attach every final asset, checksum manifest, and provenance artifact.
4. Review the notes and download/install the candidate on a clean Windows environment.
5. Publish once complete.

Immutable releases lock the published tag and assets and automatically create a release attestation. GitHub specifically recommends the draft-first workflow so all assets are present before immutability takes effect. Corrections should be a new release, not a silently replaced executable. [Immutable releases](https://docs.github.com/en/code-security/concepts/supply-chain-security/immutable-releases)

Document advanced verification commands:

```powershell
gh release verify v1.0.0
gh release verify-asset v1.0.0 .\CodexEdgeGlow-1.0.0-win-x64.exe
Get-FileHash .\CodexEdgeGlow-1.0.0-win-x64.exe -Algorithm SHA256
```

GitHub supports `gh release verify` and `gh release verify-asset` for immutable releases. [Verifying the integrity of a release](https://docs.github.com/en/code-security/how-tos/secure-your-supply-chain/secure-your-dependencies/verify-release-integrity) PowerShell's `Get-FileHash` defaults to SHA-256 and is intended to verify that file contents have not changed. [Get-FileHash](https://learn.microsoft.com/en-us/powershell/module/microsoft.powershell.utility/get-filehash)

A checksum published beside a compromised binary cannot by itself prove origin. Prefer immutable releases plus GitHub artifact attestations, with SHA-256 as an accessible additional check.

### Build provenance

Generate a GitHub artifact attestation for the shipped binary. GitHub says artifact attestations establish where and how an artifact was built and are available for public repositories on current plans. Consumers can verify a binary with:

```powershell
gh attestation verify .\CodexEdgeGlow-1.0.0-win-x64.exe -R OWNER/codex-edge-glow
```

[Using artifact attestations to establish provenance for builds](https://docs.github.com/en/actions/how-tos/secure-your-work/use-artifact-attestations/use-artifact-attestations)

Attestation proves provenance, not that the software is bug-free or safe. GitHub explicitly makes that distinction. [Artifact attestations](https://docs.github.com/en/actions/concepts/security/artifact-attestations)

### Authenticode and SmartScreen

For a public Windows executable, code signing is highly desirable. Microsoft says signing helps establish authenticity and integrity, while unsigned apps build SmartScreen reputation separately for each new version. A consistent trusted signing identity lets publisher reputation carry across versions; Microsoft Store distribution avoids SmartScreen download warnings because Store apps are signed by Microsoft. [SmartScreen reputation for Windows app developers](https://learn.microsoft.com/en-us/windows/apps/package-and-deploy/smartscreen-reputation) [Use SignTool to sign a file](https://learn.microsoft.com/en-us/windows/win32/seccrypto/using-signtool-to-sign-a-file)

Recommended trust progression:

1. **Launch minimum:** immutable GitHub Release, SHA-256 manifest, artifact attestation, transparent unsigned-status note.
2. **Preferred:** Authenticode-sign every executable with a consistent trusted publisher identity and timestamp it.
3. **Later distribution:** evaluate MSIX/Microsoft Store if the packaging and update model fit.

Never tell users to disable SmartScreen. Explain what warning they may see, link only to the official GitHub release, show how to verify the artifact, and disclose whether the publisher signature is present.

## 4. Community health files

GitHub's community profile checks public repositories for recognized files including README, LICENSE, CONTRIBUTING, CODE_OF_CONDUCT, SECURITY, and valid issue templates. [About community profiles for public repositories](https://docs.github.com/en/communities/setting-up-your-project-for-healthy-contributions/about-community-profiles-for-public-repositories)

### Recommended repository set

```text
README.md
LICENSE
CONTRIBUTING.md
CODE_OF_CONDUCT.md
SECURITY.md
SUPPORT.md
.github/
  ISSUE_TEMPLATE/
    bug.yml
    feature.yml
    config.yml
  pull_request_template.md
  release.yml
  workflows/
    ci.yml
    release.yml
```

Add `CODEOWNERS` only when there are multiple people or teams with clear ownership boundaries. Add `.github/FUNDING.yml` only when a real funding destination exists.

### License decision

A public GitHub repository is not automatically open source. Without a license, default copyright rules apply and others generally may not reproduce, distribute, or create derivative works, apart from GitHub's platform permissions to view and fork. GitHub strongly encourages an explicit open-source license in a root `LICENSE` file. [Licensing a repository](https://docs.github.com/en/repositories/managing-your-repositorys-settings-and-features/customizing-your-repository/licensing-a-repository)

The maintainer must make the license choice; it should not be silently inferred. Practical options:

- **MIT** if the goal is a short, highly permissive license with attribution and warranty disclaimer. OSI lists it as approved and SPDX identifies it as `MIT`. [OSI MIT License](https://opensource.org/license/mit) [SPDX License List](https://spdx.org/licenses/)
- **Apache-2.0** if a permissive license with an express patent grant is preferred.
- **GPL-3.0-only** if redistributed derivative programs should remain under the same copyleft terms.

This is a product-governance choice, not merely a README decoration. If broad adoption and low contributor friction are the primary aims and there are no conflicting dependencies or employer/IP constraints, MIT is a reasonable default recommendation, but the owner should confirm it deliberately.

### `CONTRIBUTING.md`

GitHub automatically surfaces recognized contribution guidelines when people open issues or pull requests and adds a Contributing tab/link to the repository. [Setting guidelines for repository contributors](https://docs.github.com/en/communities/setting-up-your-project-for-healthy-contributions/setting-guidelines-for-repository-contributors)

Include:

- types of welcome contributions: bugs, testing, accessibility, docs, visual design, patterns, integrations, translations, and code;
- the exact supported development environment;
- exact restore/build/run/test commands copied from a clean-machine verification;
- how to run visual, DPI, multi-monitor, preview, and memory checks;
- coding and formatting conventions;
- requirements for screenshots/video when UI changes;
- the rule for discussing large design changes before implementation;
- how maintainers review and when a contribution may be declined;
- the expected PR scope: one coherent change, no unrelated cleanup;
- links to code of conduct, support, security, architecture, and roadmap.

Use a warm opening and make non-code work first-class. The GitHub-maintained Open Source Guides emphasize that specific suggestions and a welcoming tone reduce the barrier for newcomers. [Starting an Open Source Project](https://opensource.guide/starting-a-project/)

### `CODE_OF_CONDUCT.md`

Adopt a recognized template through GitHub, include a real private enforcement contact, define its scope, and only promise enforcement the maintainer can actually provide. GitHub describes a code of conduct as both a signal of an inclusive environment and a procedure for handling problems, and notes maintainers should consider whether they can enforce it. [Adding a code of conduct to your project](https://docs.github.com/en/communities/setting-up-your-project-for-healthy-contributions/adding-a-code-of-conduct-to-your-project)

### `SECURITY.md`

State:

- which release lines receive security fixes;
- what counts as a security issue;
- how to report privately;
- what information helps reproduce the issue;
- a realistic acknowledgment target;
- the coordinated-disclosure process;
- that public issues/discussions must not contain unpatched vulnerability details.

Enable GitHub **private vulnerability reporting** so researchers see a private “Report a vulnerability” path. [Adding a security policy](https://docs.github.com/en/code-security/how-tos/report-and-fix-vulnerabilities/configure-vulnerability-reporting/add-security-policy) [Configuring private vulnerability reporting](https://docs.github.com/en/code-security/how-tos/report-and-fix-vulnerabilities/configure-vulnerability-reporting/configure-for-a-repository)

### `SUPPORT.md`

Direct questions and setup help to Discussions; reserve Issues for reproducible bugs and scoped work. GitHub recognizes `SUPPORT.md` as the place to explain how users can get help. [Adding support resources to your project](https://docs.github.com/en/communities/setting-up-your-project-for-healthy-contributions/adding-support-resources-to-your-project)

### Funding

Add `.github/FUNDING.yml` only when the maintainer has a valid support destination and wants donations. GitHub can display GitHub Sponsors, supported platforms, or custom funding URLs. GitHub states these links are for direct financial support of open-source projects, not unrelated advertising. [Displaying a sponsor button](https://docs.github.com/en/repositories/managing-your-repositorys-settings-and-features/customizing-your-repository/displaying-a-sponsor-button-in-your-repository)

## 5. Contribution funnel and issue design

### Seed real beginner work before launch

Create 5–8 small, independent issues before announcing the repository. A useful beginner issue contains:

- why the work matters;
- current versus expected behavior;
- likely files or subsystem;
- acceptance criteria;
- exact test steps;
- screenshots or reference behavior when visual;
- a maintainer invitation to ask questions.

Apply `good first issue` only to work a newcomer can plausibly complete without undocumented architectural knowledge. GitHub uses this label to surface approachable issues in multiple locations, so accurate labeling can improve discovery. [Encouraging helpful contributions with labels](https://docs.github.com/en/communities/setting-up-your-project-for-healthy-contributions/encouraging-helpful-contributions-to-your-project-with-labels)

Use `help wanted` for useful, scoped work that may require more familiarity. Do not label a vague redesign or major refactor as beginner-friendly.

Potential first issues for this project, after validating scope:

- document a second monitor/DPI troubleshooting case;
- add keyboard focus visuals to an existing control;
- add a high-contrast preset;
- add a translation scaffold for UI strings;
- improve alt text and screenshots in docs;
- add a small, testable animation preset;
- add clean-machine build verification documentation.

### Structured issue forms

GitHub issue forms can require structured information and convert submissions into ordinary Markdown issues. They live in `.github/ISSUE_TEMPLATE`; GitHub currently documents forms as public preview. [Syntax for issue forms](https://docs.github.com/en/communities/using-templates-to-encourage-useful-issues-and-pull-requests/syntax-for-issue-forms)

**Bug form fields:**

- app version and download source;
- Windows edition/version/build and architecture;
- monitor count, resolutions, orientation, and scaling percentages;
- selected edge pattern, island mode, and relevant settings;
- reproduction steps;
- expected and actual results;
- whether live preview reproduces the issue;
- logs, screenshots, or screen recording;
- observed memory use if relevant;
- confirmation that secrets/private chat content were removed.

**Feature form fields:**

- problem or user outcome;
- proposed experience;
- alternatives/workarounds;
- accessibility implications;
- memory/CPU implications;
- whether the change affects edge light, island, settings, integration, or packaging;
- willingness to implement/test.

In `.github/ISSUE_TEMPLATE/config.yml`, disable blank public issues and add `contact_links` for Discussions/support and private vulnerability reporting. GitHub supports both behaviors. [Configuring issue templates](https://docs.github.com/en/communities/using-templates-to-encourage-useful-issues-and-pull-requests/configuring-issue-templates-for-your-repository)

### Pull request template

GitHub automatically inserts a recognized PR template into new PR bodies. [About issue and pull request templates](https://docs.github.com/en/communities/using-templates-to-encourage-useful-issues-and-pull-requests/about-issue-and-pull-request-templates)

Ask for:

- summary and motivation;
- linked issue using `Closes #123` where appropriate;
- tests run and their results;
- before/after visuals for UI changes;
- DPI and multi-monitor configurations tested;
- keyboard, screen-reader, contrast, and reduced-motion considerations;
- working-set measurement for rendering/lifecycle changes;
- confirmation that the PR contains no unrelated changes;
- confirmation that documentation/changelog entries were updated.

Linked PRs make work visible and can automatically close an issue when merged into the default branch. [Linking a pull request to an issue](https://docs.github.com/en/issues/tracking-your-work-with-issues/using-issues/linking-a-pull-request-to-an-issue)

### Protect the default branch

Require pull requests and passing CI before merging to `main`; prevent force pushes and deletion. Add review requirements when a second active maintainer exists. GitHub branch protection can require PR reviews, status checks, conversation resolution, signed commits, and a linear history. [About protected branches](https://docs.github.com/en/repositories/configuring-branches-and-merges-in-your-repository/managing-protected-branches/about-protected-branches)

Do not impose a review rule no one can satisfy while the project has one maintainer. Start with required CI and no force pushes, then strengthen governance as the maintainer pool grows.

## 6. Discussions and community boundaries

Enable GitHub Discussions for transparent conversations that do not yet need issue-style tracking. GitHub explicitly distinguishes Discussions from code-related, trackable Issues. [Quickstart for GitHub Discussions](https://docs.github.com/en/discussions/quickstart)

Recommended categories:

- **Announcements** — maintainer-only release/project news;
- **Q&A** — installation, integration, display setup, and troubleshooting questions;
- **Ideas** — early product concepts before they are scoped;
- **Show and tell** — presets, screenshots, setups, and integrations;
- **Themes and patterns** — community-created color/animation recipes;
- **General** — relevant conversation not covered elsewhere.

GitHub supports announcement, Q&A, poll, and open-ended category formats, with up to 25 categories. [Managing categories for Discussions](https://docs.github.com/en/discussions/managing-discussions-for-your-community/managing-categories-for-discussions)

Create and pin a welcome post that explains:

- which category to use;
- that reproducible bugs belong in Issues;
- that security reports must be private;
- how to search before posting;
- the code of conduct;
- how ideas graduate into scoped issues.

When publishing a Release, create its associated announcement discussion. GitHub supports this directly in the release flow. [Managing releases in a repository](https://docs.github.com/en/repositories/releasing-projects-on-github/managing-releases-in-a-repository)

## 7. GitHub Actions and release automation

### Workflow split

Use separate trust boundaries:

**`ci.yml`** on pull requests and pushes:

- run on a GitHub-hosted Windows runner;
- restore/build in Release mode;
- run unit/regression tests;
- run deterministic DPI/geometry/configuration tests that do not require an interactive desktop;
- archive diagnostic logs only on failure;
- keep `GITHUB_TOKEN` read-only.

**`release.yml`** on a protected version tag or manual dispatch:

- verify the tag/version relationship;
- build once in a clean GitHub-hosted Windows runner;
- run the complete automated suite;
- assemble predictably named assets;
- generate `SHA256SUMS.txt`;
- Authenticode-sign when a trusted signing service is available;
- generate a GitHub artifact attestation;
- create a draft release and attach assets;
- publish only after the release gate/check completes.

Use `.github/release.yml` to group automatically generated notes into Added, Improved, Fixed, Documentation, and Other. GitHub-generated release notes can include merged PRs, contributors, and a full changelog link, and labels can customize categories. [Automatically generated release notes](https://docs.github.com/en/repositories/releasing-projects-on-github/automatically-generated-release-notes)

### Workflow security

- grant the `GITHUB_TOKEN` only the minimum permissions required; CI should normally use `contents: read`, while only the release job needs `contents: write`, plus `id-token: write` and `attestations: write` when attesting;
- pin third-party actions to full-length commit SHAs;
- avoid `pull_request_target` unless its privileged semantics are truly necessary, and never combine it with checkout/execution of untrusted fork code;
- do not use a persistent self-hosted runner for arbitrary public PR code;
- place signing credentials behind an environment with required review when applicable;
- audit workflow logs for accidental secret exposure.

GitHub recommends least-privilege `GITHUB_TOKEN` permissions and full-SHA action pinning, warns about privileged triggers with untrusted code, and says self-hosted runners should almost never be used for public repositories because they can be persistently compromised. [Secure use reference](https://docs.github.com/en/actions/reference/security/secure-use) [Use `GITHUB_TOKEN` for authentication](https://docs.github.com/en/actions/tutorials/authenticate-with-github_token)

Enable Dependabot for GitHub Actions references so update PRs are proposed, while retaining human review of the new full SHA. GitHub supports version updates for Actions dependencies. [Dependabot version updates](https://docs.github.com/en/code-security/concepts/supply-chain-security/dependabot-version-updates)

## 8. Ethical promotion inside GitHub

GitHub has no legitimate “advertise this repository” switch and no guaranteed path to Trending. Sustainable promotion is earned through accurate presentation, useful releases, and genuine participation.

### Recommended GitHub-native promotion

1. Finish the README, community health files, release, and several good first issues before announcing.
2. Pin the project on the maintainer profile and add one concise profile-README entry.
3. Use accurate topics and a branded social preview.
4. Publish a release announcement in Discussions with a concise demo, changes, known limitations, and a request for specific feedback.
5. Ask users to report real display configurations and edge cases, not merely to star the project.
6. Participate helpfully in relevant communities; mention the project only where it directly solves the problem under discussion.
7. Thank and credit contributors in merged PRs and release notes.
8. Keep a small public roadmap and close or update stale items honestly.

The GitHub-maintained Open Source Guide recommends explaining what makes a project useful, meeting the audience where it already gathers, asking for specific feedback, helping others before asking for attention, and building relationships over time. [Finding Users for Your Project](https://opensource.guide/finding-users/)

### Explicitly avoid

- unsolicited promotion in unrelated repositories' issues, PRs, or Discussions;
- automated or purchased stars/follows;
- reciprocal-star schemes;
- giveaways tied to stars or follows;
- bulk mentions or promotional issue creation;
- misleading affiliation claims;
- repeatedly posting the same pitch without contributing to the conversation.

GitHub permits project-related promotional material in one's own README and description, but prohibits spam, bulk promotions, rank abuse, inauthentic activity, incentivized engagement, and advertising in other users' accounts. [GitHub Acceptable Use Policies](https://docs.github.com/en/site-policy/acceptable-use-policies/github-acceptable-use-policies)

### Measure quality, not vanity

Review GitHub Insights → Traffic after launch for the available 14-day window, focusing on:

- unique visitors and clones;
- referring sites;
- popular repository content;
- release asset downloads;
- issue completion rate;
- time to first maintainer response;
- first-time contributors who return.

Use the findings to improve the install path and documentation, not to manufacture engagement. GitHub documents repository traffic as a view of visitors, clones, referring sites, and popular content. [Viewing traffic to a repository](https://docs.github.com/en/repositories/viewing-activity-and-data-for-your-repository/viewing-traffic-to-a-repository)

## 9. Recommended launch sequence

### Phase 0 — legal and trust gate

- Confirm ownership of all source, logo, screenshots, fonts, and inspiration assets.
- Choose and add the open-source license deliberately.
- Scrub repository history, test artifacts, logs, settings, screenshots, and workflow output for secrets and private user content before making the repository public.
- Add the independence disclosure.
- Decide and document whether the first executable is signed.

### Phase 1 — repository foundation

- Add README and `docs/` content.
- Add community health files and issue/PR templates.
- Enable Discussions and private vulnerability reporting.
- Configure topics, description, website, and social preview.
- Add protected-branch/CI requirements.
- Seed 5–8 scoped issues, including 2–4 legitimate good first issues.

### Phase 2 — release candidate

- Build on a clean GitHub-hosted Windows runner.
- Run regression, DPI, multi-monitor geometry, tray lifecycle, live-preview stress, and memory tests.
- Generate the versioned executable, SHA-256 manifest, and artifact attestation.
- Test the exact downloaded release candidate on a clean Windows user account or VM.
- Draft complete release notes, known limitations, and verification instructions.

### Phase 3 — publish and announce

- Publish the immutable release.
- Pin the repository to the maintainer profile.
- Publish the release Discussion.
- Share the canonical repository/release URL only in genuinely relevant communities.
- Ask for concrete feedback: Windows build, monitor layout/scaling, selected pattern, and whether completion capture worked.

### Phase 4 — first 30 days

- Respond quickly and kindly to early reports.
- Convert repeated questions into docs.
- Turn validated ideas into small scoped issues.
- Merge and acknowledge early contributions promptly.
- Publish small, reliable follow-up releases rather than an oversized roadmap promise.
- Review Traffic and release downloads, then improve the highest-friction entry points.

## 10. Launch checklist

### Presentation

- [ ] Repository name and one-sentence description are accurate.
- [ ] 10–15 truthful topics are configured.
- [ ] 1280 × 640 social preview is uploaded.
- [ ] Repository is pinned to the maintainer profile.
- [ ] README shows the outcome, download, compatibility, and screenshot before deep technical detail.
- [ ] Every image has meaningful alt text and essential information is also text.
- [ ] Independence/non-affiliation language is visible.

### User trust

- [ ] Root `LICENSE` is deliberately selected.
- [ ] Release assets use versioned, predictable names.
- [ ] `SHA256SUMS.txt` is attached.
- [ ] Artifact attestation is generated.
- [ ] Immutable releases are enabled.
- [ ] Signed/unsigned and SmartScreen behavior are disclosed honestly.
- [ ] Privacy/data-flow claims have been checked against the code.

### Contributors

- [ ] `CONTRIBUTING.md` contains clean-machine build and test steps.
- [ ] `CODE_OF_CONDUCT.md`, `SECURITY.md`, and `SUPPORT.md` contain real monitored contacts/routes.
- [ ] Bug and feature issue forms are valid.
- [ ] PR template asks for tests, visuals, DPI/multi-monitor coverage, accessibility, and memory impact.
- [ ] At least two genuinely bounded `good first issue` items exist.
- [ ] Discussions categories and a pinned welcome post exist.

### Automation and governance

- [ ] CI passes on the default branch and PRs.
- [ ] Release workflow uses least privilege.
- [ ] Third-party Actions are pinned to full commit SHAs.
- [ ] Untrusted public PR code does not run on a persistent self-hosted runner.
- [ ] Default branch blocks force pushes and requires CI.
- [ ] Release notes are categorized and contributors are credited.

## Bottom line

The ideal GitHub page for Codex Edge Glow should make three questions effortless to answer:

1. **Will this solve my problem?** Show the completion glow and island immediately.
2. **Can I trust and install it?** Provide a verifiable, preferably signed, immutable release with clear Windows expectations.
3. **Can I help?** Offer a warm, precise contribution path with real beginner-sized work.

GitHub discoverability compounds when the repository is genuinely useful and easy to evaluate. Accurate topics and profile placement help people find it; accessible documentation helps them understand it; safe releases help them try it; responsive maintenance gives them a reason to stay.
