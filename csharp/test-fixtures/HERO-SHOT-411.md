# Hero shot camera lifetime diagnostic (#411)

Run `dotnet run --project csharp/src/SuperMetroid.Verification -c Release -- --hero-shots`
from the repository root with the private retail ROM available.

## Controlled setup

- Fresh retail address space per branch; no invincibility or infinite-ammo host.
- Constructed 64 by 16 block room, air except a solid vertical column at block X=32.
- Samus normal right-facing pose 1, world X/Y=128/128, zero subpixels and movement inheritance.
- Ordinary uncharged Power Beam, Shoot newly pressed and held only on frame zero.
- Actual shared projectile owner runs before actual humanoid projectile owner each frame.
- Stationary branch camera is (0,0). Following branch camera X is max(0, preceding
  projectile X minus 128), Y=0. This deliberately controls camera input; it does not
  exercise Samus movement or the camera tracker.
- Both branches use the same firing and target data. The distant terrain collision
  must turn the surviving beam into its real explosion family near world X=512.
- The upward counterpart uses a 16 by 64 block room, solid row Y=16, pose 3,
  Samus at (128,640), and fixed camera (0,512). Following camera Y is the
  preceding shot Y minus 128. All other inputs/equipment are identical.
- Camera modes 2/3 place the next shot exactly at the retained edge (X=319 or
  Y=-64) or one pixel outside (X=320 or Y=-65). These arithmetic camera controls
  deliberately include 16-bit wrap and do not claim to model natural tracking.

## Observed C# results

Zero-based frame 35: stationary-camera projectile deleted, no target impact.
Zero-based frame 61: camera-followed projectile hits the solid column and enters
BeamExplosion. The upward equivalents delete on frame 34 or hit on frame 59.
Centered-camera controls needed no production change.

## Reproduced defect #601

The edge-retained shots contact terrain at frame 61 (right) / 59 (up). Collision
moves their explosion origin beyond the retention window. Native `$90:AF00`
then executes `$90:B16A` and clears the complete slot. C# returned from the
collision branch before that check, retaining the explosion. The comparison
failed on both impact frames before the fix. C# now preserves the collision
result but performs the offscreen check after `KillBeam`, matching native order.
Both one-pixel-outside controls delete on frame 1, and exact-edge positions are
asserted on every preceding active frame.

Pinned disassembly `$90:B16A..B196` subtracts layer-1 camera coordinates and
deletes outside the signed [-64,320) window on either axis. The production
`DeleteIfOutsideMovementWindow` matches those comparisons. This code inspection
is not execution of the cartridge and does not establish the entire technique.

## Cartridge execution comparison

`movement-release/native-hero-shot-probe.h` executes original `$90:B80D` fire
dispatch and `$90:AECE` projectile processing after restoring unpatched retail
bytes. All 319 frame records across eight cases match C#: camera X/Y, world X/Y and subpixels, X/Y
velocity, type, and instruction pointer, including deletion and terrain impact.
Accepted numeric-only trace: `movement-release/hero-shot-411.csv`. No ROM,
save data, audio, or screenshots are included.

The first native fixture omitted the room's screen dimensions and level-data
byte count. The zero byte count skipped collision; that invalid capture is not
accepted. Both are now initialized explicitly in the retained probe.

Compare or rerun the accepted regression:

```
dotnet run --project csharp/src/SuperMetroid.Verification -c Release -- --hero-shots csharp/test-fixtures/movement-release/hero-shot-411.csv
```

This paired case also runs in the default suite and `--samus-projectiles`.

To regenerate, temporarily include `native-release-probe.h` followed by
`native-hero-shot-probe.h` after `state_recorder` in upstream `sm_rtl.c`. Dispatch
`DiagnosticHeroShot(argv[2], argv[3])` for `--hero-shot-probe` before SDL startup
in `main.c`, with explicit SDL error/warning dialogs suppressed on that branch.
Force Rebuild of `upstream-sm/sm.sln`, Release/x64, PlatformToolset=v145: incremental
builds did not reliably detect the external header changes. Run the native EXE
with `--hero-shot-probe ROM NEW.csv`; output refuses to overwrite existing files.
Remove only those temporary hooks afterwards. No temporary hooks are retained
in upstream sources by this change.

Pinned upstream: `578f90b3cc49557bb70060ad033bb90b8cf8ac50`.
ROM SHA256: `12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72`.

## Evidence progression

The controlled projectile comparison alone was not sufficient. The subsequent
native controller/movement/camera comparison and supplemental retail-target
check below complete the mechanical coverage; see the acceptance audit at the end.
The separately reproduced explosion-lifetime defect is #601.

## Runtime movement integration

`--hero-shot-runtime` loads Landing Site and constructs an air corridor with a
floor at block Y=32 and a solid target column X=54 (world X=864). Existing room
scroll policies and all production movement/projectile/camera owners are retained.
Samus starts (512,490), normal right-facing, and the initial camera (400,350)
settles for 64 neutral frames before firing. The test does not write camera
position again. Both branches press Shoot on frame zero; subsequent input is
neutral or Right. Neither branch holds Shoot, Dash, or uses cheats.

The full launch snapshots are identical before walking diverges. Stationary
Samus remains at X=512, camera X=512, and the shot expires on frame 54. Walking
Samus reaches X=649, camera X=553, and the shot hits the target on frame 57.
Assertions check actual target coordinates, explosion state, movement/camera
advancement, and the stationary camera remaining fixed. Both cases run in the
default suite and `--samus-projectiles`.

## Full movement/scrolling comparison and #603

`movement-release/native-hero-runtime-probe.h` executes original input, radius,
gravity, projectile, movement, animation, pose-transition and camera routines
for the same corridor. It initializes Landing Site's 9x5 scroll table and header
offsets, then runs all 64 warm-up frames and both input sequences. Accepted trace:
`movement-release/hero-runtime-603.csv`, 241 numeric-only records. All records
compare exact Samus X/Y and subpixels, pose, camera X/Y, projectile X/Y and
subpixels, velocities, type, and instruction pointer. The native harness omits
unrelated enemies, graphics, PLM execution and room-main code, rather than claiming
a complete room emulation.

Initial native setup omitted the per-frame radius refresh; that invalid result
was corrected before accepting the reference. Adding the room scroller offsets
did not resolve the remaining two-pixel mismatch. Capturing warm-up exposed its
first occurrence at frame -56: pose expansion on landing changes center Y from
493 to 491. Native changed-pose collision also writes previous Y=491; C# left
the camera checkpoint at 493. Therefore C# camera Y advanced from 355 to 357,
and stayed two pixels too low throughout both sequences.

The #603 fix records the previous-Y writes from successful terrain/solid-enemy
pose correction and crouch fallback, then applies them before camera tracking.
Only the integer Y word changes; X and previous Y fraction remain intact. The
native 241-frame comparison failed before this fix and passes afterwards. A
focused landing fixture additionally asserts fraction preservation and one-time
consumption. This does not claim to fix every other native previous-position
write outside the changed-pose collision routines.

Regeneration follows the build/temporary-entrypoint procedure above with
`native-hero-runtime-probe.h`, `DiagnosticHeroRuntime`, `--hero-runtime-probe`.
Compare using `--hero-shot-runtime TRACE.csv`; no-argument runtime mode, default
suite and projectile batch use the accepted trace. All temporary native hooks
were removed after collection. Retail block/PLM integration is covered separately
below; its controlled camera must not be confused with this controller-driven test.

## Retail Red Tower block: controlled-camera integration

`--red-tower-hero` now runs a paired target check, also in the default suite and
`--samus-projectiles`. Room $8F:A253 is loaded with its original terrain, enemy
population and PLMs unchanged. Samus starts at (116,587), standing aiming up,
ordinary uncharged Power Beam, no equipment or host cheats. Both branches run
120 Up-only frames with camera (0,160) selected before each frame, then 120
Up-only frames after setting camera (0,450). This deliberately activates upper
Rippers before settling the launch viewport; it is fixture setup, not a route.

Both branches then press Up+Shoot only on frame zero and hold Up thereafter.
The control makes no further camera writes and deletes its shot on frame 36,
leaving block (7,10) intact. The following branch sets camera Y to the previous
shot Y minus 160 (clamped at zero) before each later frame. It impacts at world
(118,174) on frame 64 and the actual PLM changes the target from ShootableBlock
to Air. Assertions cover identical launch snapshots, exact impact coordinates,
frame, target collision change and unchanged Samus health. No production fix
was needed for this target interaction.

Early exploratory standing Hi-Jump/controller sequences did not reach the
target. Several were interrupted by Rippers. Without upper activation, later
shots also consistently struck an initially offscreen Ripper at Y=317. Those
attempts do not establish a movement regression or a valid player route.
The [Red Tower guide](https://wiki.supermetroid.run/Red_Tower) mentions following
a shot with Hi-Jump, but does not specify this fixture's starting position.

This controlled-camera retail check complements the original-cartridge
projectile/window and full corridor movement comparisons above. It does NOT
execute the retail Red Tower target path in the original cartridge and does NOT
prove a controller-only climb. That limitation remains explicit; it is not a
claim that the original technique requires a full retail route test.

## Acceptance audit

#411 and parent #394 explicitly permit a faithful synthetic, room-local fixture.
The previously stated requirement for a controller-only *retail Red Tower climb*
was an additional diagnostic ambition, not part of that completion contract.
No acceptance criterion is being replaced by the easier controlled-camera test:

| Required property | Authoritative evidence |
| --- | --- |
| Pinned cartridge, not wiki-derived expected values | Original unpatched NTSC CPU routines; pinned source/ROM identifiers and regeneration steps above |
| Identical firing, with/without subsequent scrolling | Corridor frame-zero launch equality; neutral versus Right inputs afterwards; no camera writes after initial setup |
| Production input, movement, collision and camera | Original CPU routine sequence in `native-hero-runtime-probe.h`; C# `SuperMetroidRuntime.StepFrame` |
| Lifetime AND target interaction, not just no-crash | All 241 native corridor records compare 15 fields, including exact impact type/list/world position and deletion; target reached only while walking |
| Vertical and adjacent failure bounds | All 319 records across horizontal/upward shots and centered/exact-edge/one-pixel-outside camera cases |
| Deterministic setup and no cheats | Fresh address space per branch; explicit pose, position, zero initial subpixels, inputs, scroll policy and equipment in retained probes; no enemy/RNG dependency in native corridor |
| Retail interaction integration | Red Tower actual block/PLM clears only in the retained-shot branch; exact frame and world-coordinate assertions |
| Cartridge-equivalent mismatch fixes | #601 post-impact culling and #603 pose-collision camera checkpoint writes were reproduced before correction and are covered by the native comparisons |

Ready for player validation, not closure. This establishes the shared Hero-shot
mechanic on the pinned NTSC revision. It does not certify every listed enemy,
door, boss encounter or speedrun route, and makes no PAL parity claim. The retail
Red Tower check is supplemental C# integration evidence, not a second native
execution claim. No ROM, player state, screenshots or audio are published.
