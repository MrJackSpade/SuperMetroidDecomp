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

## Observed C# results

Zero-based frame 35: stationary-camera projectile deleted, no target impact.
Zero-based frame 61: camera-followed projectile hits the solid column and enters
BeamExplosion. No production changes were needed for this diagnostic.

Pinned disassembly `$90:B16A..B196` subtracts layer-1 camera coordinates and
deletes outside the signed [-64,320) window on either axis. The production
`DeleteIfOutsideMovementWindow` matches those comparisons. This code inspection
is not execution of the cartridge and does not establish the entire technique.

## Cartridge execution comparison

`movement-release/native-hero-shot-probe.h` executes original `$90:B80D` fire
dispatch and `$90:AECE` projectile processing after restoring unpatched retail
bytes. All 98 frame records match C#: camera X, world X/Y and subpixels, X/Y
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

- Cover vertical shots and actual camera-following movement, including adjacent
  successful/failing boundaries and a representative room-local target.
- The issue remains open, not awaiting player validation; the horizontal
  controlled-camera comparison does not fulfill the broader integration scope.
