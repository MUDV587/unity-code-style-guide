# Project configuration

This is the single configuration record to adapt in the target project. Actual project files are
evidence; this document records the intended choices and reasons, not a second package lockfile.

## Versions

- Intended starter line: **Unity 2022.3 LTS** (2022.3.x). An intended line is not a tested release claim.
- Exact target Editor: **fill in from `ProjectSettings/ProjectVersion.txt`**.
- Language/API baseline: **C# 9 / .NET Standard 2.1**, verified against the selected release.
- Starter validation results and prerelease limits: [Validation.md](../../Validation.md).

## Selected systems

| Area | Baseline | Change only when selected |
|---|---|---|
| Rendering | URP matched to the chosen Editor/template | Built-in or HDRP |
| Input | Input System | Legacy Input Manager or deliberate combined handling |
| Runtime UI | UI Toolkit, UXML/USS | uGUI or another runtime UI system |
| Async | Coroutines for simple delays/sequencing; typed async paths (e.g. `Addressables.Handle.Task`) otherwise. **Awaitable does not exist on 2022.3** | Package-specific Task/coroutine integration |
| Pooling | UnityEngine.Pool.ObjectPool<T> | Specialized pool with a demonstrated requirement |

Editor IMGUI remains an available Editor-tooling option; runtime UI selection does not ban it.
Unity 2022.3 has no `Awaitable` — use coroutines for delays and sequences, and manual refresh in
views for data changes. `[AutoStaticsCleanup]` is a Unity 6.6+ attribute; on 2022.3 reset static
state with `[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]`.
Pool capacity reserves storage rather than prewarming objects.
Use matching skills for implementations instead of loading audit references during setup.

## Resolved packages

Read `Packages/manifest.json`, `Packages/packages-lock.json`, and Package Manager's completed resolved
list through Unity CLI. Record versions here when needed for compatibility decisions; retain the
lockfile as the authoritative dependency graph. Install only selected packages.

| Capability | Package | Exact resolved version in target |
|---|---|---|
| Rendering | `com.unity.render-pipelines.universal` | Fill in; do not assume a URP/Editor pairing |
| Input | `com.unity.inputsystem` | Fill in |
| Tests, when selected | `com.unity.test-framework` | Fill in or remove |
| Addressables, when selected | `com.unity.addressables` | Fill in or remove |
| Networking, when selected | `com.unity.netcode.gameobjects` | Fill in or remove |

## Target decisions

| Decision | Target value |
|---|---|
| Project namespace | Fill in |
| Desktop platforms / backends | Windows x64 and macOS Apple Silicon; record primary platform and backend per target |
| Additional architectures/platforms | Intel macOS only when selected; others outside the desktop baseline |
| Desktop support and profiles | Fill in minimum OS/GPU/hardware and selected API order; follow [desktop setup](../../AuditInstructions/BuildSettings/DesktopBuilds.md) or [mobile setup](../../AuditInstructions/BuildSettings/MobileBuilds.md) |
| Frame/memory budget | Record when established for a representative workload |
| Enter Play Mode reload | Scene reload enabled; domain reload off (common new-project default) with static state reset via `[RuntimeInitializeOnLoadMethod(SubsystemRegistration)]`; see [project settings](../../AuditInstructions/ProjectSettings/ProjectSettings.md#reload-policy) |
| Assembly policy | Default assemblies; add boundaries for concrete needs; isolate tests/Editor code |
| Optional presets/templates/examples | List selected groups |
| Optional scene templates | Strategy Sandbox, UI Preview, Asset LookDev; select installed groups and record target validation |

## API baseline notes (Unity 2022.3)

Use version-matched source/API docs and compiler diagnostics. Corrections relevant to this line:

| Topic | Unity 2022.3 baseline |
|---|---|
| `FindObjectsOfType` / `FindObjectsOfType<T>()` | Still the supported API; no replacement needed. `FindObjectsByType<T>(FindObjectsSortMode)` is available (2020.1+) but unsorted variants only; there is no obsolete warning on 2022.3 |
| Rigidbody `velocity`, `drag`, `angularDrag` | Current API; `linearVelocity`/`linearDamping` are Unity 6 renames — do not use them on 2022.3 |
| Graphics DrawMesh family | `Graphics.DrawMesh`/`DrawMeshInstanced` are the supported calls; the Unity 6 RenderMesh migration does not apply |
| UXML custom elements | Register with `UxmlFactory` / `UxmlTraits`; `[UxmlElement]`/`[UxmlAttribute]` do not exist on 2022.3 and will not compile |
| Runtime data binding | Not available (`dataSource` / `binding-path` / `[CreateProperty]` are Unity 6-only). Populate elements in code and refresh manually, or use `SerializedObject.Bind()` for Editor-only data |
| Render Graph (SRP) | Not in 2022.3 URP (URP 14); URP Render Graph lands with URP 15 (Unity 6-era). 2022.3 render passes use the classic ScriptableRenderPass model |
| `Awaitable` | Does not exist — coroutines; `Animator` triggered APIs and `ObjectPool<T>` are unchanged |
| `Object.FindFirstObjectByType` / `FindAnyObjectByType` | Present on 2022.3 (2021.2+) if you need them; `FindObjectsOfType` remains the documented default |

For the Unity 6 API line (`Awaitable`, `[UxmlElement]`, runtime data binding, Render Graph,
`linearVelocity`), see the `Unity6000` branch of this fork. Do not introduce Unity 6-only APIs here;
see the optional [migration reference](../../AuditInstructions/CoreCLRmigration/CoreCLRUpgrade.md).

## CLI/manual completion

**Prerequisite:** exact Editor, project, and selected platform are known. Use `unity-cli` to inspect
versions/settings, and `unity-package-management` for supported Client API execution. Manual fallback:
Hub + Package Manager + Project Settings. **Done:** exact versions resolve, the selected pipeline/input
actually run, configuration agrees with saved files, and any mismatch is resolved or reported.
