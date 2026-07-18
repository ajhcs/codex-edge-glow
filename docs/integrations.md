# Codex integration

## External notification contract

Codex CLI supports a top-level `notify` command array in `config.toml`. At the end of an agent turn, Codex starts that command and appends a JSON payload as the final argument. The upstream Codex source documents the same command-array model and `agent-turn-complete` event.

Codex Edge Glow reads these payload fields when present:

| Field | Use |
| --- | --- |
| `type` | Completion event identity; unknown payloads still receive a generic preview |
| `last-assistant-message` or `last_assistant_message` | Island result excerpt |
| `cwd` | Optional task folder label and quick-reply working directory |
| `thread-id` or `thread_id` | Quick-reply routing ID |

Example direct configuration:

```toml
notify = ["C:\\Tools\\CodexEdgeGlow\\codex-edge-glow.exe"]
```

Codex invokes the equivalent of:

```text
codex-edge-glow.exe {"type":"agent-turn-complete", ...}
```

## Preserve another notifier

Codex currently exposes one external notification command array. Codex Edge Glow can run an existing notifier after processing the same event:

```toml
notify = [
  "C:\\Tools\\CodexEdgeGlow\\codex-edge-glow.exe",
  "--chain",
  "C:\\Tools\\ExistingNotifier\\notify.exe",
  "--existing-option"
]
```

Everything after the chained executable, including the final Codex payload, is forwarded using Windows-safe argument quoting. Test the existing notifier after changing the chain. If it expects a shell pipeline or unusual environment, use a small wrapper script rather than placing shell syntax in the TOML array.

## Quick reply

Quick reply appears only when all of the following are true:

1. the island is enabled;
2. quick reply is enabled in settings;
3. the notification payload contains a task/thread ID;
4. a local Codex CLI executable is available.

The app looks for the executable in this order:

1. the `CODEX_CLI_PATH` environment variable;
2. the newest `codex.exe` under `%LOCALAPPDATA%\OpenAI\Codex\bin`;
3. `codex.exe` on `PATH`.

It starts:

```text
codex exec resume --skip-git-repo-check THREAD_ID -
```

and writes the reply to standard input. The app itself does not call a remote API; the launched Codex CLI follows its own authentication, network, and approval configuration.

## Host compatibility

The external `notify` hook is part of Codex CLI configuration. Codex Desktop, IDE extensions, and other app-server hosts may not invoke it consistently in every version. If manual preview works but automatic completion does not, verify the behavior in Codex CLI before reporting a rendering bug.

Upstream reference: [OpenAI Codex configuration source](https://github.com/openai/codex/blob/main/codex-rs/core/src/config/mod.rs).

