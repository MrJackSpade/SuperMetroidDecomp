# Missile impact / offscreen deletion (#602)

The ordinary missile direction handler calls collision and then unconditionally
checks the camera-relative deletion bounds. C# instead returned immediately
after creating the explosion. This is the missile counterpart of #601, not a
change to the bounds or to impact sound/cooldown handling.

## Exact reproduction

Fresh original-cartridge WRAM; constructed 64x16-block room, solid column X=32.
Samus stands facing right at (497,128), pose 1, zero movement inheritance and
subpixels, five missiles, HUD selection 1. Shoot pressed only on frame zero.
Camera X=384 is the retained-explosion control; X=192 is the failing edge case.
Camera Y=0. No cheats, enemies, room transition or prior save history are used.

`native-missile-edge-probe.h` executes original `$90:BE62` and `$90:AECE`.
At zero-based frame 3 both missiles hit terrain at X=$0201.E000. The centered
camera keeps type $8800/list $A041. At the edge, native clears the entire slot;
before the fix, C# retained the same explosion as the centered control.

After the fix all eight frame records match: world X/Y, both fractions, both
velocities, projectile type and instruction pointer. The fixture also explicitly
asserts that only the centered-camera control retains an active explosion.

Run `--missile-edge csharp/test-fixtures/movement-release/missile-edge-602.csv`
with the Verification console. Also included in default and `--samus-projectiles`.

## Regeneration

Follow the native build/loading instructions in `HERO-SHOT-411.md`, substituting
`native-missile-edge-probe.h`, `DiagnosticMissileEdge`, and `--missile-edge-probe`.
The same pinned upstream and ROM apply. The loader restores unpatched ROM bytes;
use a forced Release/x64 rebuild and suppress explicit SDL dialogs in the temporary
headless entrypoint. Remove only those temporary hooks after capturing NEW.csv.
The checked-in trace is numeric state from this constructed fixture, not ROM or
player save data. Native comparison covers ordinary missiles here; Super Missiles
already execute their offscreen check after their collision/link handlers.
