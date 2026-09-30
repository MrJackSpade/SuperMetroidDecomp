# Archived retail-room census evidence (#549)

This file preserves dated diagnostic results, **not the current migration strategy**.
The instructions and source-reader descriptions below describe older commits.
Do not run these gameplay/census/probe commands to discover cartridge reads.
The current source/type-boundary inventory and remaining acceptance work are in
[ROM-FREE-SOURCE-ACCESS-549.md](ROM-FREE-SOURCE-ACCESS-549.md).

As of the 2026-09-29 migration, Core has no cartridge reader or ROM allocation;
the concrete `SuperMetroidAddressSpace.ReadByte` reference API and Core import
fallbacks described below have been removed. Native reference decoding lives in
AssetExtraction. A passing archived gameplay path cannot establish completeness.

## Compiler-guided read-API migration (2026-09-28)

`ISnesAddressSpace.ReadByte` has been removed from the shared gameplay
contract. The ordinary full-solution build now catches any new untyped read
against that interface; the former `NO_UNTYPED_BUS_READS` switch is no longer
needed. Gameplay Core uses explicit WRAM/SRAM readers, a cartridge-import
source for remaining native-data fallbacks, and a narrowly scoped mapped-CPU
reader for instruction/glitch-derived addresses. Each typed reader rejects the
wrong memory region. `SuperMetroidAddressSpace.ReadByte` remains on the concrete
class as a reference oracle for diagnostics; a test-only adapter lets existing
verification and debug tools exercise their synthetic bus fixtures without
making the generic read visible to gameplay Core. This compile-time boundary
is **not** a green ROM-free guarantee: remaining cartridge-import fallbacks,
direct file APIs, and later-frame/transition behavior still require auditing.
Tests remain necessary to verify value and timing parity, but not as the
primary way of discovering direct gameplay bus-read call sites.

The compiled room-header/state-selection path no longer accepts an address
space at all. Diagnostic native room parsing requires `IImportCartridgeSource`;
the installed file-map path remains on its separate compiled-map branch.

## Current one-frame baseline (2026-09-28)

A fresh gameplay snapshot and a full rerun now pass **all 262 retail rooms**
for one isolated neutral frame each: native pixels match and the installed
game performs no cartridge reads. The older failure inventory below describes
the 2026-09-27 baseline, not current open first-frame failures. The snapshots
are private, version-sensitive debugger graphs in ignored `csharp/test-temp`;
stale graphs cannot establish a current census. Separate direct-room probes
also pass held right+fire for 1,500 Kraid-room frames, 1,000 Crocomire-room
frames, and 1,500 Phantoon-room frames. These results do not prove every later
room event, controller sequence, door transition, or complete fight.

An earlier 2026-09-27 all-room pass found 32 first-frame failures among 262 retail
rooms. One was room `$8F:ACF0` reading the Speed Booster escape PLM list; eight
Tourian rooms read the same Metroids-cleared PLM `Sleep` word at `$84:DB42`;
Wrecked Ship room `$8F:CA52` read the attic PLM list at `$84:BAFF`. All three
bounded lists are now compiled. Selected-room reruns for `$ACF0`, `$CA52`,
`$DAE1`, and `$DEDE` pass with native pixel parity and all cartridge reads
blocked; the other six Tourian rooms and 22 unrelated baseline failures have
not been rerun as a batch. Later frames and inter-room transitions are outside
this one-frame diagnostic.

Shaktool room `$8F:D8C5` originally stopped on its `$84:B8D6` resident PLM
list. Compiling the complete three-word list advances the same isolated
first-frame check to a separate missing Shaktool saw-hand composition at
`$AA:E028`; the room remains a census failure, not a pass.

The regular `--intro-cinematic-artwork` verification now restores a gameplay
snapshot once and checks Kraid's first frame after rebinding installed artwork.
This is a regression for the deferred HUD upload: Kraid's private BG2 map owns
VRAM word `$4000`, while his room relocates HUD characters to word `$2000`.
The test compares the entire private BG2 allocation and the rendered frame
against the cartridge-backed game.

For broader diagnosis, opt in to a **one-frame** census of every retail room:

```powershell
$env:SM_ROM_FREE_ROOM_CENSUS = '1'
$env:SM_ROM_FREE_CENSUS_SNAPSHOT_DIR = 'csharp/test-temp/rom-free-room-census-cache'
dotnet run --project csharp/src/SuperMetroid.Verification -c Release -- --intro-cinematic-artwork 'Super Metroid.smc'
```

The first command runs the normal menu/cinematic parity path, then snapshots
the two live gameplay graphs. Each census room starts from a fresh restore of
those graphs, so a prior room's VRAM queue, event bits, or enemy slots cannot
contaminate the next one. It compares the first neutral frame's state, phase,
pixels, and absence of runtime cartridge reads. This is **not** coverage of
doors, later enemy frames, combat, or alternate saved boss states.

The snapshot files contain the source ROM bytes embedded in debugger graphs.
They stay in ignored `csharp/test-temp`; never commit or publish them. After
one capture, repeat a specific room without replaying the cinematic:

```powershell
$env:SM_ROM_FREE_CENSUS_ROOM = 'A59F'
dotnet run --project csharp/src/SuperMetroid.Verification -c Release -- --rom-free-room-census 'Super Metroid.smc' 'csharp/test-temp/rom-free-room-census-cache'
```

Omit `SM_ROM_FREE_CENSUS_ROOM` to rerun all 262 rooms. A failing selected room
also writes native and installed PNGs to ignored
`csharp/test-temp/rom-free-room-census-compare`. Keep those screenshots local.
The diagnostic fails nonzero with the room pointer and exception; it never
silently skips a room, including known incomplete instruction programs.

For a neutral-input sequence within one room, set
`SM_ROM_FREE_CENSUS_FRAMES` to an integer from 1 through 3600 while selecting
`SM_ROM_FREE_CENSUS_ROOM`. For example, use 90 frames to check a Viola loop:

```powershell
$env:SM_ROM_FREE_CENSUS_ROOM = 'B37A'
$env:SM_ROM_FREE_CENSUS_FRAMES = '90'
dotnet run --project csharp/src/SuperMetroid.Verification -c Release -- --rom-free-room-census 'Super Metroid.smc' 'csharp/test-temp/rom-free-room-census-cache'
```

The frame setting is rejected for an all-room census to avoid an accidental
hours-long run. This checks neutral frames only; it does not substitute for
controller sequences, door transitions, combat, or a complete room state graph.

## Direct-room probe without a debugger snapshot

For a single retail room, use the direct-room verifier instead of capturing
and later deserializing the intro gameplay graph:

```powershell
dotnet run --project csharp/src/SuperMetroid.Verification -c Release -- --rom-free-direct-room 'Super Metroid.smc' A59F 900
```

The hexadecimal argument is a compiled retail room-header pointer. The last
argument is 1..5000 neutral frames. A fifth hexadecimal argument can hold the
same 16-bit SNES controller word throughout the probe. For example, `0040`
holds X (the default fire button):

```powershell
dotnet run --project csharp/src/SuperMetroid.Verification -c Release -- --rom-free-direct-room 'Super Metroid.smc' A59F 900 0040
```

The verifier imports a fresh temporary
installation, initializes both reference and installed games through the same
new-game room setup, and applies the same host invincibility option to both so
later frames are not disguised by matching Game Over screens. The installed
game has no cartridge allocation; every attempted cartridge read fails. Each
frame compares game state, phase and native pixels, and the temporary import is
removed on exit. This path does not depend on the version-sensitive debugger
graphs above. Its fixed-input mode checks an in-room gameplay route, but it
does not encode a full controller sequence, door transition, complete fight,
or whole-game ROM-free proof. A transition leaving the selected room fails
explicitly rather than extending this test across a room boundary.

On 2026-09-28 the direct verifier passed 900 Kraid-room frames (`A59F`),
1,000 Crocomire-room frames (`A98D`), and 1,500 Phantoon-room frames (`CD13`)
with installed cartridge reads blocked. The fixed-input variant also passed
900 Kraid-room frames while holding X (`0040`); the older focused Golden
Torizo fixture still passed its 500-frame zero-input route.
