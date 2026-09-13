# Zebetite physical slot reuse — partial #443

Original CPU probe: `native-zebetite-slot-probe.h`, included after
`native-release-probe.h` in the native harness and dispatched before SDL.
ROM/revision provenance and unpatched-ROM loader are the same as XRAY-CHARGE.md.
No player state, cheats, or translated C spawning functions are used.

Four independent zeroed-WRAM setups occupy three of slots 0..3 with a nonzero
Ripper header, leaving one hole. Execute original $A6:FCD9, which invokes
$A0:9275 with the embedded Zebetite population record. Every hole 0, 1, 2, 3
receives $E27F, health 1000, X=824, Y=111. Other occupied headers stay $D47F.
This tests allocation/initialization only, not a full enemy frame or battle.

`--zebetite-audit ROM` now repeats those setups through the production private
spawn path. Before the fix it failed: `Native Zebetite spawn chose hole 0; C#
chose 1.` The old code used the population high-water mark instead of scanning
free physical slots. The correction scans from slot zero, preserving existing
occupied actors and allowing a just-killed primary's slot to be reused. The
four-generation progression assertion now expects that reuse instead of
encoding the incorrect monotonically increasing allocation.

The focused audit passes after the change. The native entrypoint/includes were
removed after capture. No ROM or player-state artifact is published.

This is not completion of #443: surviving linked-half follow-up shots, exact
double-kill timing, camera-gated ten-missile kills, and adjacent failing controls
still require original CPU comparisons. Do not infer those outcomes from the
allocator test alone.

## Linked-half follow-up boundary

The extended original CPU probe initializes generation one (event bit three),
spawns its linked half, sets both health words to zero (the lethal-shot result),
and executes only the primary main callback. The primary slot now contains
generation two at 1000 HP; the old secondary remains at zero HP, linked to slot
zero. Execute the secondary's original shot callback with an ordinary beam,
damage 20. Both health words become zero, and both flash timers remain zero.

The C# audit reproduces that same callback boundary, then uses the public
production projectile-hit resolver to deliver the follow-up beam. It matches
the cartridge's 1000-to-zero health transfer and unchanged flash timers. The
preceding slot-reuse correction suffices; no further production change needed.

This confirms the follow-up mechanism but not its player-accessible timing.
The probe deliberately withholds the secondary main callback, and does not
derive that scheduling from camera visibility. Full camera-gated timing and
adjacent failing input sequences remain required before closing #443.
