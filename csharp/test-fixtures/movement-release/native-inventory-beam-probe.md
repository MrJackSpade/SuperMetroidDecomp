# Inventory input parity reproduction (#395)

This is an isolated call to unpatched retail 65816 instructions. It does not
execute upstream C's explicit `Fixed var bug` guards in the Boots/Weapons handlers.
The loader in `native-release-probe.h` restores cartridge bytes after SnesInit's
comparison patches. No SDL window, SRAM writes, or gameplay cheats are involved.

Temporarily include `native-release-probe.h` then `native-inventory-beam-probe.h`
after the includes in `upstream-sm/src/sm_rtl.c`. Dispatch
`DiagnosticInventoryBeams(argv[2])` from main before SDL startup for
`--inventory-beam-probe`. Build Release/x64 with installed MSBuild and
`PlatformToolset=v145`; invoke with the local `Super Metroid.smc` path.
Remove these hooks and rebuild afterward. Neither hook is committed.

## Actual CPU observations, 2026-09-09

Seed: selector $0003 (Boots/HiJump), equipped beams $0004, collected beams $100F,
items $3300, empty scratch tilemap filled with $5555. Call $82:B150, the Boots
category routine. For the separate-frame accept case, subsequently call the
Weapons handler at $82:AFBE with A only.

| Input timing | Final selector | Equipped beams | Tile effect |
| --- | --- | --- | --- |
| Left+A together | $0401 | $000C | Nine words copied at $3D08-$3D18 |
| Left then A | $0401 | $0008 | Five words at $3D08-$3D10; Spazer recolored |
| Left only | $0401 | $0004 | No label writes |

Same-frame copied words are `08FF 08EC 08ED 08EE 08EF 08FF 0900 0901 0902`.
Separate-frame copies only the first five. The extra four are from reading
beyond Plasma's five-word label with the Boots handler's nine-word copy length.
This establishes both the bit mismatch and exact tile footprint independently
of the wiki description. The extended probe calls $82:B20C afterward and confirms
the wireframe overwrites the ninth word with $0000 for this equipment set. The
remaining overrun renders the visible VAR suffix.

## C# reproduction

`dotnet run --project csharp/src/SuperMetroid.Verification -- --invalid-beam-selection`
seeds Boots through the real initial-selection rule, then supplies the remaining
inventory before the tested input. It drives the public pause Step path. Currently
failed before the fix: Left+A stayed in Boots (category 3, expected 1). The production
handler lacked cross-category movement and used a simple toggle rather than native
move-then-category-response ordering. The fixed path preserves the original
category's copy length and Weapons-only exclusion. Tests now compare simultaneous,
adjacent-frame, and Left-only beam bits, measured tile writes and rendered pixels.
Additional normal navigation enters each Boots item from the initial Beams category.
