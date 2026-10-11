# CPU A-bus boundary (#629)

Gameplay, runtime, frontend, tooling and test code never know the SNES CPU address map. Addresses,
banks, widths, byte order, mirrors and encodings live in a small set of declared boundaries; every
other caller works with typed, domain-named members.

## Layers

| Layer | Lives in | May know addresses? |
|---|---|---|
| Address-space implementation | `SuperMetroidAddressSpace` (`ISnesMemoryBus`), cartridge import source | yes — `[CpuBusBoundary("CPU-bus implementation …")]` |
| Cell catalogs | static classes of `CpuBusCell` descriptors, one per domain (e.g. `SamusWorkRam`, `RoomScrollWorkRam`, `SaveSlotSram`) | yes — the only place an address literal appears; `[CpuBusBoundary("catalog …")]` |
| Domain memory views | classes over `ISnesMemoryBus` exposing typed properties/operations/tables (e.g. `SamusMemory.Health`, `RoomScrollMemory[cell]`) | through cells only — `[CpuBusBoundary("domain view …")]` |
| Native-layout codecs | save/SRAM codecs, WRAM snapshot import/export, ROM import decoders | through cells only — `[CpuBusBoundary("codec …")]` |
| Exact bus tests | mapping/mirror/wrap/side-effect tests and bus test doubles | `[CpuBusBoundary("exact bus test …")]` on the narrowest type or method |
| Everyone else | gameplay, runtime, rendering, frontend, verification of behaviour | **no** — typed members only |

`[CpuBusAccess]` marks the raw API: `ISnesAddressSpace`, `ISnesMutableMemory`, `ISnesMemoryBus`,
`ISnesCpuPeripheralSource`, `IImportCartridgeSource`, `SnesWorkRam`, `SnesCpuOperandRead`,
`SnesIndirectLongDataRead`, `SuperMetroidAddressSpace.SaveRam`, `CpuBusCell` and the cell
primitives `WramByte`, `WramWord`, `WramBytes`, `WramWords`, `SramBytes`.

## Rules (analyzer)

- **SME6290** — using a `[CpuBusAccess]` member (invocation, property, method group, construction,
  or a concrete implementation of an attributed interface member) outside a `[CpuBusBoundary]`.
- **SME6291** — a class/struct implementing a `[CpuBusAccess]` interface outside a boundary.

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
encoding, aliases, consumers). The verifier enumerates every `CpuBusCell`:

- `(owner, name)` is unique;
- two cells overlap only through a declared `AliasOf`, and every alias pair is round-tripped
  (write through one view, read through the other) to prove coherence;
- representative typed round trips, mapping/mirror/wrap and side-effect tests cover the primitives.
