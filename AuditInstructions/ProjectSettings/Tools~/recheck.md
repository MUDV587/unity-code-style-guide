# Re-check checklist for ProjectSettings.md

Maintainer notes, not an agent document (Unity ignores this `Tools~` folder). Run this at least
every six months, and before relying on any "default" printed in the guide.
`python3 AuditInstructions/Tools/validate_all.py` warns when a "Last verified" date is older
than that.

After re-checking, update the **Last verified** line in `ProjectSettings.md` and the CHANGELOG.

## Claims that rest on thin evidence (2022.3 fork)

These rows were written against the Unity 6.6/6.7 manual. They were adapted for 2022.3 but never
re-verified — check each against the installed 2022.3 Editor.

| Claim in `ProjectSettings.md` | Why it is weak | Source to re-read |
|---|---|---|
| Domain reload default in new projects | Default differs by Editor version and template; no default is printed in 2022.3 docs | Project Settings > Editor > Enter Play Mode Settings on a fresh project |
| Existing projects keep their saved Enter Play Mode value | Not stated anywhere | Compare `ProjectSettings/EditorSettings.asset` across a saved before/after Editor pass |
| Dropdown option to `EnterPlayModeOptions` flag mapping | The manual does not map them | [EnterPlayModeOptions](https://docs.unity3d.com/2022.3/Documentation/ScriptReference/EditorSettings-enterPlayModeOptions.html); set via API, read the dropdown |
| Templates ship the same Enter Play Mode value | No page says | Create a 3D (URP) project and read the value |
| Burst package included in new projects | Manifest-dependent, not Editor-guaranteed | The resolved `Packages/packages-lock.json` of a fresh project |
| Active Input Handling default and the legacy deprecation | Options listed; default unstated | Player settings on a fresh project; Input System installation docs |
| API Compatibility Level default | Options listed; default unstated | Player settings on a fresh project |
| `[AutoStaticsCleanup]` guidance | Attribute not present on 2022.3 — text is a Unity 6.5+ note only | Unity 6 manuals, on the `Unity6000` branch |
| Visual Scripting/CoreCLR symbols | No 2022.3 relevance | The Unity 6 manual, on the `Unity6000` branch |
| Project Auditor | Package availability not verified | The package registry for the installed Editor |

## Claims to re-confirm on every release

- Force Text and Visible Meta Files are the defaults (Editor settings reference).
- URP package version matches the Editor (core packages list).
- The Built-in pipeline status you assume (not deprecated on 2022.3 — check before telling a user to migrate).
- Dynamic batching and SRP Batcher status for the pipeline in use.
- The reload dropdown labels and the default option.

## When audited against a newer Editor

1. Note that the `Unity6000` line of this fork tracks Unity 6; the Unity 6-specific rows (CoreCLR,
   `AutoStaticsCleanup`, managed code variants) are documented there.
2. Confirm nothing in this file references a Unity 6-only default.
3. Update the readme's "Good to know" version note.
