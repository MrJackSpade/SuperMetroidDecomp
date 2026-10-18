# CPU A-bus boundary

## What this boundary is for

The destination has no CPU bus in the simulation (`manifesto.md`, "The machine is removed, not encapsulated"):
no address space, no WRAM cells, no memory views. Gameplay state is owned by the system that uses it, and the
CPU address map survives only in the import and export codecs that read a ROM, a snapshot or a legacy save.

Everything below is the **scaffolding** that gets the tree from a bus-shaped port to that destination. A boundary
attribute makes a remaining reach-in visible so it can be removed; it is debt with a label on it, not a design. A
cell catalog in Core is the first step for an address that used to be scattered, and the next step is the owning
system holding that state with no cell at all. A domain view over the bus is a step, and the next step is deleting
the view when its last consumer owns its state.

Direction rules:

- Nothing new goes behind this boundary. A change that adds a cell, a view member or a bus parameter to the
  simulation moves the wrong way, whatever reason it carries.
- A boundary is narrow (a type or a method) so it cannot hide more than it labels.
- When a system takes ownership of a cell's state, the cell, its view member and its inventory entry go in the
  same change.

## Layers while the bus is being removed

| Layer | Lives in | May know addresses? |
|---|---|---|
| Address-space implementation | `SuperMetroidAddressSpace` (`ISnesMemoryBus`), cartridge import source | yes — `[CpuBusBoundary("CPU-bus implementation …")]` |
| Cell catalogs | static classes of `CpuBusCell` descriptors, one per domain (e.g. `SamusCartridgeTables`) | yes — the only place an address literal appears; `[CpuBusBoundary("catalog …")]` |
| Domain memory views | classes over `ISnesMemoryBus` exposing typed properties/operations/tables; Core has none left, the last (the save mirror) having become the owned `SaveMirrorImage` | through cells only — `[CpuBusBoundary("domain view …")]` |
| Native-layout codecs | WRAM snapshot import/export, ROM import decoders | through cells only — `[CpuBusBoundary("codec …")]` |
| Exact bus tests | mapping/mirror/wrap/side-effect tests and bus test doubles | `[CpuBusBoundary("exact bus test …")]` on the narrowest type or method |
| Everyone else | gameplay, runtime, rendering, frontend, verification of behaviour | **no** — typed members only |

`[CpuBusAccess]` marks the raw API: `ISnesAddressSpace`, `ISnesMutableMemory`, `ISnesMemoryBus`,
`ISnesCpuPeripheralSource`, `IImportCartridgeSource`, `SnesWorkRam`,
`CpuBusCell` and the cell primitives `WramByte`,
`WramWord`, `WramBytes`, `WramWords`, which only the exact bus tests still use and which live in test support. Import-time raw decoders shared by several
codecs (`CartridgePaletteImporter`, `CartridgeBankWindow`) are themselves `[CpuBusAccess]`
primitives, not boundaries: each importer that calls them is the codec boundary. The address space exposes no raw WRAM
buffer and maps no SRAM: saves are `SaveSlotStore` state, and `SaveRamCodec` decodes and encodes the 8 KiB SRAM image
as bytes at the import and export boundaries (legacy `.srm`, recording headers, recorder seeds). This is the
pattern every other region follows on its way out: an owned store in the simulation, a codec at the edge, and no
cell in between.

## Rules (analyzer)

- **SME6290** — using a `[CpuBusAccess]` member (invocation, property, method group, construction,
  or a concrete implementation of an attributed interface member) outside a `[CpuBusBoundary]`.
- **SME6291** — a class/struct implementing a `[CpuBusAccess]` interface outside a boundary.
- **SME6292** — in Core, a parameter, field or property (record and primary-constructor
  parameters included) typed as a `[CpuBusAccess]` interface (`ISnesAddressSpace`, `ISnesMemoryBus`,
  `ISnesMutableMemory`) outside a boundary. SME6290 checks use; this rule checks threading, so a
  bus passed along without being read is also reported. Gameplay code takes the owned state or
  narrow service it uses. Every declaration is reported; there is no baseline.

All are build errors. The analyzers check the enclosing member, so a boundary on a local function
or lambda has no effect; extract a named method.

**Code fix (SME6290).** A view property that returns exactly one cell's raw value — same width and
signedness, no validation or conversion — declares `[CpuBusViewOf(typeof(Catalog), nameof(Cell))]`.
The IDE fix (`SuperMetroid.EnsureAnalyzer.CodeFixes`) rewrites a constant-address WRAM word/byte
read, or a primitive read over a named cell, to `new View(bus).Member` only when the
address, width and region identify one catalog cell and exactly one member declares it with the
read's own type. Every other mapping is ambiguous and is migrated by hand.

**Test doubles.** Shared exact bus doubles live in `csharp/test-support/CpuBusTestDoubles`
(`CartridgeReadGuard`, `MutableCartridgeReadGuard`, `SyntheticCartridge`). A double that wraps a
source reads through `CpuBusRegionRead`, which routes each address to the source's typed cartridge,
or WRAM contract and fails when the source lacks it. Raw seeding of the sparse
`TestAddressSpace` (`WriteTestByte/Word/Words`, `WriteBytes`) happens only inside named synthetic
cartridge fixture builders.

Boundaries are narrow: mark the view/catalog/codec type or the exact test method, never a whole
project, test class family or `Program` partial. The reason names which layer it is.

## Cells and catalogs (scaffolding)

A cell is where an address goes when it is first pulled out of functional code, so that it exists in one
place with its meaning stated. It is not where the address lives permanently: a Core gameplay catalog declares
no WRAM cell, and each remaining cell is replaced by owned state in the system that reads it.

```csharp
/// <summary>Samus's WRAM state (bank $7E).</summary>
[CpuBusBoundary("catalog: Samus WRAM")]
public static class SamusWorkRam
{
    /// <summary>$7E:09C2 <c>SAMUS_HEALTH</c>: current energy.</summary>
    public static readonly CpuBusCell Health = CpuBusCell.WramWord(nameof(Health), 0x7E09C2);
}
```

- Every address literal is a `CpuBusCell` in a catalog, with the address, native symbol and
  meaning in its XML summary. Two meanings that share bytes are two cells; the second declares
  `aliasOf:` the first.
- Primitives bind from cells (`new WramWord(bus, SamusWorkRam.Health)`), which checks region and
  width. Tables are `WramWords`/`WramBytes` with bounds-checked element access; callers
  never compute element addresses.
- Encoding text records the domain type: `"u16 little-endian"`, `"SamusPose (u16)"`,
  `"signed 8.8 (u16)"`, `"ScrollState[] (u8)"`.

## Views (scaffolding)

A view is a typed facade over bus storage that has not yet been moved into its owner. It is still hardware
coupling: a "typed" property that returns a cell's word has changed the spelling, not the design.

- While a view exists, its properties and operations expose the domain type end to end (enums,
  `Position16<T>`, bounded values, correct width/signedness); narrowing, validation and byte order happen
  inside the view, never in callers.
- Preserve gameplay-observable behaviour exactly: bank wrapping, mirrors, access order and every write side
  effect that gameplay can observe. When the state moves into its owner, the behaviour that was load-bearing
  is kept as a named operation on the owner; the rest goes.
- A view is not extended to cover a new consumer. The new consumer takes the owned state, and the move of
  that state out of the view is part of the same change.

## Inventory and checks

`docs/cpu-bus-inventory.json` is generated from the catalogs (owner, name, region, address, size,
encoding, aliases, consumers) by `SuperMetroid.DebugRunner --generate-cpu-bus-inventory`, run from
the repository root; it refuses to write while a catalog rule below is broken. The verifier
(`--cpu-bus-boundary`, part of the full run) enumerates every `CpuBusCell` and requires the
committed inventory to list exactly the catalog cells:

- `(owner, name)` is unique;
- two cells overlap only through a declared `AliasOf`: one cell's alias chain reaches the other
  or a cell that covers it (overlap is judged on physical bytes, so `$00:0123` and `$7E:0123`
  are the same WRAM byte). Staging surfaces that the cartridge reuses under several meanings are
  owned buffers with views, so no mutable cell aliases another;
- Core gameplay catalogs declare no WRAM cell: gameplay state lives in its owning system, and every
  Core WRAM cell is reported by `VerifyCoreDeclaresNoWorkRamCells`;
- representative typed round trips, mapping/mirror/wrap and side-effect tests cover the primitives.
