# Player-confirmed posture: Morph Ball only

## Latest: matched room trajectory and grounded-carry ceiling fix

The historical conclusions below are refined by `shutter-bomb-arc`: unlike the
earlier two-routine carry experiment, this comparison includes native solid
collision pose selection and pose-command side effects.

Run `Verification --shutter-native-arc` to export `bomb-arc.wram` and
`bomb-arc.csv`, then `GrapplePoseAudit/audit.exe "Super Metroid.smc"
shutter-bomb-arc`. The WRAM fixture is a minimal state assembled from the actual
room-local reproduction immediately before frame 117; it includes the complete
room collision/BTS plane, both platforms and Samus's movement state. It is not a
full emulator save. The CSV is production output, independently checked against
original ROM instructions rather than fed back into native movement.

Before the fix, the first final-frame position divergence was frame 161: native
Samus stayed at Y=71, while the port moved to Y=72. The preceding upward carry
hit the ceiling, but the no-speed Morph Ball probe omitted command $91:EFDF's
zero-speed/down-direction side effects. The next call incorrectly used another
grounding probe. The shared probe now publishes `HitCeiling` and applies those
native side effects, including for transition poses that use the same probe.

The focused `--bomb-wall` suite includes this ceiling/carry case and failed on
the missing ceiling result before the fix. Afterward, all 62 native/port frames
117..178 match X/Y position, vertical velocity, bomb direction, and platform
position/fraction. Frame 179 introduces a new bomb reaction, so the comparison
stops before it rather than copying the port's projectile result into the oracle.

The platform continues into Samus at the constrained ceiling corner even in
the native comparison. Therefore the old blanket `gap >= -1` diagnostic is not
a valid cartridge-fidelity assertion. This is a verified state-transition fix,
not proof that every reported embedding trigger has been resolved; #347 remains
open while the later bomb interaction is investigated.

The player explicitly ruled out unmorphing. The README's earlier unmorph/ceiling case is not a reproduction of their report.

`--shutter-morph-approaches` runs 288 valid room-local Morph Ball-only sequences. It combines both platforms, centered and adjoining-passage starts, repeated bombs, rolling away and rolling back. Every frame asserts that Samus's collision height remains the Morph Ball radius (7). The sweep reaches 38 pixels of overlap.

`dotnet run --project csharp/src/SuperMetroid.Verification -c Release -- --shutter-morph-repro` selects a single sequence and deliberately fails on the first overlap exceeding one pixel. It is an unresolved diagnostic, not part of the passing default suite.

Sequence: unchanged X-ray Scope room, slot 0, begin centered on the platform in Morph Ball; one neutral setup frame; Shoot every eight frames through frame 219; Right on frames 75..78; Left on frames 79..82; otherwise no direction. No Up or unmorph input. At frame 161 Samus is (371,72), pose $32, platform Y=109, and the support gap is -2. Continuing the sweep grows the overlap beyond a tile.

This supplies the first failing edge-contact assertion for the correct posture. Native movement/carry/ceiling behavior still requires comparison before applying a production change. Do not confuse this with proof that the cartridge never permits such an overlap, or with player validation of a fix.

## Native comparison result

The first excess overlap is **not a demonstrated port difference**. `GrapplePoseAudit/audit.exe "Super Metroid.smc" shutter-ceiling` executes the original $90:923F bytes for the two critical contacts. The room trace confirms platform radii (8,32) and a solid ceiling block at (23,3). Seed Samus at (371,71), radii (5,7), platform at (360,109), with that ceiling corner. Native upward external displacement -1 is blocked at Y=71. On the following zero-carry frame, the ordinary one-pixel downward probe ignores the already-overlapping platform and moves to Y=72, matching the port.

The native fixture isolates these contacts, not the whole bomb/roll sequence. It rules out treating the failing gap assertion alone as proof of incorrect collision behavior. Keep the diagnostic available, but do not change production code merely to make it green. The original player report remains open; further reproduction must establish a cartridge/port difference.

## Coupled native carry/grounding continuation

`GrapplePoseAudit/audit.exe "Super Metroid.smc" shutter-carry` now runs 64
successive calls of the original moving-up AI ($A2:EF68), followed by original
grounded Y movement ($90:923F). It starts at the ceiling contact with platform
Y=110.5 and native upward speed -0.5 pixels/frame; each pass clears external carry
before allowing the enemy routine to publish it. The native contact predicate,
platform fractional movement, and terrain/solid-enemy grounding all execute as
ROM instructions, not reimplementations.

Observed and asserted: Samus alternates Y=71/72 after the first two passes,
the platform continues moving upward, and the support gap reaches -32 at pass
62. All 64 platform positions, fractions, carry flags/displacements, and Samus
positions are checked. Thus the prolonged overlap—not just the first bad gap—
can result from native carry and grounding at this ceiling corner.

This still does **not** execute the complete bomb/input sequence or prove the
player's issue is a cartridge bug. The next comparison needs to establish whether
the port reaches this ceiling-contact state differently, or reproduce embedding
away from a blocking ceiling. Do not patch the shared collision code to suppress
this native result, and do not close #347 on the strength of this fixture.

## Confirmed adjacent bomb-jump divergence

`GrapplePoseAudit/audit.exe "Super Metroid.smc" bomb-wall` executes $90:E032
with an upward/right bomb jump against a solid vertical wall. The seed is
X=59.F000, Y=80.0000, radii (5,7), speed 2.C000 and gravity 0.4000.
Native output is X=59, Y=77.4000, speed 2.8000, bomb direction still $0803.
The side collision does not end the handler because its branch observes the
subsequent vertical collision result.

`--bomb-wall` reproduces the same condition through production movement. Before
the fix, its active-handler assertion failed: the port combined horizontal and
vertical collisions to terminate the bomb jump. It now tests only the vertical
result, and the native endpoint/velocity/handler assertions pass.

This fixes a verified bomb-jump divergence, **not the entire #347 report**.
The room-local `--shutter-morph-repro` still reaches the ceiling-corner overlap
at frame 161. Keep that issue open while comparing the earlier trajectory.
