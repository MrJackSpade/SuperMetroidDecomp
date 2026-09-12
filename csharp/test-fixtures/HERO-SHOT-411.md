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

## Still required

- Compare a representative retail target and native normal camera movement.
- #411 remains open, not awaiting player validation; the controlled horizontal/vertical
  controlled-camera comparison does not fulfill the broader integration scope.
- The separately reproduced and corrected explosion-lifetime defect is #601.

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

This closes the C# integration gap between camera tracking and projectile
lifetime; it does not claim native execution of the entire Samus/camera sequence
or a specific retail enemy/door interaction. The earlier controlled native traces
remain independent evidence for projectile processing itself.
