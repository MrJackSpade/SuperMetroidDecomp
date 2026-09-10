# #416: Charge Beam Combo implementation evidence (incomplete)

The live C# firing path increments charge to 120 but never invokes the native
FireSBA dispatcher. Its projectile pre-instruction enum also lacks the four combo
families. `--combo-activation-input-audit ROM` reproduces the missing handoff for
Charge plus Wave, Ice, Spazer and Plasma with two Power Bombs and PB selected:
130 held-Fire frames leave PB=2 and charge=120 in every case. This is an explicitly
failing diagnostic, not a passing implementation test or full timing oracle.

## Native dispatcher oracle

`native-combo-activation-probe.h` calls original $90:CCC0, not the C translation.
432 cases cover all twelve normal beam combinations, PB stock 0/1/2, selection
none/PB, both facings, and slot-zero pre-instruction none/Ice/Plasma. Equipment is
Charge plus that low-nibble beam selection. Samus starts at (512,384); remaining
WRAM starts zero except auto-cancel selection three. Existing pre-instructions
are seeded handler-boundary conditions, not proof that those states can be reached
with a particular equipment/input sequence. The zero-ammo selected case likewise
tests the dispatcher, not whether the HUD permits selecting it normally.

The capture records ammo, selection, auto-cancel, projectile count, cooldown,
shared SBA state, carry and four native projectile records. Each slash-separated
record contains fourteen concatenated four-hex-digit words: type, direction,
pre-instruction, X, Y, X speed, Y speed, variable, trail timer, damage, X radius,
Y radius, instruction pointer, instruction timer.

Native observations that implementation must preserve:

- Only indices 1/2/4/8 dispatch a family; other normal combinations return clear.
- Ammo subtraction/clamping precedes family dispatch. A repeated Ice/Plasma
  attempt can debit ammo even when the existing pre-instruction rejects spawning.
- The dispatcher itself can spawn with zero ammo; do not invent a gate here.
  HUD/input eligibility needs its own comparison.
- Depletion clears both selected and auto-cancel item state.
- Wave/Spazer and Ice/Plasma have different initialization, overwrite and shared
  direction-state semantics; do not assume four identical radial projectiles.

## Reproduction

- ROM SHA256: `12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72`.
- Native host: `578f90b3cc49557bb70060ad033bb90b8cf8ac50`.
- Disassembly: `362be646929cf8e483f692b73a6561cfc2dc1d0d`.
- Archive: `combo-activation-416-v1.zip`.
- CSV SHA256: `6A50975CDAEA6A8465118CA7E27DC024BA7CB6A959DD4BDCEB26071921411CD8`.
- Two captures are byte-identical. Temporary source hooks removed; reapply checked.
- Apply `native-combo-activation-entrypoint.patch`, build, and run only the bounded,
  dialog-free `sm.exe --diagnostic-combo-activation ROM NEW.csv` entrypoint.

Remaining: implement shared activation and particle ownership/pre-instructions;
compare real-input timing including turns/spins/release and invalid selections;
test ammo limits, equipment changes and the active-combo armament restrictions.
Family-specific trajectory/damage work belongs to #417–#420, while #416 must not
be considered ready merely because allocation or this endpoint witness passes.
Reference: https://wiki.supermetroid.run/Charge_Beam_Combos (revision 7564).

## Allocation implementation checkpoint

`SamusProjectileSystem.TryActivateCombo` now implements the original dispatcher
and all four initialization families. It retains native ammo debit before family
rejection, selection teardown, fixed four-slot overwrite, family-specific data
initialization and shared direction/phase state. Addresses live in
`SamusComboRomData`; live phase and auxiliary fields live on the projectile owner
and slots. The normal firing path does **not** invoke this method yet, because
the new pre-instruction identities still need movement/lifetime implementations.
This is not a claim that any combo can be used in gameplay now.

`--combo-allocation-audit ROM CSV` compares all 432 native cases and all fourteen
captured words for each of four slots plus global resource/selection/count/cooldown/
shared-state/carry results. Zero mismatches. Build and full core verification pass.
The earlier real-input diagnostic remains intentionally failing until integration.
The capture starts other slot words at zero; stale-word overwrite cases and sound
publication still need expanded checks when particle updates are implemented.

## Ice particle update checkpoint (#416 / #417)

`StepIceCombo` implements the two native pre-instructions ($90:CF09/$CF7A),
including the two-byte sine multiply, orbit direction, 600-call phase transition,
outward radius byte arithmetic, asymmetric viewport bounds, hit-bit deletion,
trail requests, cooldown/charge writes and optional sound requests. It is still
not connected to ordinary gameplay firing while other combo families are missing.

`native-ice-combo-probe.h` allocates through original FireSBA, then executes only
the original pre-instructions in descending slot order. Six 640-frame sequences
cover both facings and no collision / slot-zero collision at frame 10 / frame 610.
Samus moves horizontally by frame modulo nine around X=128, Y=128. Every call
starts cooldown zero and a charge sentinel 77; the audio queue is drained between
frames so ordered requests are observable without audio playback timing. Fourteen
allocation words remain covered by the earlier oracle; this trace compares seven
motion words per live slot, allocation count, cooldown, charge and sound sequence.
Deleted slots are normalized to zero because this does not claim stale-field parity.
All 3,840 frames match. Trail requests execute but trail visuals are not asserted.

- Archive: `ice-combo-417-v1.zip`.
- CSV SHA256: `AFDBD584CC67F305238D796A91CBAD6BE6E088D0C40842C35323CA1EB79CDAB2`.
- Same ROM/source pins above; two independent captures are identical.
- Apply `native-ice-combo-entrypoint.patch`, rebuild, then run only the bounded,
  dialog-free `sm.exe --diagnostic-ice-combo ROM NEW.csv` entrypoint.
- Managed command: `--ice-combo-motion-audit ROM CSV`.
- Source hooks removed and reapplication checked. Build and allocation audit pass.

Still required: normal firing/update integration, visible trail/particle animation,
enemy hit/freeze/equipment-change semantics, and other combo-family updates.
Neither #416 nor #417 is ready for player validation at this checkpoint.

## Wave particle update checkpoint (#416 / #418)

`StepWaveCombo` implements $90:DA08: signed 8.8 acceleration toward Samus,
16.16 coordinate carry, the asymmetric acceleration limits, lifetime/hit deletion,
trail cadence, slot-three sign-crossing pulse sound, cooldown and charge clearing.
The original-CPU trace and managed comparison execute the projectile animation
handler after each pre-instruction. This is required for Wave trail offsets:
the first probe omitted animation and crashed on a trail lookup at frame three;
that incomplete v1 output is not an accepted oracle. The corrected v2 capture is.

Six 640-frame sequences cover both facings and mirrored horizontal target motion,
vertical target motion, natural expiry, collision at frame ten and a post-expiry
collision control at frame 610. All 3,840 frames match nine words per live slot
(the Ice trace's seven plus X/Y subpixels), count, cooldown, charge and ordered
sound requests. Deleted slots are normalized to zero. Shared `ComboMotionAudit`
retains the earlier Ice comparison instead of duplicating its fixture driver.

- Archive: `wave-combo-418-v2.zip`.
- CSV SHA256: `7D20E4D77F95D6633377648EDA456DD4D71A4286FA966572B05B23DB0B617442`.
- Two corrected captures are identical; same ROM/source pins above.
- Regenerate with `native-wave-combo-entrypoint.patch`, rebuild, then invoke the
  bounded/dialog-free `sm.exe --diagnostic-wave-combo ROM NEW.csv` entrypoint.
- Managed: `--wave-combo-motion-audit ROM CSV`.
- Temporary hooks removed and patch reapplication checked.

Still not connected to the live firing/update path. Wave enemy multi-hit/damage,
visible animation/trails and real audio playback are not established by this
motion trace. #416 and #418 remain open without awaiting-player-validation.

## Plasma particle update checkpoint (#416 / #420)

`StepPlasmaCombo` implements $90:D793 and its three phase handlers. Radius grows
by four until 192, contracts by four until below 45, then expands with viewport
deletion. Angle and radius retain the native byte arithmetic. Crucially the first
two phases do not delete offscreen rings. Hits delete before cooldown/charge
writes; final-phase offscreen deletion happens after those writes. There are no
particle-update sound requests in this family.

Six 200-frame original-CPU sequences cover both facings, moving Samus on both
axes, natural departure and hit-bit removal at frames 10/60. The native animation
handler runs after each live particle update, as in the Wave fixture. All 1,200
frames match the same nine live-slot fields and global state as Wave. Explicit
witnesses assert all four rings at radius 192/phase one on frame 37, radius
44/phase two on frame 74, and no live rings at frame 199 in no-hit cases.

- Archive: `plasma-combo-420-v1.zip`.
- CSV SHA256: `2F053D2B633D5B4A534B868D9FA7DE78AFCF9EC1D4ECC4CFC6809D2B41701EC6`.
- Same ROM/source pins; two captures identical.
- Regenerate with `native-plasma-combo-entrypoint.patch`, rebuild, then use only
  the bounded/dialog-free `sm.exe --diagnostic-plasma-combo ROM NEW.csv` entrypoint.
- Managed: `--plasma-combo-motion-audit ROM CSV`.
- Temporary source hooks removed, patch reapplication checked.

Enemy penetration/damage and visual properties are not proved by setting a hit
bit in this movement fixture. Spazer updates and shared firing integration remain
unfinished, so #416/#420 remain open without awaiting-player-validation.

## Spazer particle update checkpoint (#416 / #419)

`StepSpazerCombo` implements $90:DB06 and $90:DC9C with the three auxiliary
phases, paired-slot hit deletion, top-edge early fall, radius-zero reversal,
slot-zero transition sound, retagging the two primary particles as falling trails,
and final downward deletion. The native ordering updates cooldown/charge after
the main phase handler, but not after a falling particle is deleted.

Six 200-frame original-CPU sequences cover both facings (the left-facing cases
also use a higher Samus position to exercise the longer upper orbit), moving
Samus, and hit-bit injection at frames 10/60 or no hit. All 1,200 frames match.
In addition to the nine motion words used by Wave/Plasma, this trace includes
auxiliary phase, projectile type, instruction pointer and damage, so a position-
only match cannot hide an incorrect falling-trail slot handoff. Animation runs
after each pre-instruction in both implementations.

- Archive: `spazer-combo-419-v1.zip`.
- CSV SHA256: `73763E826E1578999042745F4312EE9F4A909778A00E8361FE4E276EAC14403E`.
- Same ROM/source pins; two captures identical.
- Regenerate with `native-spazer-combo-entrypoint.patch`, rebuild, then invoke
  only bounded/dialog-free `sm.exe --diagnostic-spazer-combo ROM NEW.csv`.
- Managed: `--spazer-combo-motion-audit ROM CSV`.
- Temporary hooks removed and reapplication checked.

All four particle families now have updates, but the ordinary firing/update path
still requires integration. Combat and visible rendering assertions remain;
#416 and #419 are not yet ready for player validation.

## Gameplay integration checkpoint (#416–#420)

The held-charge handler now reaches the allocation dispatcher, clears the flare
and requests normal suit palette restoration on successful activation. The live
descending projectile pass executes all four families and retains each particle
sound request for the frontend audio queue rather than collapsing multiple
requests into one frame result.

`--combo-activation-input-audit ROM` now exercises 130 real runtime frames per
family. All four activate at zero-based input frame 120 with four particles,
one power bomb consumed, zero charge, and their family-specific activation SFX.
Before integration the same witness reported four missing activations.

Integration exposed a real trail-parser failure: Wave's right stream executes
MoveLeftDown. Native $90:B525/$B587/$B5B3 explicitly select the destination
position array independently of the executing stream. The managed interpreter
incorrectly restricted commands to their own side. A synthetic six-case test
failed on right-stream MoveLeftDown before the fix, and now verifies all three
commands from both streams, exact sibling Y writes and instruction advancement.
The full core verification suite passes, as do allocation (432 cases) and all
four motion audits (10,080 original-CPU frames total).

This is not full ticket acceptance: input edge cases and weapon interactions,
enemy combat, visible rendering and end-to-end audio verification remain.
Issues #416–#420 stay open without awaiting-player-validation.

## Enemy contact lifecycle checkpoint (#416–#420)

The shared enemy-impact adapter used to replace combo particles with ordinary
beam explosions, shifting their position and overwriting type, damage and
pre-instruction. That bypassed each family's hit deletion, Spazer's paired-slot
deletion and Plasma's piercing lifetime. Combo impacts now retain the live
particle and apply the native collision flag instead. Ordinary enemy properties
control whether Plasma is stopped. This does not change ordinary beam impacts.

The original-CPU probe constructs a nonlethal radius-based target with default
retail vulnerability data, executes FireSBA and first-record animation, then
executes $A0:A143 and its normal shot callback. It covers all four slots of each
family, overlap/miss and Plasma-blocking/nonblocking targets (64 cases). The
initial comparison failed all 32 overlap cases. The final v2 trace also executes
the contacted particle's next native pre-instruction and records the complete
four-slot type array and count, proving deletion/pairing versus continued flight.
All 64 cases now match, including health, flash, invincibility, direction, type,
handler, position and damage at contact.

- `combo-contact-416-v2.zip`, CSV SHA256:
  `FC99218EA93F166D7FA3274A49777CDB8782D335DDB6AE48E308A70F30492AD1`.
- Same ROM/source pins as above; two captures identical. Synthetic definition
  is changed only in the disposable in-memory cartridge, never the ROM file.
- Regenerate with `native-combo-contact-entrypoint.patch`, rebuild, then only
  `sm.exe --diagnostic-combo-contact ROM NEW.csv` (bounded and dialog-free).
- Managed: `--combo-contact-audit ROM CSV`.
- Temporary native hooks removed; patch reapplication checked.

This proves the shared ordinary-target boundary, not every boss's custom
collision callback, rendered output or end-to-end sound. Those and the remaining
input/weapon interactions still block full acceptance of #416–#420.

## Complete projectile/trail OAM checkpoint (#416–#420)

`--combo-draw-audit ROM CSV` compares every emitted low-OAM byte, all high-OAM
bytes, sprite count and four particle type words over 5,120 original-CPU frames.
It covers all four families for 640 frames at two camera offsets, moving Samus,
NMI parity, natural expiry and changing the equipped beam at frame 20 without
reinitializing active particles. It executes native $93:8254, including the trail
pass; managed drawing uses the real projectile/trail OAM methods.

This reproduced 155 mismatching frames. Two fixes were required:

1. The live projectile draw path had an invented 64-pixel horizontal margin.
   Retail only checks vertical position and spritemap validity, preserving the
   offscreen nine-bit X records for PPU clipping and OAM ordering. The separate
   bomb/explosion pass retains its native 48-pixel margin. Fixing this left 40
   mismatching Spazer frames.
2. The trail-frame lookup copied upstream C's timer-one lookahead. Pinned retail
   $93:81D8-$81E3 always reads instruction-pointer minus two (the previous
   record's field six), with no timer test. Looking ahead spread the falling
   Spazer trails early. Following the ROM removes the remaining 40 mismatches.

All 5,120 frames now match exactly. This confirms active-particle ownership
survives an equipment-word change, but does not claim full pause-menu graphics
upload coverage or final framebuffer/palette parity.

- `combo-draw-416-v1.zip`, CSV SHA256:
  `C92A9E80D98A24BD0D665A9DC36E541059843B2E1B523BE84B79F9796E4F95B3`.
- Same ROM/source pins; two identical original-CPU captures.
- Rebuild with `native-combo-draw-entrypoint.patch`; invoke only bounded,
  dialog-free `sm.exe --diagnostic-combo-draw ROM NEW.csv`.
- Temporary native hooks removed and reapplication checked.
- Prior allocation, contact, motion and held-input audits still pass.

Boss-specific interactions, final rendering/audio and remaining input/weapon
restrictions are still incomplete. #416–#420 remain open without validation labels.

## Missile admission checkpoint (#416)

The full $90:BE62 missile producer is compared across selected missile/Super,
three/four/five occupied ordinary slots, cooldown 0/1/2/$0100, ammo 0/1 and
absent/present Shoot edge (96 cases). Four occupied slots specifically models
the resource pressure from an active combo. The native input boundary is
isolated from movement and particle updates; this is not a whole-frame combo
weapon-admission proof.

Eight cases failed before correction:

- Super Missiles incorrectly shared the ordinary five-projectile limit. Retail
  $90:AC86 rejects when four slots are occupied, reserving capacity for the link.
- Empty ammo incorrectly rejected before writing cooldown. Native admission
  writes one, then the empty-ammo path rolls back only the projectile count.

All 96 cases now match count, complete cooldown word, both ammo classes and all
five projectile type words. The test calls the actual managed missile producer;
only its access was widened to internal for the focused fixture.

- `missile-admission-416-v1.zip`, CSV SHA256:
  `E237C320018B963565FFC7802C220F028EB96C36DEA8B482E8D79131E39BEAC4`.
- Same pins, two identical original-CPU captures, no cheats.
- Regenerate using `native-missile-admission-entrypoint.patch`, then only the
  bounded/dialog-free `sm.exe --diagnostic-missile-admission ROM NEW.csv`.
- Managed: `--missile-admission-audit ROM CSV`.
- Temporary native hooks removed; reapplication checked.

Bomb/PB/Grapple admission and complete held/released/turning/spinning input
sequences remain to be checked for #416; this checkpoint does not close it.
