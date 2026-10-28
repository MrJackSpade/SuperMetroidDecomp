# Cartridge addresses stay in the import pipeline (SME6447)

The ROM is known only to the import and extraction pipeline. A cartridge address, bank, work-RAM offset, pointer,
video-memory destination or ROM table stride inside `SuperMetroid.Core` is scaffolding on the way out (project manifesto;
v0.9.0 manifesto). This note describes the target shape and the rule that detects the addresses that remain (#1411, and
the domain tickets that follow it).

## Target shape

Definition data the game needs (a duration, a speed, a shape, a colour, a frame list, a count) stays in Core in a
domain-named catalog, with its meaning on the member's XML summary and no address in its name or value. An address
moves to the AssetExtraction codec that reads it once; the codec produces the installed definition or the semantic
identity. An enum whose members were backed by addresses gets arbitrary values, and every read of such a value becomes
a translation in the codec or a dispatch through a behaviour reference.

A `*RomData`, `*Addresses`, `*Pointers` or `*CodePointers` file is deleted, not renamed: a file with the same members under
a new suffix is the lazy column of the project manifesto's section 7.

## The rule

`CartridgeAddressAnalyzer` reports, in the `SuperMetroid.Core` assembly only, every value written as a number in an
integral constant (field or local), a field or property initializer, an enum member value, an attribute argument or a
switch label (a `case` value or a constant pattern), when the value is a plausible cartridge or work-RAM address:

- a 24-bit value in a mapped bank, anywhere: the cartridge's ROM in banks `$80`–`$DF` at `$8000` and above, save RAM in
  banks `$70`–`$7D` below `$8000`, or work RAM in banks `$7E`–`$7F`;
- a 16-bit value from `$8000` to `$FFFF` declared in a type, or under a namespace, whose name contains a cartridge
  structure word as a whole camel-case word (`Rom`, `Cartridge`, `Bank`, `Pointer`, `Address`, `Wram`, `Sram`, `Snes`,
  `Native`, with plural endings);
- a 16-bit value from `$8000` to `$FFFF` in a file the encoding scanner names as a definition catalog
  (`CartridgeEncodingScanner.CatalogSuffixes`);
- a 16-bit value from `$8000` to `$FFFF` in an address-valued enum, whatever its file or name (#1452): an enum that is
  not `[Flags]` and has a member valued from `$8000` to `$FFFE`. Identities are ordinal names with arbitrary values, so
  a member in the cartridge's half of a bank is a routine or record address. A `[Flags]` enum's high member is a bit,
  and the all-ones word `$FFFF` is the native "none" sentinel that no routine or record starts at, so an ordinal enum
  whose only high member is that sentinel is not address-valued.

A value is read as written: a literal, or arithmetic, casts and parentheses over literals. A reference to another
constant is checked where that constant is declared, so one address is reported once.

The bank `$00`–`$5F` mirrors of the ROM are not listed: a value such as `$18000` is as often a 16.16 fixed-point
quantity as an address, and the game addresses its ROM through the `$80`–`$DF` banks.

## Catalog names no longer exempt addresses

The encoding scanner's catalog-suffix list exempts definition catalogs from the hexadecimal-literal and storage-width
rules (`docs/cartridge-encoding.md`). `Addresses.cs`, `CodePointers.cs`, `Pointers.cs`, `RomData.cs` and `Tables.cs` are
removed from it: those names say the file holds addresses or ROM structure, which is exactly what the rules exist to
find.

## Exemplar

```csharp
// Before: the projectile's program is a cartridge address, and the catalog holds it.
internal static class SamusProjectileProgramAddresses
{
    internal const ushort WaveBeamTrail = 0x8f91;
}

// After: the projectile names its behaviour; the codec translated the address once, at import.
internal ProjectileTrail Trail { get; } = ProjectileTrail.Wave;
```

## Allowance

There is no allowance list. A site is justified individually only when the reason would not be equally true of a
thousand other sites. No baseline, allowlist, count, severity change or scope change (project manifesto, section 6).

## Status

The rule ships at error severity. Its findings in the Samus domain are #1411's work list, and the findings in other
domains are the work lists of the address tickets that follow it. Count the findings with the census
(`docs/analyzer-cleanup.md`).
