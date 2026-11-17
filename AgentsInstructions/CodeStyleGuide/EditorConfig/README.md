# Editor configuration

Merge `EditorConfig/.editorconfig` from this kit into the target project's root `.editorconfig`. It encodes the enforceable
subset of [the style policy](../UnityStyleGuide.md); ownership, serialized migrations,
member order, and architectural choices still require review. C# uses four spaces; UXML/USS two.

## Naming analyzers

Modern Roslyn chooses naming rules by specificity, not declaration order. Constants are also
static, so the constant group explicitly requires both modifiers (`const, static`). It outranks the
static and plain field groups. Instance fields use `_camelCase`; private/internal statics `s_camelCase`;
constants at any accessibility use PascalCase; properties PascalCase; interfaces an I prefix.
Exceptions for external data contracts may need a scoped analyzer suppression.

Severities here are warnings, and IDE1006 is pinned to `warning` so naming violations also surface
in `dotnet build`/CI rather than only as IDE suggestions. Unity compilation does not automatically enforce
Roslyn IDE naming diagnostics, and setting a naming severity alone does not make every build enforce
it. If CI enforcement is selected, install a compatible analyzer/formatter runner, and verify
representative intersections. Use
`python3 CodeStyleGuide/Tools~/check_naming.py` in this kit checkout for good/bad fixtures against the installed
.NET SDK's Roslyn through `dotnet format`; record its version in [Validation](../../../Validation.md).
This is separate from Unity CLI compilation/testing, which verifies Unity code and dependencies.

Unity serialized formats, including `.preset`, are excluded from whitespace rewrites. Binary files
also retain their original bytes. These settings guide compatible formatters; still scope formatting
to owned source and inspect the diff. Do not run a general text formatter over Unity assets.

## Git attributes

The starter `.gitignore` and `.gitattributes` now live with their installer in the
[version control guide](../../../AuditInstructions/VersionControl/README.md), including LFS routing and
scene-merge setup.
