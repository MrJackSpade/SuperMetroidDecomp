# Projectile velocity inheritance — failing reproduction (#600)

At b924381c, the production beam initializer ignores movement inheritance.
Run `SuperMetroid.Verification --projectile-inheritance-probe` (or pass the switch
after `dotnet run --project ... --`). This opt-in probe intentionally exits 1 until
the implementation is fixed; it is not included in the passing default suite.

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

Source audit and work remain tracked in #600, related to #540/#547. No gameplay
fix or player-validation claim is made by this diagnostic commit.
