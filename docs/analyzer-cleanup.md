# Analyzer finding cleanup

The repository's analyzers run at error severity with no baseline, count or allowlist, so the build is red until
every finding is fixed or individually justified. This note is the starting point for that cleanup.

## Counting the findings

A red build stops at the first failing project, so `dotnet build` cannot list everything. The census loads the
whole solution as Visual Studio does (project references are source compilations) and lists every SME finding:

```
dotnet build csharp/src/SuperMetroid.EnsureAnalyzer -c Release
dotnet run tools/sme-census/SmeCensus.cs -- csharp/SuperMetroid.Full.slnx findings.tsv
```

Each row is `rule, severity, project, path:line, message`. It fails if a project has a real compile error.

## Starting count: 30,836

| Rule | Count | Fix |
| --- | ---: | --- |
| SME6340 | 27,259 | Raw hex literal in functional code: a named catalog member, quantity, enum, flags or definition. |
| SME6347 | 2,150 | `byte`/`ushort` instance state: store the domain type and convert at the codec. |
| SME6342 | 838 | Opaque word array: typed definitions with named fields. |
| SME6346 | 179 | `[Flags]` cast to a primitive: keep the flags type; convert with `ToNativeWord` at the boundary. |
| SME6341 | 76 | Address-based dispatch: dispatch on a typed state, command or definition. |
| SME6392 | 75 | Species identity test outside its owner (Core): ask the bound entity, or move the test. |
| SME6284 | 73 | Controller word conversion outside a `[ControllerWordBoundary]`. |
| SME6345 | 48 | Raw width shift or mask: a named encoding operation (`SnesLittleEndian`, fixed-point types, `SnesAddress`). |
| SME6271 | 45 | Discard arm in an enum switch that does not fail. |
| SME6332 | 22 | Broad context parameter: accept the narrower capability interface. |
| SME6276 | 20 | Arithmetic cast to an enum: a bounded domain transition. |
| SME6349 / SME6348 | 18 / 3 | Hand-written countdown: `NativeCountdown`. |
| SME6274 | 14 | Primitive compared against named constants: model the domain as a type. |
| SME6292 | 12 | CPU-bus declaration in Core outside a `[CpuBusBoundary]`. |
| SME6275 / SME6277 | 3 / 1 | Raw primitive standing in for a domain type. |

By project: Verification 27,485 (26,199 of them SME6340), Core 1,832 (1,605 SME6347), RenderVerification 427,
IntegrationVerification 370, DesktopVerification 314, AssetExtraction 196, DebugRunner 69, ResourceAudit 54,
Tooling 43, Desktop 15, ReachabilityAudit 9, Diagnostics 7, Rendering.Direct3D11 5, Android 4,
ArchitectureAudit 3, BuildPolicyVerification 3.

The heaviest files are Verification suites that state native values inline (SamusMorphBallVerification 1,087,
SamusAimVerification 944, ProjectileInstructionsVerification 922, SamusTerrainMovementVerification 843,
ProjectileRadiiVerification 811, LookupStream1Verification 772).

`docs/cartridge-encoding.md` describes each cartridge-encoding rule; `docs/cpu-bus-boundary.md` covers SME6292.

## What counts as resolved

- Fix the code (AGENTS.md "Analyzer findings are addressed, never grandfathered").
- Otherwise justify that one site: a same-line `// cartridge-encoding: allow(Class) - reason`, the rule's boundary
  attribute with its reason (`[ControllerWordBoundary("...")]`, `[CpuBusBoundary("...")]`), or `[SnesEncodingOwner]`
  on a type that owns and documents an encoding. 185 such justifications exist and deserve the same review.
- Never a baseline, allowlist, count, blanket suppression or severity change.
- Core first: every other project references it, and it must build before EnsureVerification can run the analyzer
  tests.

## Other checks that are red

- `--magic-number-audit` and `VerifyCoreDeclaresNoWorkRamCells` (10 Core WRAM cells, #1319/#1330) fail on every
  finding.
- Failing before the analyzer restore (confirmed against 35e23ceaf): VerifySamusBodyArtwork, VerifySamusEaterVisuals,
  VerifySamusStandaloneSpeeds, VerifySamusAtmosphericAnimationDefinitions, and the lowc movie replay ("Decrementing
  Samus count 0" at update 80012).
- DesktopVerification: three capture checks reject an `IntPtr` in debugger save states, and the close-during-load
  checks let an exception escape the process.
