> How it works: give this URL to your agent, for example "Review my Unity project's settings
> against this document". The agent audits first and recommends changes; you pick which to carry
> over, and nothing changes until you confirm. Works on any Unity 2022.3 project. Human overview:
> [readme](README.md).

# Project settings review and setup (agent instructions)

Check a Unity project's core settings against a safe starting policy, report the gaps, and, only
when the user approves, apply them. This document is self-contained. Values are starting choices,
not performance claims: keep a setting that already works unless the user selects the change.
**Last verified: October 2026**, against the Unity 6.7 beta manual (upstream). This branch (`Unity2022`) retargets the guide to **Unity 2022.3 LTS**; version-sensitive rows are adapted but not re-verified — check them against your installed Editor.

## Operating rules

- Start in **review** mode (stages 1 and 2 below): read and report only. Apply nothing until stage 4
  ends in explicit user confirmation.
- Change settings through Editor APIs or Unity CLI (`EditorSettings`, `PlayerSettings`,
  `QualitySettings`, `GraphicsSettings`, `SerializedObject`) or the Project Settings window. Never
  hand-edit `ProjectSettings/*.asset` or `.meta` files.
- Read every value before changing it and read it back after saving. Confirm each API name against
  the installed Editor; those below are the 2022.3 shape.
- Continue independent read-only audit with existing edits preserved. Establish an attributable
  baseline or isolated checkout before confirmed changes; resolve conflicting edits before execution.
- Preserve occupied layer, tag, and sorting-layer slots, existing quality entries, and assigned
  pipeline assets. Report a policy conflict instead of migrating outside the user's scope.
- Treat Preferences (Auto Refresh, worker count, script-change behavior) as machine-local; recommend,
  never change another person's workstation.
- Mark any check you cannot run **Not verified**.

## Review, recommend, confirm

This document always runs in four stages. Nothing is written to the project until stage 4 ends in an
explicit user confirmation.

1. **Audit.** Read the project and report what already matches this document, what differs, and what
   could not be checked. Read-only.
2. **Recommend.** List every difference as a numbered recommendation: ID, finding, proposed change,
   why it matters, risk (low, medium, high), and a default (do or skip). List items that already
   match as "no change needed". Add recommendations of your own from what you found, including ones
   this document does not mention.
3. **Choose.** The user picks any subset of IDs, "all recommended", or none. Carry over only the
   picked items; unpicked items stay proposals in the report.
4. **Confirm.** Present the exact changes for the picked subset (files, settings, values, commands)
   and obtain explicit user authorization before making them, including saving the report.
   Confirmation need not contain the literal word "yes": after the concrete scope is presented,
   "implement R1 and R3" can both select and confirm those items. Selection alone ("R1 and R3")
   chooses scope without authorizing implementation. Honor existing authorization for unchanged
   scope without asking again; clarify a delayed reply only when its scope is materially ambiguous.
   Resolve IDs against the presented change list rather than guide defaults. If evidence establishes
   that exact scope was previously presented and authorized but its record is now unavailable,
   retrieve that record before execution; recovered unchanged scope needs no repeat confirmation.
   If concrete changes were never presented, an instruction to implement an ID leaves scope
   unconfirmed: first present the exact changes and obtain confirmation before execution.
   Changed scope needs confirmation for the changed items. In a non-interactive session, or when
   authorization is absent, stop after stage 2 and change nothing.

Steps below that apply, install, or build run only after stage 4, and only for confirmed items. A new
finding during those steps becomes a new recommendation and returns to stage 3.

## 1. Preflight

Record (this is the audit input), in one table (setting, current value):

- Editor version from `ProjectSettings/ProjectVersion.txt`. This document targets Unity 2022.3 LTS.
  On a different Editor line, report mismatches and apply nothing.
- Render pipeline, the URP package version, and the installed Input System package.
- The current value of every row in step 3, and the Enter Play Mode setting in Project Settings >
  Editor > Enter Play Mode Settings > **When entering Play Mode**.
- Source-control state, and whether the Editor is open.
- Mutable static state and static events in owned code (include mutable contents of `static readonly`
  collections as well as assignable fields; immutable constants and cached hashes need no reset), for the reload decision below. For a deeper,
  evidence-based audit, use the "Session reset" check of the
  [Scenes and lifecycle lens](../ProjectAuditGuides/UnityScenesAndLifecycleInstructions.md).

**Done:** the table exists, every row has a known value, an observed "not set", or Not verified with a reason,
and nothing was changed.

## 2. Ask

Ask in one batch. When the user says "use defaults", the default becomes a recommended answer that
still goes through stages 2 to 4.

| Question | Default |
|---|---|
| Stop after the recommendations, or choose items to apply? | Stop after the recommendations; the user chooses in stage 3 |
| Which input system, and which render pipeline? | Input System package and URP, unless the project already uses another |
| Reload policy (see the next section) | Keep the observed setting while auditing; for a new project, consider that Editor's default after reviewing static state |
| Source control in use? | Git, with visible meta files |
| Which platforms ship? | Ask; the build guides own platform settings |

**Done:** the mode and choices are repeated back, one line each.

## 3. Review and apply

Apply only the items confirmed in stage 4; every other row is reported. Mark each row **OK**,
**Differs** (report, and fix only if confirmed), **Not set** (observed absent), or **Not verified**
(value/evidence unavailable). An unknown value is neither an unset value nor a policy difference.

| Setting | Target | Read / set via | Condition |
|---|---|---|---|
| Asset Serialization Mode | Force Text | `EditorSettings.serializationMode` | A common default; makes diffs and merges readable |
| Version Control Mode | Visible Meta Files | `VersionControlSettings.mode` | A common default. Commit every `.meta` with its asset |
| Source control hygiene | `Library/`, `Temp/`, `Obj/`, `Logs/`, `UserSettings/` ignored; `Assets`, `Packages/manifest.json`, `Packages/packages-lock.json`, `ProjectSettings` committed | `.gitignore` ([version control guide](../VersionControl/README.md)) | Do not ignore `.meta` files |
| Input handling | Input System Package (New) | `Active Input Handling` in Player Settings; no public property, so use `SerializedObject` on `ProjectSettings.asset` or the window | Changing it needs an Editor restart. The legacy Input Manager is deprecated in practice. Choose Both only for a project mid-migration |
| API Compatibility Level | .NET Standard 2.1 | `PlayerSettings.SetApiCompatibilityLevel(NamedBuildTarget, …)` | .NET Framework only when a dependency requires it |
| Render pipeline | URP asset assigned as the default and in every quality level | `GraphicsSettings.defaultRenderPipeline`, `QualitySettings.renderPipeline` per level | Keep a working HDRP or Built-in project as it is; URP 14 ships with 2022.3 |
| Color space | Linear | `PlayerSettings.colorSpace` | Gamma only for a deliberate legacy-device target |
| Enter Play Mode | See "Reload policy" | `EditorSettings.enterPlayModeOptionsEnabled`, `enterPlayModeOptions` | Verify against the dropdown after setting |
| Burst and jobs | Use through the installed API | Read the resolved package list | `com.unity.burst` is included in typical 2022.3 manifests; new projects get it. Do not assume — read the resolved list |
| Batching | Rely on the SRP Batcher | Pipeline settings | Check SRP Batcher compatibility for the pipeline in use |
| Compile-time symbols | Avoid `UNITY_64` and `DEVELOPMENT_BUILD` in new code | Search owned code | Legacy symbols; Unity 6 deprecates them for Managed Code Variant. Harmless on 2022.3 |
| Packages | Manifest and lockfile committed; selected packages only | Package Manager Client API | Read the completed resolved list; never float versions |
| Tags, layers, sorting layers | Add only names the game uses | `TagManager` through `SerializedObject` | Never reuse or renumber an occupied slot |
| Scenes in the build | Only real scenes, enabled | `EditorBuildSettings.scenes` or Build Profiles | |
| Product and company name | Final values before any shipping build | `PlayerSettings.productName`, `companyName` | They set `persistentDataPath`; see the build guides |

Platform architecture, graphics APIs, scripting backend, stripping, and quality tiers belong to the
build guides ([desktop](../BuildSettings/DesktopBuilds.md), [mobile](../BuildSettings/MobileBuilds.md)).
Do not duplicate them here.

### Reload policy

**What it means and why it matters.** On entering Play mode the Editor does not have to reload the
scripting domain (it re-compiles) or the scene. Skipping either speeds up iteration; skipping the
domain reload means static fields and static event subscribers survive between Play sessions, so
session state must be reset in code. The default differs by Editor version and template — read the
actual dropdown instead of assuming (Unity 6.6 changed the documented new-project default to
**Reload Scene only**; earlier versions, including 2022.3, may keep **Reload Domain and Scene** —
verify on the installed Editor).

**The dropdown** (Project Settings > Editor > Enter Play Mode Settings > **When entering Play
Mode**): Reload Domain and Scene · Reload Scene only · Reload Domain only ·
Do not reload Domain or Scene. Scripts set it with `EditorSettings.enterPlayModeOptionsEnabled` and
the `EnterPlayModeOptions` flags (`DisableDomainReload`, `DisableSceneReload`, `None`); the manual does
not map the dropdown to those flags, so set it, then read the dropdown back.

**What domain reload off means.** Static fields keep their values between Play sessions, static
events keep their subscribers, and non-serialized fields keep values from the last session. State
must be reset in code.

**The policy**

1. For a new 2022.3 project, decide the reload policy after the static-state audit, not from a
   default printed in any document — the safe starting policy depends on the Editor/template and the
   project's static state. For an existing project, retain the observed saved policy during audit;
   propose a change only after evaluating reset behavior. Scene reload off changes
   teardown/restoration behavior and requires a deliberate project contract.
2. Audit mutable statics and static events (step 1, or the lens above for a full audit). On Unity 6.5+
   the `[AutoStaticsCleanup]` attribute from `Unity.Scripting.LifecycleManagement` exists (events,
   delegates, collections of user objects, Unity object references); on **2022.3 — the target of this
   branch — it does not exist**. `[NoAutoStaticsCleanup]` opts out of reset for intentional
   cross-session persistence. Types containing auto-cleaned fields must be `partial`, and `readonly`
   collections retain their instance and are cleared. The analyzer reports `UAL0010` to `UAL0014`.
3. Code that runs in Edit mode must reset on entering Play mode, because exit-time cleanup cannot be
   relied on there. On 2022.3, reset with a `[RuntimeInitializeOnLoadMethod(
   RuntimeInitializeLoadType.SubsystemRegistration)]` method and unsubscribe static handlers.
4. If the audit finds many unprotected statics, set **Reload Domain and Scene** as a stopgap and
   record the follow-up. Say so in the report; do not leave it silent.

**Verify.** Press Play twice with a static counter and a static event subscriber: the second run
starts from the initial state, with no duplicate handler calls.

**Done:** the audit and numbered recommendations were shown and the user's explicit confirmation was received
before any change; every row has a status, the reload policy is recorded with the audit result, and
re-reading the settings returns the target values.

## 4. Optional choices

Offer these as recommendations; apply one only if it is picked and confirmed:

| Option | Choose when | Check |
|---|---|---|
| Reload Domain and Scene | Legacy code depends on a full reset | Two Plays are clean; accept the slower enter-Play time |
| Disable scene reload | The project deliberately restores object state | Repeated Plays and enable/disable tests pass |
| asmdefs | A concrete compile, dependency, or test boundary exists | Editor and player compile; tests are discovered in isolated assemblies |
| Quality tiers and volumes | Target hardware needs distinct budgets | Each tier runs on a representative device |
| Project Auditor | A broader review is wanted | Install `com.unity.project-auditor` through Package Manager; what it checks is not verified here |

## 5. Verify

- Read back every changed value. Reopen a scene and a prefab to confirm saved references survive.
- The project compiles in the Editor, and the project's tests pass.
- Press Play twice and confirm the reload contract above.
- An agreed platform build completes; otherwise mark it Not verified.

**Done:** each check is Passed, Failed, or Not verified.

## 6. Report

| Item | Status | Detail |
|---|---|---|
| Each step 3 row | OK / Differs / Applied / Not set / Not verified | Old value → new value |
| Reload policy | Kept / Stopgap | Audit result and follow-up |
| Each unpicked recommendation | Proposal | Finding, proposed change, and risk |
| Each step 5 check | Passed / Failed / Not verified | Evidence |

Put decisions you need, unpicked recommendations, conflicts, and failed checks in a "Needs your
decision" block at the top.

## References

- [Configuring how Unity enters Play mode](https://docs.unity3d.com/2022.3/Documentation/Manual/configurable-enter-play-mode.html)
- [Enter Play mode without domain reload](https://docs.unity3d.com/2022.3/Documentation/Manual/domain-reloading.html)
- [Editor settings reference](https://docs.unity3d.com/2022.3/Documentation/Manual/class-EditorManager.html)
- [EditorSettings.enterPlayModeOptions](https://docs.unity3d.com/2022.3/Documentation/ScriptReference/EditorSettings-enterPlayModeOptions.html)
- [Input System installation](https://docs.unity.com/Packages/com.unity.inputsystem@latest/index.html?subfolder=/manual/Installation.html)

Links follow current documentation — select the installed Editor/package version before applying an API or default. The Unity 6 references (what's new in 6.6, the 6.6 upgrade guide, CoreCLR backend) live on the `Unity6000` branch of this fork.
