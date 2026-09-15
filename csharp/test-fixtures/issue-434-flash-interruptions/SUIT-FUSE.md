# Actual bomb fuse timing around suit interruption (#434)

`suit-fuse.csv` extends the native suit-flow fixture to place an ordinary bomb
through $90:BF9D, execute its real projectile pre-instruction/instruction stream,
and stop at fuse values 7, 8, 9, 10 or 11 before Flash admission and suit setup.
The port does the same with BombProjectiles.StepFrame. Neither version writes
the fuse, projectile direction, bomb-overlap result or retained spark timer.

The bomb is placed at (120,491), the suit animation's world position for camera
(0,355). Prepared ball poses and Morph/Bombs equipment permit actual placement;
the setup advances projectiles without moving Samus. Flash admission then uses
the real chord/resources, and the suit is awarded at the post-message callback
boundary. No collectible/message sequence is claimed by this fixture.

The suit suspends projectile updates. After release, the actual fuse continues
through the timer-eight overlap, rather than spawning a timer-eight bomb then.
Starting values 9..11 hit in time to replace Flash and preserve its palette;
7..8 miss that interruption window and Flash completes. Both facings and both
suits are covered: 20 cases, 440 frames each, all 8,800 checkpoints match.

As in SUIT-BOMB.md, successful cases return to normal movement and launch a
vertical spark at frame 403 with contact damage two and health 48. Negative
cases clear the Flash palette/timer instead. These properties are asserted in
both the original-CPU generator and C# comparator. No production fix was needed.

Generate: `DraygonCrystalAudit/audit.exe "Super Metroid.smc" suit-fuse`.
Compare: DebugRunner `--flash-suit-fuse-audit "Super Metroid.smc" <suit-fuse.csv>`.
Pinned ROM identity and common compared fields are documented in README.md and
SUIT-BOMB.md. Native placement/update references are pinned `sm_90.c`.
LF-normalized trace SHA-256:
`9A8A9FECE1CA70060CAD0A12C18ADB7B4BACD1EBB3BFF68B013D71501FF01A2B`.

The additional room-local integration audit loads the cartridge-authored Varia
room at `$8F:A6E2`. It breaks the real Chozo-orb PLM, advances its instruction
stream until the item is exposed, places both bomb types through the production
projectile systems, and publishes contact at that visible item block. That one
resolved-contact publication is the only synthetic seam: pickup, inventory,
Power Bomb cleanup/Flash admission, the 387-frame bank-$85 message, the
163-frame suit transformation, bomb collision, restored control, and spark
launch all execute through production runtime ownership.

Because the contact and message handoff consume two cartridge-ordered gameplay
frames before suit setup, pre-pickup fuses 10 and 11 reach the native-proven
timer-eight overlap and preserve a usable spark; adjacent fuses 8 and 9 do not.
All four cases assert Varia acquisition, completed Flash movement, normal input
return, retained/cleared palette and timer, and successful/absent vertical spark
launch as appropriate. Run with DebugRunner
`--flash-suit-collectible-audit "Super Metroid.smc"`.
