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

## Still required

- Execute the same setup on the pinned cartridge and compare per-frame world
  position, camera-relative position, deletion and collision timing.
- Cover vertical shots and actual camera-following movement, including adjacent
  successful/failing boundaries and a representative room-local target.
- The issue remains open, not awaiting player validation. This diagnostic is
  intentionally opt-in until it has the native comparison required by #411.
