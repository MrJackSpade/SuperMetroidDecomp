# Held charge across doors (#552)

Affected player version: 0.1.1. Run
`SuperMetroid.DebugRunner --door-charge-audit ROM`.

This is a constructed Flyway -> Bomb Torizo door-boundary fixture, not the player's
recording or a full approach test. Charge is acquired through 100 held-Fire frames;
the real room door record publishes the boundary and the production transition
coroutine runs with Fire held throughout.

Two failures were observed sequentially before their respective fixes:

1. `LoadMoreThingsAndOpenDoor` cleared charge 100 -> 0. The room loader called a
   full host reset; the cartridge's `ResetProjectileData` ($90:AD22), called by
   `$82:E4A9`, deletes projectile slots but does not write the flare counter or
   its animation/charge-palette words. The room-specific reset now retains them.
2. With that corrected, `HandleTransition` still cleared charge 100 -> 0. Its
   destination OAM construction invokes a runtime frame with zero input. Native
   `$82:E737` runs enemy/projectile actors and drawing, not Samus weapon-input
   alpha. Normal weapon processing is now excluded by the existing transition gate.

The completed regression asserts retained charge across every transition phase,
no projectile during the destination draw/fade, resumed charge palette cycling,
and a charged projectile only after Fire is released in resumed gameplay.
Release build, full Core verification, and the three suit variants of the #551
charged elevator cancellation regression pass. Native semantics were checked
against the pinned local C translation and disassembly; this test does not claim
an original-CPU frame-by-frame capture or audio-waveform comparison.

Keep the issue open awaiting player validation.
