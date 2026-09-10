# Charge Beam Combo implementation evidence

Current status: shared selection/timing/weapon ownership (#416), Ice Shield
(#417), Wave Shield (#418), and Spazer Shield (#419) are ready for player
validation. Plasma Shield (#420) remains incomplete. The sections below record chronological checkpoints; later
evidence supersedes the earlier statements of missing integration.

The initial reproduction found that live C# charge reached 120 without invoking
FireSBA, and the four family pre-instructions were missing. All four held-Fire
cases left PB=2 and charge=120. That failing witness is now a passing regression
after the implementations and integration described below.

## Native dispatcher oracle

### #419 Spazer contacts across phases and acceptance

`native-spazer-contact-probe.h` extends the shared collision oracle with authored
contact ages 0/32/48/64. The cartridge allocates Charge+Spazer, executes all live
particle pre-instructions and animation for that age, then places exactly one
particle at an ordinary target or 64 pixels to its right. All other particles
are outside contact range. Each of four physical slots is tested as both hit
and miss at every age (32 cases). Native collision and the real normal shot
callback compute damage; the contacted particle then runs its own pre-instruction
to observe cleanup. Samus starts (128,128), facing right, zero subpixels, two
Power Bombs, no cheats; target health 10000, radii 16, default vulnerability.

Two original-CPU captures match byte-for-byte. Accepted CSV is archived in
`spazer-contact-419-v1.zip`, SHA-256
`7923418D79D9A222D29C7C0FB7C60B5FABE180878A8F76B34433D5C0C83C51B5`.
The shared managed contact comparator accepts it via
`--spazer-contact-audit ROM CSV`. It uses the production allocator, phase updates,
animation and ordinary enemy collision, comparing health/flash/invincibility,
projectile type/direction/handler/position/damage, count and all four post-cleanup
type words. All 32 match without production changes.

The trace confirms 300 damage at each tested stage. Ages 0/32/48 still use the
sweeping handler and a hit removes the paired slot as well; at age 64 all four
are falling and contact removes only the struck slot. Adjacent misses preserve
all four. Seeded contact positions isolate hit behavior; natural paths and their
phase/viewport transitions are established by the separate motion/render traces.

Regeneration: apply `native-spazer-contact-entrypoint.patch`, build the pinned
host, and invoke only the bounded headless
`sm.exe --diagnostic-spazer-contact ROM NEW.csv`. Temporary hooks removed.

#419 acceptance: the 32 phase-contact cases above, 1,200 Spazer motion frames
(both paths/facings, phase timing, early/late hits, natural cleanup and sounds),
the shared 5,120-frame OAM comparison (Spazer particle/trail visuals included),
64 shared contact cases and real held-Fire activation all pass on rerun. Earlier
shared integration/render/contact fixes are documented below. Evidence is for
the pinned NTSC revision, not every custom enemy callback or PAL. #419 is ready
for player validation and remains open for confirmation.

### #418 stationary/moving/turning patterns and acceptance

`native-wave-patterns-probe.h` runs original Wave pre-instructions plus projectile
animation for nine 640-frame sequences: stationary Samus at (128,128); moving
Samus on a 120-frame horizontal triangle from X=128 to 188 plus an 80-frame
vertical triangle from Y=128 to 138; and that same movement with facing toggled
left on horizontal phases 60–119. Each crosses no hit, particle-zero hit on
frame 10, and an after-expiry hit control on frame 610. Every setup allocates
Charge+Wave through FireSBA with two Power Bombs, initial facing right, zero
subpixels/RNG, no cheats. These authored target coordinates isolate projectile
homing; they do not claim to test Samus's own movement or turn-pose transitions.

Two independent native captures are identical. Accepted CSV is archived in
`wave-patterns-418-v1.zip`, SHA-256
`89890BAE1F685B2D26471915F1A5D99F21B41E3B446244E53366F7C1D482B805`.
The existing managed motion comparator now accepts this additional hash-gated
matrix through `--wave-patterns-audit ROM CSV`. All 5,760 frames match count,
cooldown, flare writes, ordered sound requests, and all four live particles'
positions/subpixels, velocities, lifetime, handler and trail clock. Both sides
run the native animation handler after each pre-instruction. Explicit assertions
also require 300 damage per live particle and zero particles starting at frame
599 (the 600th update), with early collision removing only particle zero.

Regenerate using `native-wave-patterns-entrypoint.patch` and only the headless
`sm.exe --diagnostic-wave-patterns ROM NEW.csv` entry. Temporary hooks removed.
No production mismatch was found in these additional trajectory cases.

#418 acceptance is now covered by this pattern matrix, the earlier 3,840 Wave
motion frames (both initial facings), the 5,120-frame shared OAM oracle (including
Wave particle/trail visuals and midflight equipment changes), real held-Fire
activation, and the 384-pass retail-hitbox Phantoon contact matrix. All were
rerun successfully, together with the complete 5,906-frame Phantoon audit.
The boss comparison reproduces and verifies the charged-damage fix in 03820451.
This evidence targets the pinned NTSC cartridge, not PAL or every custom boss
callback. Leave #418 open with awaiting-player-validation for player confirmation.

### #418 supporting Phantoon audit repaired

The remaining flame-helper failure was an outdated timing assertion, not failed
gameplay knockback. Pinned `$A0:9923` publishes invincibility, timer five and a
horizontal direction; it does not install the bank-$90 hurt handler. The domain
helper now checks the pending request with unchanged standing pose, then invokes
the real deferred hit-interruption consumer and requires active rightward
knockback and the right-facing hurt pose. It still asserts exact damage, touch
instruction and projectile persistence/deletion. The helper's erroneous `$A0:A306`
reference (Power Bomb processing) was corrected to `$A0:9923`.

The subsequent flame-death fixture passed null Samus to suppress contact, but
the production drop selector requires current health/ammo. It now keeps the
existing Samus outside contact range rather than removing that required state.
No production code changed in this checkpoint. `--phantoon-audit ROM` now passes
all 5,906 encounter frames, including contact and shot-destruction producers at
frame 866, four shot-response maps, combat/rage/death and Wrecked Ship activation.
This supersedes the earlier recorded final-helper failure below. #418's remaining
explicit trajectory cases are still pending.

### #418 Wave Shield against Phantoon

`native-wave-phantoon-probe.h` executes original extended collision `$A0:9B7F`
and Phantoon's real `$A7:DD9B` shot callback. It uses retail definitions and
invulnerable/full-body/eye-only extended hitboxes without ROM modifications.
The 384 passes cover all 16 particle overlap masks, those three maps, and
eye-tracking versus swooping reaction states, with four contacts per setup.
Boss health starts at 2500, reaction clock 60, accumulated round damage zero;
Samus and the boss are at (128,128), subpixels zero. Wave is allocated by the
original FireSBA with Charge+Wave and two Power Bombs, facing right. Only masked
particles overlap; others start at (1024,1024). No gameplay cheats are active.

Collision-marked particles execute their real Wave deletion pre-instruction
after each pass. Nonhits deliberately remain at their seeded positions: this
is per-particle boss-contact coverage, not a natural trajectory test. Missed
particles' natural lifetime remains covered by the separate Wave motion trace.

Two native captures are identical. Accepted CSV in `wave-phantoon-418-v1.zip`:
SHA-256 `49CBBF9D478C6F3351B62B825BB5D0B8E2093DD6822EC8D8953EAE2BB2E61675`.
`WavePhantoonAudit` loads the real boss population, seeds the matching contact
boundary and invokes production extended collision. It compares boss health,
phase, reaction timer, properties, accumulated damage, tentacle reaction marker,
particle count and all four type words after every pass.

Before the fix, 120 passes differed: particles vanished without hurting the
boss. Phantoon's private managed path used the uncharged Wave vulnerability
instead of common shot AI's charged override. It now uses the existing shared
normal-shot vulnerability decoder. All 384 passes match: a Wave particle deals
300 here; eye-tracking closes after that first accepted hit, while swooping can
accept the remaining particles on subsequent passes. No-op body hits consume
particles without damage, and the eye-only miss control remains a miss.

Regenerate using `native-wave-phantoon-entrypoint.patch` and only the headless,
dialog-free `sm.exe --diagnostic-wave-phantoon ROM NEW.csv`; compare using
`SuperMetroid.DebugRunner --wave-phantoon-audit ROM wave-phantoon-418-v1.csv`.
The temporary native hooks were removed after capture. #418 remains open until
the explicit stationary-X versus moving/turning trajectory cases are verified.

The existing Phantoon route audit also had stale audio assertions expecting
momentary requests at later route endpoints. It now observes intro/post-battle
music during their publication frames and the hit sound at the actual rage hit.
This let its missile/super-missile checks catch an intermediate implementation
error: the shared decoder must receive the captured pre-impact type, not the
slot's newly installed explosion type. That was corrected before commit.
Introduction, combat/rage and death checks now reach the final destroyable-flame
helper, which still fails its independent active-knockback expectation (health,
invincibility and knockback clock match, active flag false). This older route
is therefore NOT claimed passing. The new 384-pass boss oracle and full core
verification pass; the flame helper requires separate investigation.

### #417 full post-hit dispatch and acceptance

`native-ice-thaw-probe.h` runs original EnemyMain `$A0:8FD4` for 420 calls
after an actual Ice Shield contact. The target starts with 90 health and either
normal freezable vulnerability `$02` or direct-freeze `$FF`. Ice remains enabled,
or is removed at post-contact frame 0, 6, or 20. The actor uses the cartridge's
common RTL main/hurt callbacks and common frozen callback: no custom gameplay
AI is authored. Samus and remaining particles are moved out of contact range
after the hit. Actor properties are normal, subpixels/RNG start zero, no cheats.

The accepted v2 capture is archived in `ice-thaw-417-v2.zip`, SHA-256
`D06ECE784E8E26534709254AE384503266516EDACB6A34586D6FCBCE0D5AB8D7`;
two independent native captures match. An exploratory v1 used property `$0400`,
which skips native invincibility decrement, and was replaced before production
changes to keep this fixture focused on ordinary hurt/frozen dispatch.

`IceThawAudit` compares health, freeze clock, AI bits, flash, invincibility and
frame counter after every real managed enemy frame. All 3,360 frames failed
before the fix. The managed dispatcher fell through the ordinary RTL hurt
callback into frozen AI, prematurely clearing flash and decrementing freeze;
it also failed to count frozen AI calls in the actor frame counter. Respecting
the explicitly declared no-op hurt callback's priority and counting frozen
calls fixes all 3,360 frames. Existing custom hurt handlers retain precedence.

Regeneration uses `native-ice-thaw-entrypoint.patch`, the same pinned sources
and ROM as other captures, and only the dialog-free command
`sm.exe --diagnostic-ice-thaw ROM NEW.csv`. Managed comparison:
`SuperMetroid.DebugRunner --ice-thaw-audit ROM ice-thaw-417-v2.csv`.
Native hooks were removed after capture.

Ice Shield acceptance evidence, all rerun at this checkpoint:

| Required property | Evidence |
| --- | --- |
| Four particles through real charged input | Four-family activation-input audit; Ice activates at zero-based input frame 120 |
| Orbit, release, hit consumption and timed sounds | 3,840 Ice motion frames, both facings, early/late contact and natural release |
| Particle/trail animation and equipment changes | 5,120-frame shared OAM oracle, including 1,280 Ice frames and midflight beam changes |
| 90 damage, lethal freezing, nonfreezable and frozen targets | 96-case Ice contact oracle, including Ice disabled after allocation |
| Freeze sound and refreeze silence | Same contact oracle, exact library-three request and max-three capacity |
| Natural expiration and equipment-removal recovery | 3,360-frame full EnemyMain oracle plus 16 callback-boundary cases |

Full core verification also passes. These are pinned NTSC cartridge checks,
not a PAL claim or a claim about every enemy-specific custom shot callback.
#417 is ready for player confirmation; leave it open with the validation label.

### #417 Ice Shield contact and freeze audio

`native-ice-contact-probe.h` allocates Ice Shield through original `$90:CCC0`,
then runs collision `$A0:A143` and normal shot AI against a synthetic ordinary
target. The 96-case matrix varies health 45/90/91/180, vulnerability `$02`/`$82`/
`$FF`, Ice still equipped/removed after activation, area 0/2, and initial freeze
clock 0/20. Charged vulnerability is two. Only particle zero overlaps; all other
particles are moved outside the target. Position is (128,128), subpixels zero,
facing right; no random or cheat behavior is involved. Definition and vulnerability
patches exist only in the disposable in-memory cartridge.

`IceContactAudit` runs the real managed allocator, animation, enemy collision,
normal damage and particle pre-instruction. It compares actor survival, health,
freeze clock, AI bits, invincibility, projectile damage/direction, library-three
freeze request and post-contact particle count. Dead actor fields are normalized
to zero, not treated as proof of stale-memory parity. Managed freeze requests
must also use max-three capacity and occur at most once per hit.

Two native captures match byte-for-byte. Accepted CSV is in
`ice-contact-417-v1.zip`, SHA-256
`5007128C0E926474328C3EABB2F9BE3B8E73D02DE877B383E627E91B4BB05CD3`.
All state fields matched before the fix, but 24 cases lacked native freeze SFX
`3:$0A`. The shared freeze helper now queues that sound only when the prior
freeze clock was zero; direct `$FF` refreezing remains silent. All 96 cases pass.
The helper's area comment was corrected from Maridia to Norfair (area two),
without changing its already-correct area selection.

Regenerate with `native-ice-contact-entrypoint.patch` and the headless native
command `sm.exe --diagnostic-ice-contact ROM NEW.csv`; compare with
`SuperMetroid.DebugRunner --ice-contact-audit ROM ice-contact-417-v1.csv`.
Temporary native hooks were removed. This verifies the shot boundary, not the
subsequent multi-frame hurt/frozen dispatcher sequence. That sequence remains
to be checked before #417 is marked ready for player validation.

### #417 common frozen-handler boundary

`native-frozen-ai-probe.h` executes original CPU code at `$A0:957E`, with
the same retail ROM and pinned host/disassembly sources used below. Its 16
constructed actor records cover Ice enabled/disabled, freeze clocks 0/1/2/400,
and frozen-only versus hurt+frozen AI bits. Each starts with flash clock 18.
Two independent captures were byte-identical. Accepted CSV is archived in
`frozen-ai-417-v1.zip`, SHA-256:
`CF8B8D22622DBD7463A79CAC01F5EFEE3ABD6E1B9B42D0C9EF6EB514A494376F`.

`FrozenAiAudit` runs the production enemy frame with the same synthetic actor
and compares freeze clock, AI bits, and flash clock. Seven cases failed before
the fix: C# cleared frozen dispatch on the decrement-to-zero frame instead of
the following call, and thaw wrote literal zero instead of the remaining AI
bits into the freeze-clock word. All 16 now match. The native callback clears
flash first, so subsequent managed flash housekeeping cannot change the fields
being compared. This is a callback-boundary regression, not a claim that every
constructed hurt+frozen combination is reachable by Ice Shield gameplay.

Reproduction: apply `native-frozen-ai-entrypoint.patch` to the pinned native
host, build Release x64, then run only
`sm.exe --diagnostic-frozen-ai ROM output.csv`. This entry bypasses SDL and
suppresses error/warning dialogs. Remove the temporary entrypoint afterward.
Managed comparison:
`SuperMetroid.DebugRunner --frozen-ai-audit ROM frozen-ai-417-v1.csv`.
The remaining Ice Shield damage/contact sequence and presentation acceptance
work is still open in #417; this checkpoint does not complete that ticket.

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

## Bomb / Power Bomb admission checkpoint (#416)

512 original-CPU $90:BF9D cases cover selected normal/PB, bomb counts 0/1/4/5,
cooldown 0/1/2/$0100, ammo 0/1, Shoot edge absent/present, Bomb equipment
absent/present and an existing armed Power Bomb. Charge is zero, as the combo
particle updates leave it. Compare aggregate bomb count, complete cooldown,
PB ammo, HUD selection, armed state and all five bomb-slot type words.

All 512 cases match the existing managed producer without a gameplay change.
The producer is exposed internally only so the focused test invokes the real
allocation path without unrelated bomb movement or explosion updates.

Confirmed: a first bomb is admitted even with cooldown two, while a further
bomb is blocked by that cooldown until the active one clears. Selected PB
bypasses the Bomb equipment requirement, respects the armed flag, and consumes
ammo only after admission. Forced selection of an empty PB class retains the
native helper's earlier count/cooldown side effects. This evidence does not
justify adding a special blanket prohibition on bombs during combos.

- `bomb-admission-416-v1.zip`, CSV SHA256:
  `3D384C192B95B243BE8E02EE4AF5B07373F404439815711854E8F83B2886EC62`.
- Same pins; two identical original-CPU captures.
- `native-bomb-admission-entrypoint.patch`; bounded/dialog-free invocation:
  `sm.exe --diagnostic-bomb-admission ROM NEW.csv`.
- Managed: `--bomb-admission-audit ROM CSV`.
- Temporary hooks removed and patch reapplication checked.

Grapple admission and integrated input/particle sequencing remain for #416.
These isolated producer cases alone do not establish the full-frame contract.

## Grapple admission checkpoint (#416)

32 original-CPU $90:DD3D cases start with each of the four allocated combos,
Grapple selected, cooldown zero/two, and current/previous Shoot edge independently
absent/present. The HUD path reaches bank $9B without consulting ordinary
projectile capacity or cooldown. Managed admission matches firing state, endpoint
position, projectile count/types, PB ammo and unchanged cooldown in every case.

Four additional runtime sequences hold Shoot for 121 frames to activate each
family, press Select normally (no diagnostic Grapple flag), then press Shoot.
They verify successful Grapple firing at the native standing-origin offset,
four retained combo particles with unchanged types, and no additional PB debit.
No gameplay correction was required for these admission/ownership checks.

- `combo-grapple-416-v1.zip`, CSV SHA256:
  `8F43C1DE13732895F6044ACCF7A14AA3B1274252E37679CDC38B962A55339CDA`.
- Same pins; two identical original-CPU captures.
- `native-combo-grapple-entrypoint.patch`; bounded/dialog-free invocation:
  `sm.exe --diagnostic-combo-grapple ROM NEW.csv`.
- Managed: `--combo-grapple-audit ROM CSV`.
- Temporary native hooks removed; reapplication checked.

This covers admission and initial ownership, not sustained grapple/combo
animation, audio ordering or every posture. Full charge release/turn/spin timing
and integrated bomb-lifetime restrictions remain before #416 acceptance.

## Combined input and lifetime checkpoint / #416 acceptance

`--combo-input-sequence-audit ROM CSV` compares 23,680 original-CPU frames of
$90:DCDD (cooldown, HUD dispatch and projectile updates together), using twelve
normal beam combinations, PB stock 0/1/2 and initial charge 119. The 160-frame
scripts cover continuous hold, twenty scripted spin frames, twenty scripted
turn frames, and release followed by recharging. Four extra scripts activate
each valid family then repeatedly attempt normal bombs every other frame.

Both sides use an empty 16x32-block room, Samus at (128,128), zero subpositions,
Bombs/Morph equipment, Charge plus the selected beam, and no cheats or enemies.
Poses are prescribed to isolate HUD admission rather than testing movement or
the duration of a physical turnaround. Native screen dimensions are 1x2, matching
the block dimensions; a rejected v2 diagnostic omitted these dimensions and
therefore skipped bomb terrain handling. Only corrected v3 is accepted.

Every frame matches charge, previous-charge sample, PB ammo, selected item,
ordinary/bomb counts, cooldown and all ten projectile type words. With initial
charge 119, ordinary hold activates at frame 1; the prescribed twenty-frame
spin/turn intervals defer it to frame 21. Release/recharge activates at frame
121. Repeated bombs agree through fuse, explosion, deletion and reallocation.

- `combo-input-416-v3.zip`, CSV SHA256:
  `CC8511EA8A9BAD9E0CBD36616D8D3CBE82A98AEE8C533A906F45B28FA70F20FA`.
- Same pins; two identical original-CPU captures; ROM hash rechecked.
- `native-combo-input-entrypoint.patch`; only bounded/dialog-free invocation:
  `sm.exe --diagnostic-combo-input ROM NEW.csv`.
- Temporary hooks removed and reapplication checked.

### Shared-ticket requirement coverage

| #416 requirement | Evidence |
| --- | --- |
| Four valid families, selected PB and one-PB debit | 432-case allocation oracle; real held-input activation; combined alpha trace |
| Charge threshold, hold/release and spin/turn delay | 23,680 per-frame comparisons plus zero-charge runtime activation witness |
| Invalid normal combinations and insufficient ammo | All twelve normal selections and 0/1/2 PB, including native forced-empty-selection behavior |
| Equipment changes preserve active ownership | 5,120-frame draw/type trace with mid-flight equipment changes |
| Beam/missile restrictions | Combined charge/particle trace and 96-case full missile-producer oracle |
| One normal bomb, PB admission and resource ownership | 512-case bomb producer oracle plus repeated bombs through expiration in the combined trace |
| Grapple allowed alongside combo | 32 native cases and four real Select/Shoot runtime handoffs |

Out-of-table Spazer+Plasma glitch loadouts remain the explicit #395–#399 scope,
not normal beam selection. Boss-specific damage, freeze/piercing particulars,
final palette/VRAM rendering and full audio presentation remain with #417–#420.
No family ticket is declared ready by the shared #416 acceptance checkpoint.
