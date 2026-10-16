# CPU A-bus boundary (#629)

Gameplay, runtime, frontend, tooling and test code never know the SNES CPU address map. Addresses,
banks, widths, byte order, mirrors and encodings live in a small set of declared boundaries; every
other caller works with typed, domain-named members.

## Layers

| Layer | Lives in | May know addresses? |
|---|---|---|
| Address-space implementation | `SuperMetroidAddressSpace` (`ISnesMemoryBus`), cartridge import source | yes — `[CpuBusBoundary("CPU-bus implementation …")]` |
| Cell catalogs | static classes of `CpuBusCell` descriptors, one per domain (e.g. `SaveMirrorWorkRamAddresses`, `SaveRamLayout`) | yes — the only place an address literal appears; `[CpuBusBoundary("catalog …")]` |
| Domain memory views | classes over `ISnesMemoryBus` exposing typed properties/operations/tables (e.g. `SaveMirrorMemory.LoadingGameState`) | through cells only — `[CpuBusBoundary("domain view …")]` |
| Native-layout codecs | save/SRAM codecs, WRAM snapshot import/export, ROM import decoders | through cells only — `[CpuBusBoundary("codec …")]` |
| Exact bus tests | mapping/mirror/wrap/side-effect tests and bus test doubles | `[CpuBusBoundary("exact bus test …")]` on the narrowest type or method |
| Everyone else | gameplay, runtime, rendering, frontend, verification of behaviour | **no** — typed members only |

`[CpuBusAccess]` marks the raw API: `ISnesAddressSpace`, `ISnesMutableMemory`, `ISnesMemoryBus`,
`ISnesCpuPeripheralSource`, `IImportCartridgeSource`, `SnesWorkRam`,
`CpuBusCell` and the cell primitives `WramByte`,
`WramWord`, `WramBytes`, `WramWords`, `SramBytes`. Import-time raw decoders shared by several
codecs (`CartridgePaletteImporter`, `CartridgeBankWindow`) are themselves `[CpuBusAccess]`
primitives, not boundaries: each importer that calls them is the codec boundary. The address space exposes no raw WRAM/SRAM
buffer: whole-image SRAM import/export goes through the `SaveRamImage` codec. Views hand callers
bounded `WorkRamByteTable`/`WorkRamWordTable` collections bound to their own cells.

## Rules (analyzer)

- **SME6290** — using a `[CpuBusAccess]` member (invocation, property, method group, construction,
  or a concrete implementation of an attributed interface member) outside a `[CpuBusBoundary]`.
- **SME6291** — a class/struct implementing a `[CpuBusAccess]` interface outside a boundary.

Both are build errors. The analyzer checks the enclosing member, so a boundary on a local function
or lambda has no effect; extract a named method.

**Code fix (SME6290).** A view property that returns exactly one cell's raw value — same width and
signedness, no validation or conversion — declares `[CpuBusViewOf(typeof(Catalog), nameof(Cell))]`.
The IDE fix (`SuperMetroid.EnsureAnalyzer.CodeFixes`) rewrites a constant-address WRAM word/byte or
SRAM byte read, or a primitive read over a named cell, to `new View(bus).Member` only when the
address, width and region identify one catalog cell and exactly one member declares it with the
read's own type. Every other mapping is ambiguous and is migrated by hand.

**Test doubles.** Shared exact bus doubles live in `csharp/test-support/CpuBusTestDoubles`
(`CartridgeReadGuard`, `MutableCartridgeReadGuard`, `SyntheticCartridge`). A double that wraps a
source reads through `CpuBusRegionRead`, which routes each address to the source's typed cartridge,
WRAM or SRAM contract and fails when the source lacks it. Raw seeding of the sparse
`TestAddressSpace` (`WriteTestByte/Word/Words`, `WriteBytes`) happens only inside named synthetic
cartridge fixture builders.

Boundaries are narrow: mark the view/catalog/codec type or the exact test method, never a whole
project, test class family or `Program` partial. The reason names which layer it is.

## Cells and catalogs

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
  width. Tables are `WramWords`/`WramBytes`/`SramBytes` with bounds-checked element access; callers
  never compute element addresses.
- Encoding text records the domain type: `"u16 little-endian"`, `"SamusPose (u16)"`,
  `"signed 8.8 (u16)"`, `"ScrollState[] (u8)"`.

## Views

- Properties and operations expose the domain type end to end (enums, `Position16<T>`, bounded
  values, correct width/signedness); narrowing, validation and byte order happen inside the view.
- Organize by domain and compose (`memory.Samus`, `memory.Room.Scroll`), never one flat bus object.
- Preserve native behaviour exactly: bank wrapping (`SnesAddressMath.AddWithinBank`), mirrors,
  access order (low byte first unless the cartridge does otherwise) and every write side effect.

## Inventory and checks

`docs/cpu-bus-inventory.json` is generated from the catalogs (owner, name, region, address, size,
encoding, aliases, consumers) by `SuperMetroid.DebugRunner --generate-cpu-bus-inventory`, run from
the repository root; it refuses to write while a catalog rule below is broken. The verifier
(`--cpu-bus-boundary`, part of the full run) enumerates every `CpuBusCell` and requires the
committed inventory to list exactly the catalog cells:

- `(owner, name)` is unique;
- two cells overlap only through a declared `AliasOf`: one cell's alias chain reaches the other
  or a cell that covers it (overlap is judged on physical bytes, so `$00:0123` and `$7E:0123`
  are the same WRAM byte). Every alias pair is round-tripped (write through one cell, read
  through the other) to prove coherence;
- representative typed round trips, mapping/mirror/wrap and side-effect tests cover the primitives.
