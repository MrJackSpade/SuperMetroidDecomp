# Static resource dependency audit

Issue #1155: find undeclared installed-resource dependencies from source and immutable
definition catalogs, without searching for failures by playing or replaying the game.
This tool is development-only and is not referenced or shipped by either playable host.

#1161 adds a separate static PLM program audit: complete
timer/draw records, byte/word operands, branches, links and production provider
closure. Use `--plm-program-audit` or `-p:RunPlmProgramAudit=true`; it does not
execute gameplay and is also a Windows release-packaging gate.

#1156 reconciled the initial inventory: 266 absent identities and 353 unresolved
boundaries are fully accounted, with no findings in the final declared static
scope. All 352 original consumer sites remain in the report. This is not a whole-game
validation claim.

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
- The ordinary renderer's compiled zero-part frames satisfy display requirements without
  editable artwork. The exact supported bank/pointer predicate and no-OAM return branch
  are guarded; a changed renderer contract fails the adapter. These definitions do not
  satisfy direct installed-catalog lookups and are listed separately in the JSON.
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
- Mother Brain's fake-death palette program has its own typed adapter: its fourteen
  color operands must map to the timed-entry rows installed by the real importer/loader.
  Its cutscene-baby sprite program declares bank ownership explicitly; resolving that
  declaration also checks all nine compiled bindings and their shared Tourian artwork.
- Core resource-catalog instance operations, including internal draw methods, are inventoried
  through Roslyn. Named arguments, constant casts and symbols resolve semantically.
  Supported constant bank/pointer lookups are checked; dynamic IDs and unknown catalog
  domains are retained as explicit unresolved findings with their source/arguments.
- Particular complete, loader-validated domains have source-reviewed, method-specific
  closure rules. They are guarded by provider/definition fingerprints, not baselines.
  Changed contracts, new operations, invalid constants and unbounded string names remain
  failures. See [CLOSED-PROVIDER-CONTRACTS.md](CLOSED-PROVIDER-CONTRACTS.md) for scope/proof.
  Named menu selections compare finite source-resolved names against loader-required
  keys. Locals/private fields require every initializer/write to be finite; private
  helper parameters require closed call inputs, and expression-bodied finite switch
  results may be followed. Unknown writes, ref aliases, delegate escapes, public
  state, unavailable bodies and cycles stay unresolved. Reflection and debugger
  payload validity are not certified by this source-flow analysis.
  Map names also support the two source-guarded bounded area-name factories;
  lookalike functions and arbitrary strings are not inferred. The credits row
  proof is revoked by a Core reference to its variable-row verification factory.
  Ordinary projectile composition coverage is revoked by external Core use of
  the shared partial factory; timed binding coverage is revoked by external
  access to its array-backed owner list. Private flare coverage remains separate.

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

The JSON has schema `version` 2, scope, reference/distinct-missing/unresolved counts,
per-domain `coverage`, all `findings`, all `consumers`, and source-owned `compiledDefinitions`.
It also retains per-consumer `classifications` with the precise closure rule, reason and
guarded provider-source fingerprints; classified calls do not disappear from inventory.
Coverage exports count both installed identities and applicable compiled definitions.
Findings include domain, owner,
identity, relative source location and explanation. Multiple references to one missing
identity are preserved; `missingResourceCount` counts distinct domain/identity pairs.
Output order is deterministic and contains no timestamp, ROM bytes, screenshots or saves.
Console output is abbreviated; JSON is the authoritative complete report.

## Confirm the auditor itself

Focused catalog confirmations for already identified omissions in #1156:

```powershell
dotnet run --project csharp/src/SuperMetroid.ResourceAudit -c Release -- --work-robot-resource-check
dotnet run --project csharp/src/SuperMetroid.ResourceAudit -c Release -- --mama-turtle-resource-check
dotnet run --project csharp/src/SuperMetroid.ResourceAudit -c Release -- --zero-resource-check
dotnet run --project csharp/src/SuperMetroid.ResourceAudit -c Release -- --friendly-animal-resource-check Etecoon
dotnet run --project csharp/src/SuperMetroid.ResourceAudit -c Release -- --friendly-animal-resource-check Dachora
dotnet run --project csharp/src/SuperMetroid.ResourceAudit -c Release -- --friendly-animal-resource-check EscapeEtecoon
dotnet run --project csharp/src/SuperMetroid.ResourceAudit -c Release -- --friendly-animal-resource-check EscapeDachora
dotnet run --project csharp/src/SuperMetroid.ResourceAudit -c Release -- --ordinary-enemy-resource-check Hibashi
dotnet run --project csharp/src/SuperMetroid.ResourceAudit -c Release -- --crocomire-skeleton-resource-check
dotnet run --project csharp/src/SuperMetroid.ResourceAudit -c Release -- --kraid-part-resource-check Foot
dotnet run --project csharp/src/SuperMetroid.ResourceAudit -c Release -- --kraid-part-resource-check Lint
dotnet run --project csharp/src/SuperMetroid.ResourceAudit -c Release -- --nuclear-waffle-resource-check
```

The ordinary-enemy check also accepts `Zebetite`, `WreckedShipGhost`, `Powamp`, `Spark`,
or `Shitroid`, each targeting its already identified catalog omission.

These constructed-data checks confirm selected compositions and legacy edit inheritance;
they do not execute gameplay or search for additional defects.

The Crocomire skeleton check also confirms the identified thirteen-component collapse pose
through the production importer and loader. Its synthetic source has no hitbox bytes,
and verifies that the increased component bound applies only to the skeleton family.

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
