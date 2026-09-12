# Projectile velocity inheritance — reproduction and fix (#600)

At b924381c, the production beam initializer ignores movement inheritance.
Run `SuperMetroid.Verification --projectile-inheritance-probe` (or pass the switch
after `dotnet run --project ... --`). The original diagnostic exited 1; after the
fix it passes and is also included in the default verification suite.

Observed: 18 mismatches across 50 actual initializer calls. Zero-state controls
pass. Rightward movement 3.0000 plus base speed 0400 expects 0700, receives 0400.
Leftward FFFD.0000 expects F900, receives FC00. Upward FFFC.0000 expects FB00,
receives FC00. Fractional/camera-byte snapshots demonstrate the native overlapping
reads, not merely an integer-speed discrepancy. This is a synthetic production
initializer reproduction, not proof of a complete runtime movement sequence.

Pinned bank_90.asm B218..B2F5 reads words at 0DA9, 0DAD, 0DB1, 0DB5; memory.asm
defines camera Y subspeed at 0DA8 and the directional integer/fraction pairs at
0DAA..0DB9. The unusual integer-before-fraction layout is intentional to preserve:
the native unaligned loads mix fractional bytes and adjacent integer bytes.
Upward loads use BIT FF00, two logical shifts and OR C000 before subtracting base
speed. Other directions add the unaligned word with sixteen-bit wrapping.

Normal, demo and locked alpha handlers process projectiles before EB02 resets
the eight directional words (E6C0/E6C3, E70A/E70D, E719/E71C). New-state movement
then writes them. Camera Y subspeed is not part of that reset. The old production
comment describing an end-of-beta reset was incorrect and has been corrected.

Implementation must provide the actual movement write owners, not net frame
displacement. Collision wrappers publish at 9395, 93D0, 9433, 9461; no-collision
movers at 9839, 9868, 9884, 98B3. Calls can overwrite individual direction records.
The shared consumer also serves missile setup and delayed ignition. Inventory
the translated movers, reset timing, frozen/locked paths and exact-state snapshot
serialization before claiming this fixed. Add runtime-generated preceding-frame
movement tests in addition to this initialized-WRAM probe. The current probe's
independent address-space snapshots are evidence of the missing consumer only.

## Implemented fix

`SamusProjectileInheritance` reads and writes the native mutable WRAM records,
with addresses in a separate named catalog. The shared beam/missile initializer
and both ignition handlers consume its byte-exact calculation. Live horizontal
and vertical collision movers publish accepted signed displacement before slope
alignment; copied/prospective kinematics and nondirectional wall observations
do not publish. These shared movers also cover the solid-enemy clipped path,
which corresponds to the native no-collision position adders. Direction records
overwrite rather than accumulate. Runtime alpha clears them after projectile
processing, and camera tracking publishes its separate subspeed word.

No new serialized fields are needed: the existing debugger object graph already
persists the backing WRAM array. This is tested, not assumed: three controller-
driven runtime fixtures (beam, missile, Super Missile) serialize immediately after
movement and verify the same inherited launch velocity after resuming the graph.
The fixtures use a constructed clearing in Landing Site, twenty Right frames,
then Right+Shoot. Expected velocity uses the preceding frame's actual movement
record, native base speed and the applicable first-frame acceleration/ignition.

The original 18/50 mismatch reproduction now passes. Additional checks assert
negative word ordering, opposite-direction fractional leakage, repeated-write
overwrite semantics, wall-probe isolation, clipped rather than requested wall
movement, reset boundaries and retained camera bytes. These are production-path
synthetic and runtime fixtures, not a frame-by-frame external emulator capture.

Tracked in #600, related to #540/#547; player confirmation remains separate from
the automated checks. This does not complete the broader asset/lookup migrations.

Validation: original failure now passes; focused fixture additionally checks that
frozen gameplay retains directional words and input-locked alpha clears them.
All eighteen physics groups and full Release Verification pass. Windows Desktop
Release builds with zero warnings/errors. Full-run log:
`csharp/test-temp/projectile-inheritance-600-final.log` (ignored local evidence).
