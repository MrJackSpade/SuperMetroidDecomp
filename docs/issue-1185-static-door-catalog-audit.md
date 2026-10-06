# Independent static door catalog audit (#1185)

The previous completeness check stopped at the first pointer outside the compiled
known-header set. That circular boundary rule accepted the missing $83:A18A
sentinel and truncated Tourian/Maridia lists in #1181-#1184.

The new oracle is generated independently: pinned disassembly labels bound each
door-list allocation, each `dw` names its expected header, and cartridge bytes
must agree with every symbol and room/list link. No compiled door catalog is used
to generate the oracle. Native room-state labels identify the compressed level
data; decompression inventories distinct type-$9 BTS values per state without
executing scripts, collision, or gameplay. The generator includes the unused
$8F:B3E1 room, excludes the separate debug room, and imports all 597 physical
headers plus both elevator sentinels.

The checked-in manifest contains definition metadata and door-index summaries,
not graphics, full level data, recordings, saves, or a ROM. Its LF-normalized
SHA-256 is pinned so it cannot silently shrink alongside the compiled catalog.
The importer also checks source-file hashes and the J/U NTSC 1.0 cartridge hash.
Disassembly revision: `362be646929cf8e483f692b73a6561cfc2dc1d0d`.

## Run and regenerate

Normal local/CI use requires no ROM or external checkout:

```text
dotnet run --project csharp/src/SuperMetroid.ResourceAudit -- --door-catalog-self-check .
dotnet run --project csharp/src/SuperMetroid.ResourceAudit -- --door-catalog-audit . out/door-catalog-audit.json
dotnet build csharp/src/SuperMetroid.ResourceAudit -p:RunDoorCatalogAudit=true
```

Regeneration requires the exact pinned disassembly files and private local ROM:

```text
dotnet run --project csharp/src/SuperMetroid.ResourceAudit -- --door-catalog-manifest ../../upstream-disassembly "Super Metroid.smc" csharp/src/SuperMetroid.ResourceAudit/Data/native-door-catalog.json
```

Review any manifest diff and its normalized digest before updating `ManifestHash`.
The dedicated workflow runs on matching pushes from any branch and PRs; release
packaging also runs the audit. Findings have stable codes and room/list/BTS context:
DOOR001 boundaries/inventory, DOOR002 ordering/pointers, DOOR003 headers,
DOOR004 room/state ownership, DOOR005 invalid native indexes, DOOR006 missing
compiled destinations used by stored room data.

## Results

- 262 independently bounded room lists, 606 entries, 599 header records.
- 323 room states, 742 distinct stored door-index references across those states.
- No further compiled-list omissions or header mismatches beyond the already
  fixed Tourian/Maridia sentinel problem.
- One native-data exception: unused room $8F:B3E1, state $B3EE, level $C8F40B has
  BTS $01 despite its one-entry list at $B408. The disassembly names it UNUSED;
  none of the native door headers has this room as its destination. This is
  preserved and reported explicitly, not given an invented port-only destination.
  The exception requires this exact room/state/level/list/entry and the absence
  of inbound native doors. Another index or room is not exempt.

Focused audit self-checks reproduce the original two-entry Tourian and three-entry
Maridia omissions, plus missing/extra lists, reordered entries, and unknown
pointers. These must fail the analyzer. The unmodified oracle must pass. Checks
also verify that the native-data exception cannot hide other rooms or indexes.

## Coverage boundary

This is complete for native door-list allocations, their header definitions,
room/state ownership, and door blocks already present in compressed room data.
It is not a proof of script-created block values or runtime reachability. PLM
draws can replace collision types and interpreter instructions can change BTS;
those require room-aware script dataflow analysis, not simply scanning constant
values or pretending raw room data is the final state. The report states this
limitation. Catalog completeness still protects script-created references to
every legitimate native list entry, including entries absent from initial data.
No gameplay sweeps or exploratory tests were run.
