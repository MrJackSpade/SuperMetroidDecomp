# Counting and resolving analyzer findings

The repository's analyzers run at error severity with no baseline, count or allowlist, so the build is red until
every finding is fixed or individually justified (`manifesto.md`, sections 6 to 8). This note is the mechanics.

By owner decision the SME rules apply to `SuperMetroid.Core` and `SuperMetroid.AssetExtraction` only: Core is what
the Godot port carries over and AssetExtraction is the import pipeline it keeps, while every other project is dropped
or refactored in the migration and the Godot port gets its own verification. `csharp/.editorconfig` sets the SME rules
to `none` for those other projects in one recorded section; everything else in this note applies to Core and
AssetExtraction, where every SME rule stays an error.

## Counting the findings

A red build stops at the first failing project, so `dotnet build` cannot list everything. The census loads the
whole solution as Visual Studio does (project references are source compilations) and lists every SME finding:

```
dotnet build csharp/src/SuperMetroid.EnsureAnalyzer -c Release
dotnet run tools/sme-census/SmeCensus.cs -- csharp/SuperMetroid.Full.slnx findings.tsv
```

Each row is `rule, severity, project, path:line, message`. It fails if a project has a real compile error. Work
from the TSV; a project is clean when the census says so, not when the build passes. Current counts belong in the
ticket that is driving them down, not here.

## What counts as resolved

- Fix the code to the destination shape (`manifesto.md`, section 7: "Fix versus satisfy").
- Otherwise justify that one site: a same-line `// cartridge-encoding: allow(Class) - reason`, the rule's boundary
  attribute with its reason (`[ButtonWordEncoding("...")]`, `[CpuBusBoundary("...")]`), `[EncodingOwner("format")]`
  on a type that is by construction the single owner of an encoding, or `[ArchitectureReview(finding, reason)]` on
  a type the architecture audit flags. The reason names the thing, not the rule, and would not be equally true of a
  thousand other sites.
- Never a baseline, allowlist, count, blanket suppression, severity change, rename to an exempt file suffix, or
  change to a rule's scope or exemption lists.
- Core first: every other project references it, and it must build before `EnsureVerification` can run the
  analyzer tests.

`docs/cartridge-encoding.md` describes each cartridge-encoding rule; `docs/cpu-bus-boundary.md` covers SME6292.
