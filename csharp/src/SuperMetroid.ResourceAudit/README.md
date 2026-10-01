# Static resource dependency audit

Issue #1155: find undeclared installed-resource dependencies from source and immutable
definition catalogs, without searching for failures by playing or replaying the game.
This tool is development-only and is not referenced or shipped by either playable host.

## Run

From the repository root, with the .NET 10 SDK:

```powershell
dotnet run --project csharp/src/SuperMetroid.ResourceAudit -c Release -- --root . --json out/resource-audit.json
```

`dotnet run` rebuilds the auditor and its Core/importer references. The source root must
match those compiled assemblies; do not point an old auditor binary at a different checkout.
The SDK's own Roslyn compiler provides semantic binding; no parser package is downloaded.

To make findings fail an explicit build:

```powershell
dotnet build csharp/src/SuperMetroid.ResourceAudit -c Release -p:RunResourceAudit=true
```

Normal solution builds compile the tool but do not run it. Existing missing definitions and
unresolved domains are not silently baselined or suppressed to make a build gate green.
The separate **Static resource audit** GitHub workflow is manually runnable and uploads the
complete JSON even when the audit fails; it does not block release packaging.

Exit codes: `0` means no findings within the declared audit scope; `1` means missing
definitions or unresolved analysis; `2` means an auditor/configuration failure, with a
full exception on stderr. Windows entry-point failures cannot open a CLR error dialog.

## What is compared

- Every `CompiledEnemyVisualSelectors` record is checked against ordinary or extended
  enemy display exports. Requirements come from the selector table, not the export list.
- Bank-$86 projectile operands follow the production routing: installed operand-bound
  frames first, otherwise direct bank-$8D sprite compositions. Skree/Metaree's named
  direct bindings and the explicit zero-part blank sprite are accounted for.
- Source-declared `PresentationWordCount` program catalogs are discovered semantically.
  Their bounded operand-address declarations must have compiled selectors or projectile
  resources. A new program with unknown ownership/kind is unresolved, not silently skipped.
- Finite room/title palette-FX color declarations are compared with identities installed
  by the production extractors and loaders. Extractors receive a recording source with
  zero RGB5 values, not a ROM; only declared color reads are allowed. Installed aliases
  and the separate title provider count. Mechanics, timers and audio are never executed.
- Core resource-catalog instance operations, including internal draw methods, are inventoried
  through Roslyn. Named arguments, constant casts and symbols resolve semantically.
  Supported constant bank/pointer lookups are checked; dynamic IDs and unknown catalog
  domains are retained as explicit unresolved findings with their source/arguments.

Samus projectile exports are inventoried, but dynamic projectile-to-artwork mappings are
not certified merely because an exported sprite exists. There is no claim of whole-game
parity, resource binding correctness, pixel correctness or player validation.

## Findings and limits

`SMRA001` is an identity absent from the audited production provider catalog.
`SMRA002` is an analysis boundary: dynamic arguments, an unsupported catalog domain,
unrecognized finite declaration or ambiguous program ownership. Both fail the audit.
Do not interpret an unresolved call as a missing resource or a proven runtime crash.

The source pass intentionally excludes provider implementation files under Core/Assets
and inventories resource types in Core/Assets or Core/Rooms with the suffix `Catalog`,
`Presentation`, `ColorSource` or `Visuals`. It does not perform arbitrary interprocedural
points-to analysis. Property-only lookups, static factory lifecycle, raw texture/memory
access, reflection and differently named providers are outside that inventory. New resource
domains need explicit adapters; this tool does not certify those by naming convention alone.
The union of provider identities does not prove that a particular host installed the right
provider instance, that an override file is complete, or that a resource has correct pixels.

Generated selector tables can include native records which are not reachable through the
translated runtime. An absent export is still a declaration mismatch, but reachability and
alternative renderers must be inspected from source before classifying it as a player bug.
No gameplay search is needed or authorized to establish that distinction.

The JSON has a schema `version`, scope, reference/distinct-missing/unresolved counts,
per-domain `coverage`, all `findings`, and all `consumers`. Findings include domain, owner,
identity, relative source location and explanation. Multiple references to one missing
identity are preserved; `missingResourceCount` counts distinct domain/identity pairs.
Output order is deterministic and contains no timestamp, ROM bytes, screenshots or saves.
Console output is abbreviated; JSON is the authoritative complete report.

## Confirm the auditor itself

Focused catalog confirmations for already identified omissions in #1156:

```powershell
dotnet run --project csharp/src/SuperMetroid.ResourceAudit -c Release -- --work-robot-resource-check
dotnet run --project csharp/src/SuperMetroid.ResourceAudit -c Release -- --mama-turtle-resource-check
```

These constructed-data checks confirm selected compositions and legacy edit inheritance;
they do not execute gameplay or search for additional defects. Reconciliation evidence is
recorded in [FINDING-RESOLUTION.md](FINDING-RESOLUTION.md).

```powershell
dotnet run --project csharp/src/SuperMetroid.ResourceAudit -c Release -- --self-check
```

These small fixtures confirm missing constant IDs, named/cast arguments, dynamic and unknown
domains, deterministic reports, bound-vs-direct projectile routing, shared palette aliases
and the separate title provider. They confirm the audit's identified contracts; they do
not explore gameplay or search for additional player bugs.

`--work-robot-resource-check` confirms the specific #1156 laser omission with constructed
art through the real catalog loader: all seven bindings, preserved schema-11 override
edits, inherited new stock frames and rejection of incomplete old stock. It does not
run an enemy, game frame or room.
