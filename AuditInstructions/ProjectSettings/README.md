> How it works: give the URL of [ProjectSettings.md](ProjectSettings.md) to your agent and it will
> audit your Unity project's core settings, recommend changes, and apply only the ones you pick
> and confirm.

# Project settings

A short list of settings that are cheap to get right at the start and expensive to fix later. The
agent document works on any Unity 2022.3 project, new or established, and it reviews before it changes
anything.

| I want to… | Give your agent |
|---|---|
| Check or set up my project's core settings | [ProjectSettings.md](ProjectSettings.md) |
| Set up Windows/macOS or Android/iOS player and build settings | [BuildSettings](../BuildSettings/README.md) |

Example prompts:

- *"Review my Unity project's settings against this document."* You get a table of what matches and
  what differs, plus numbered recommendations.
- *"Review them, then apply the changes I pick."*
- *"Check how my project handles domain reload and fix the static state."*

## What happens when you hand it over

1. **Audit.** The agent reads your settings and writes nothing.
2. **Recommendations.** Each difference becomes a numbered item with the reason, the risk, and a default.
3. **You pick.** Take any subset, "all recommended", or none; for example, only the domain-reload fix.
4. **You confirm.** The agent restates the exact changes and waits for explicit user confirmation.
5. **Apply and verify.** It changes only what you confirmed and reports the recommendations you skipped.

## What it covers

| Area | The recommendation | Why |
|---|---|---|
| Asset serialization | Force Text | Scenes and prefabs diff and merge as text |
| Meta files | Visible, committed with their assets | A `.meta` file holds an asset's GUID; losing it breaks references |
| Input | Input System package | Unity documents the legacy Input Manager as deprecated |
| API compatibility | .NET Standard 2.1 | Broadest platform support; matches the C# 9 baseline |
| Render pipeline | URP, assigned in every quality level | The Built-in pipeline is deprecated |
| Color space | Linear | Correct lighting for modern pipelines |
| Enter Play Mode | Domain reload off, scene reload on | Fast iteration, as Unity now defaults to; needs static state reset |
| Packages | Manifest and lockfile committed | Everyone resolves the same versions |
| Tags and layers | Add only names the game uses | Slots are numbered; reusing one silently breaks collisions |

Platform choices (graphics APIs, scripting backend, quality tiers) live in the
[build guides](../BuildSettings/README.md), not here.

## The big one: domain reload

When you press Play, Unity can reload your scripts (a *domain reload*) and reload the scene.
Reloading scripts resets every `static` variable, so it hides bugs but costs seconds on every Play.

- **Unity's defaults changed at some point.** Recent new projects can enter Play mode without
  reloading the domain. The default differs by Editor version and template — read the actual
  dropdown rather than assume. (Unity 6.6 changed the documented new-project default to
  **Reload Scene only**; 2022.3 may differ.)
- **What breaks.** With the domain left alone, static fields keep their values from the last Play,
  and static events keep their old subscribers. A counter that should restart at zero doesn't, and
  an event can fire twice. The game works in a build and misbehaves in the Editor, or the reverse.
- **Choose reset or persistence deliberately.** In Unity 6.5 and later, `[AutoStaticsCleanup]`
  resets supported static state; its containing type must be `partial` for generated cleanup.
  `[NoAutoStaticsCleanup]` opts out: use it only for state intentionally preserved between Plays,
  such as IDs that must remain unique across sessions. A gameplay counter that should restart needs
  a reset. On 2022.3, which has no such attribute, use an explicit
  `[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]` reset.
- **What the agent does.** It reads the saved policy and audits mutable static state before proposing
  a change. Existing projects keep their observed setting during review; for new projects the default
  depends on the Editor — verify instead of assuming. Unprotected state can justify a separately confirmed
  **Reload Domain and Scene** stopgap, with the reason recorded.
- **Leave scene reload on.** Turning it off too means `OnDestroy` is skipped and your code has to
  restore object state itself.

An existing project keeps whatever it saved before, so the agent reads the real value first.

## Why the guide looks the way it does

- **Review first.** It starts read-only and shows a table. Nothing changes until you pick items and confirm.
- **Unity's own APIs.** It changes settings through the Editor, never by editing project files by hand.
- **Keep what works.** An existing choice that works is left alone; the guide reports conflicts
  instead of migrating you.
- **Honest about gaps.** Anything it can't check is marked *Not verified*.

## Good to know

- **Version.** This branch (`Unity2022`) targets Unity 2022.3 LTS; the 2022.3-specific adaptations
  are not re-verified, so version-sensitive defaults and APIs should be checked against your installed
  Editor. (The upstream document references the Unity 6.6/6.7 defaults, which no longer apply here.)
- **Preferences are personal.** Auto Refresh, worker count, and what happens when you edit scripts
  during Play are Editor preferences on your machine, not project settings. The agent recommends
  them but doesn't change them.
- **Other tools.** The Project Auditor package can check a project more broadly. It is optional, and
  this guide has not verified what it checks.
