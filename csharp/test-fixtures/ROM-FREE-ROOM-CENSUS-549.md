# ROM-free retail-room census (#549)

The 2026-09-27 all-room pass found 32 first-frame failures among 262 retail
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
