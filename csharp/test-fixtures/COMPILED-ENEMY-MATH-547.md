# Compiled shared enemy math (#547)

## Implemented slice

EnemyTrigonometryTables contains immutable, typed samples for the shared positive
byte sine/cosine half-wave ($A0:B143, 128 bytes) and UnsignedSineTable ($A0:B7EE,
128 words). CommonMath and both Sbug vector paths now use these definitions
without runtime cartridge reads. The tables retain exact integer truncation,
including the byte table's 255 peak. No floating-point reconstruction, JSON,
asset fallback, ROM-image facade, or mutable table is introduced.

Removing instance/bus access makes the vector helpers and their pure callers
static. These signature changes do not alter their arithmetic or sequencing.
Existing independent whole/fraction negation, low-byte input truncation,
unsigned product wrapping and angle masking remain unchanged. No serialized
instance fields or save layouts change.

## Verification

Run SuperMetroid.Verification with `--compiled-enemy-sine` (also included in the
full suite). The reference reads the pinned cartridge, independently of the
compiled definitions, and checks:

- all 256 stored samples;
- all 65,536 byte angle/radius pairs, with poisoned high bytes;
- sine/cosine/negative-sine pixel and fixed-word results;
- signed and unsigned Sbug outputs in both vector phases;
- all 8,388,608 unsigned table-index/magnitude pairs under both native phase
  offsets, including 16-bit angle wrap and 32-bit signed reinterpretation.

Production helpers are invoked as static delegates without any bus parameter
or enemy-system instance. A ROM bus exists only in the independent reference.
The full Release verification suite passes on the final code, and Windows
Desktop Release builds with zero warnings and errors.

Source: pinned upstream-disassembly bank_A0.asm
362be646929cf8e483f692b73a6561cfc2dc1d0d. ROM SHA256:
12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72.

## Encounter audit caveats

Additional Climb Sbug, Draygon and Botwoon audits fail on both the changed build
and a clean detached build of ff4b0a3b, with identical diagnostics. They are NOT
passing evidence for this migration and were not weakened:

- #574: Sbug contact expects KnockbackActive immediately (health is correctly 59).
- #575: Draygon combined death assertion fails; music field is empty, death=957.
- #576: Botwoon already-defeated room expects a consumed $B797 publication.

Those diagnostics need separate investigation to distinguish obsolete fixture
assumptions from gameplay defects. The temporary baseline worktree was removed
after comparison; no historical executable remains available to launch there.

Follow-up: #574 and #575 were repaired as ordering-sensitive diagnostic checks;
#576 corrected mixed-population expectations and the wall spawner's missing
header identity. All three complete encounter audits now pass. See
BOTWOON-PLM-IDENTITY-576.md for details. The original baseline failures above
remain recorded as evidence, not silently rewritten as passing runs.

## Signed cinematic and tide slice

The compiled signed wave now reproduces all 320 words at $A0:B3C3-$B642,
including the negative-cosine prefix and the signed wave's +/-256 peaks.
Ceres approach/destruction and ending cinematic readers, plus the liquid tide
step, now consume compiled samples without runtime ROM reads. Matrix scaling,
tide arithmetic, and serialized fields are unchanged.

The regression checks all 320 words against the pinned ROM, invokes all three
production cinematic readers without a bus, and checks every 16-bit tide phase
under no tide, small tide, large tide, and both flags. These 262,144 tide cases
assert exact fixed-point offset and phase advancement, including small-tide
precedence. Invalid prefix indexes are rejected rather than silently wrapped.

Grapple migration was attempted but deliberately excluded from e695d0a5:
Program.SamusGrapple replaces several ROM samples with invented axis-aligned
vectors to isolate collision geometry. The compiled wave exposed that dependency
(first assertion: anchor Y expected 56, actual 55). These fixtures must be rebuilt
using native vectors before switching that production reader. Their assertions
were not weakened in that commit, and grapple retained its existing ROM reader.

## Grapple follow-up

The synthetic swing fixture now uses native vectors. It first passed with the
unchanged production reader and a complete ROM-derived signed wave. Production
then switched to compiled samples, and the fixture's sine region was removed
entirely. No tests need to inject engine trigonometry to arrange a collision.

The revised geometry preserves acquisition/extension, exact anchor biases,
pendulum position and art offsets, full rope/flare OAM, release velocity, six-point
collision ordering, spike damage, bounce/kick timing, growth extension collision,
wall-grab, wall-jump, and locked cancellation assertions. In particular:

- $CA connection uses (-248,-62), not (-256,0).
- $80/$81 pendulum uses (0,256)/(-6,255), not invented leftward vectors.
- $41's nearest probe is (175,136), block (10,8), not (176,136).
- $6B wall-grab probes (143,150), block (8,9); $D7 locked contact uses (7,7).

`VerifyCompiledGrappleMath`, included in `--compiled-enemy-sine` and the full
suite, independently reads pinned samples and compares 184,320 radial points
(all angles, distances 0..119, six wrap/bias anchors) and 590,080 releases (all
angle bytes and every permitted angular velocity). It checks exact coordinates,
mutated anchor biases, block-byte masking, whole/fraction speeds, direction and
deceleration mode. Production point/release delegates accept no address space.
References: $94:A957 radial helper and $9B:CA65 release in the pinned sources.

No gameplay arithmetic, pose policy, save fields, or presentation tables change.
This removes the grapple signed-table dependency, not all grapple ROM accesses.

## Remaining work

### Signed enemy/projectile caller follow-up

The shared $86:C27A speed multiplication now uses compiled signed samples for
Ridley/Mode-7/tail, Rinka, Mother Brain's rings/hand beam/baby steering, the
standalone baby/projectile systems and rainbow-beam Samus movement. The exact
unsigned product, shift and sign restoration are shared; no floating-point math
is introduced. Fly and glass prefix reads, Rio, eye-door acceleration/direction,
N00b-tube arcs, Phantoon flame components, Shaktool's common wave and Tourian
unlock particles also use compiled samples. Each retains its native indexing,
truncation, arithmetic shift and overflow behavior. Family-specific Shaktool
tables are not part of this change.

The no-bus production-delegate audit checks every speed word at every angle
(16,777,216 cases) across four production readers plus the shared catalog method;
all fly/glass samples; signed/wrapped eye and Shaktool indices; all Rio samples;
65,536 Phantoon components; and 393,216 N00b-tube arc steps, including word
wrap and retained low subpixel bytes. The full Release suite and Windows build
pass. The Phantoon diagnostic reflection callers now use static delegates after
their production helper became pure.

Additional encounter checks:

- Mother Brain glass: all 16 RNG selectors, eight shard loops/64 maps, flight,
  gravity, page deletion and sparkle lifecycle pass.
- Phantoon: 16 touch/shot cases, 48 expansion lifetimes and rain impact pass.
- The preserved, SHA-gated original-CPU Phantoon coordinate capture still matches
  all 65,536 cases after migration (no mismatches).
- Rio contact and Mother Brain phase-three recoil audits originally failed
  identically on clean 71f1c01f and the migration. Their obsolete expectations
  were subsequently repaired under #577/#578, without changing gameplay; both
  complete audits now pass. See RIO-AUDIT-577.md and MOTHER-BRAIN-AUDIT-578.md.
  The temporary baseline worktree and executable were removed after comparison.

### Crocomire projectile vectors and Shinespark echoes

Crocomire's two signed waveform reads now use compiled sine and negative cosine,
retaining the native pair of left shifts from $86:9095-$90A9. All 256 vectors are
compared against independent ROM samples. The gradient selection is unchanged,
including the ninth volley shot's intentional read past the declared gradient
table; this slice does not claim that gradient table is ROM-free.

Shinespark's $90:CC39/$CC8A radial calculation also uses compiled integer math.
All 65,536 raw angle words times all 256 radius bytes (16,777,216 vectors) match
the reference, including fractional-byte truncation, quarter-turn subtraction,
sign restoration after truncation, and wrapped negative coordinates. The tested
production delegates have no bus parameter, preventing a hidden lookup fallback.

The old Shinespark fixture injected a constant magnitude instead of stock sine.
It was changed to stock samples and passed against the original ROM-reading
implementation before the production switch. It now supplies no sine memory and
asserts the same stock positions: diagonal radius-four offsets of two pixels,
vertical departing rays with zero X displacement, and exact viewport deletion at
downward radius 96/upward radius 168 from Y=160. Existing orbit timing, slot
capacity, movement, collision and energy checks remain.

The complete Release Verification suite, clean Windows Release build and complete
Crocomire encounter audit pass. The latter includes all nine volley vectors,
mouth reactions, bridge collapse, both melting passes, skeleton wall break,
spike debris, item drop and boss completion. No serialized state layout changes.

### Bull, Yapping Maw and Shaktool family geometry

Bull and Yapping Maw now consume compiled signed 16-bit samples from
$A0:B1C3-$B3C2. These peak at +/-32767, not the shared 8.8 table's +/-256.
All 256 words match the pinned cartridge when derived by halving the compiled
unsigned positive half-wave before restoring sign. Existing high-byte multiply
and unusual negative-fraction behavior remain in the family-specific functions.

Shaktool's segment orbit now uses its own immutable 320-word catalog for
$AA:E03D-$E2BC. The authored quadrants contain asymmetric rounding (including
differences between negative-sine and negative-cosine entries), so the catalog
retains every stored word rather than approximating a scaled common waveform.

The production-path audit checks:

- 33,554,432 Bull moves: every angle/speed, two fractional origins, both axes,
  word wrap and the zero-fraction negative correction;
- Yapping Maw X/Y products for every angle word with every length low byte,
  and every length word with every angle low byte, including poisoned high bytes;
- 196,608 actual linked Shaktool placements without a loaded bus: every raw
  angle word and three preceding-segment fixed-point origins. The vector union
  also compares every one of the 320 authored samples with the ROM.

Full Release Verification, Windows Release build and the Yapping Maw encounter
audit pass (all six populations, grab/release and multipart death cleanup).
The Bull contact audit and Shaktool lethal-shot audit fail identically on clean
baseline 58b22b6f and this migration; those failures are tracked separately as
#580 and #579, respectively, and are not presented as passing evidence.
The temporary baseline worktree and binaries were removed after comparison.
The Shaktool fatal callback was subsequently repaired under #579; its complete
encounter audit and exact death-clear regressions now pass. See SHAKTOOL-DEATH-579.md.
Bull's obsolete immediate-knockback and Deleted-flag assertions were subsequently
repaired under #580 without a runtime change. Its complete audit now passes;
see BULL-AUDIT-580.md for native ordering and exact replacement assertions.

### Phantoon HDMA wave

The wave builder and its live HDMA step no longer accept an address-space bus.
They use compiled signed samples with the original nine-bit byte-offset indexing.
Odd offsets still combine adjacent sample bytes; offset 511 includes the $8B PHB
opcode at $A0:B643, which is retained as a named boundary definition rather than
silently rounding an odd phase down to a word index.

A source-literal regression reproduced a prior width mismatch before changing
production: mode 1, phase 1, amplitude 3072, base scroll zero, index 2. The native
$88:E5CF-E63E/$E65A-E6C9 byte products end with AND $FF00 / XBA, retaining only
eight magnitude bits before sign restoration. The old host multiply retained
more bits for unaligned samples. The builder now preserves the native width.
Ordinary initialization/advancement uses even phases; no ordinary-play visual
regression is claimed from this constructed odd-phase result.

Verification includes all 65,536 phase words, all 65,535 active mode words,
33,554,432 phase/amplitude products against literal byte arithmetic, and 21,504
complete short/long cycles with signed scroll wrapping and mirrored halves.
Inactive mode and wrong output lengths still fail explicitly. The existing
setup-only call, phase progression, display latch and debugger round-trip tests
now run without any ROM loaded for the wave lifecycle.

The accepted original-CPU capture (SHA256
540325129811D501D37987C45B3F3216099CBF43A6AF5A0009C0C0F97C37504A)
matches all 480 cycles/46,080 scroll words both before and after migration.
Its inputs are the normal even-phase corpus; the wider odd-phase checks above
are source-literal tests, not an original-CPU recording.
The complete Release Verification suite and Windows Release build pass.

### Ceres escape shaft rotation

The fixed $89:AD5F-$AEFC timer/sine/cosine records are now supplied by
`CeresShaftRotationDefinitions`. The 69 records are represented by their exact
signed sine ramp, authored cosine plateaus and symmetric timer list, not a
floating-point trigonometric approximation. Room main no longer reads these
records through the cartridge bus; it still needs pose data for the departure
trigger. No serialized state fields changed.

The regression compares every raw phase word that aliases an authored record
against the pinned ROM, including the wrapped $8044 reverse endpoint. It then
executes 548 matrix publications through the real room-main state using an empty
address space, comparing every frame's timer/publication/phase and every matrix
against independently read cartridge records. The existing departure admission,
input lock and dispatcher checks remain. Invalid non-record phases fail explicitly
instead of reading unrelated bank contents. This does not change or claim to fix
the separately reported elevator camera alignment.

### Shared NTSC linear enemy speeds

`EnemyLinearSpeedDefinitions` replaces the $A0:8187 common table and its $A2
mirror with exact 16.16 integer definitions: 65 records in $0000.1000 steps,
stored positive and negative whole/fraction pairs. Native byte-offset callers
retain unaligned and cross-record byte assembly. Non-record reads fail explicitly;
the catalog does not reconstruct adjacent ROM code or silently clamp indices.

The common reader and direct Cacatac, Owtch, Stoke and Ripper-family readers no
longer access these ROM bytes. Removing the bus dependency also makes their
otherwise stateless movement helpers static. No serialization fields changed.
Quadratic/family-specific speed tables remain separate work.

Verification compares all 517 complete four-byte windows against independent
ROM reads and checks the $A2 mirror. Every one of the 65 positive/negative speed
pairs passes through the four direct production-family initializers/readers with
no speed table loaded into their address space. Ripper's constructed collision
fixture now uses native parameter 16 for one pixel/frame, instead of inventing
one pixel/frame at parameter 1; its movement and collision expectations remain.
Full Release Verification, DebugRunner and Windows Release builds pass. The
entire retail Green Brinstar Fireflea audit passes, including both circle
directions, vertical oscillation, sprite animation and darkness/death behavior.

Six independent encounter audits stop at old immediate-knockback expectations;
all fail identically on clean baseline 1647ba14 and this migration. Their repair
is tracked under #581 (Stoke, Cacatac, Owtch, Ripper, Kzan, GRipper/Ripper II).
PipeBug's formation member-death alias assertion also fails identically on both
builds and is tracked separately as #582. These audits are not claimed as
passing. The temporary baseline worktree and all its binaries were removed.
PipeBug was subsequently repaired under #582: the complete audit now passes.
It exposed a real formation timer-ownership defect alongside stale death-state
expectations; see PIPE-BUG-LIFECYCLE-582.md for the pre-fix reproduction and checks.
The six contact audits were subsequently repaired under #581 without changing
production. All six complete encounter audits now pass, including additional
native death, Ice-equipment and no-op callback expectations corrected along the
way. See ENEMY-CONTACT-AUDITS-581.md for per-encounter causes and verification.

### Outstanding scope

### Shared NTSC quadratic speeds

The shared $A0:838F table is now compiled in EnemyQuadraticSpeedDefinitions.
Its 95 fraction/whole records preserve the native lost fractional carry and
both signs. Odd-byte and cross-record word reads remain exact; unsupported
out-of-table reads fail explicitly rather than inventing adjacent data.
The shared enemy reader and direct Beetom, Hopper, KiHunter, Puyo and
Yapping Maw readers no longer fetch this table from the cartridge bus.

Verification passed all 759 complete word windows, 757 displacement windows,
196,608 hop inputs each for Beetom/Hopper, 48,640 KiHunter angle inputs and
2,271 real Yapping Maw split-word additions. The full Release Verification
suite and Windows Release build passed. Complete Yapping Maw, Botwoon,
Bowling Alley Choot, Green Brinstar Beetom and PipeBug audits passed.

Puyo, Blue Hopper and Boulder contact assertions, and the KiHunter detached
wing-orbit assertion, fail identically on the clean pre-change 5c3ec7a7 baseline.
Those failures are tracked separately by #583 and #584; they are not counted
as passing encounter coverage or claimed as new regressions. The temporary
baseline worktree and its binaries were removed after comparison.
The Ki-Hunter diagnostic was subsequently repaired under #584: it now checks
the native inherited hurt hold before the first admitted orbit. The complete
encounter passes; see KI-HUNTER-AUDIT-584.md. No production change was needed.
The other three audits were subsequently repaired under #583: all complete
encounters pass after correcting deferred-contact and Puyo cleared-slot death
expectations. See ENEMY-CONTACT-AUDITS-583.md; production is unchanged.

### Remaining migration

Crocomire's $86:9059 volley gradients are compiled, including the known final
shot's out-of-table read: $86:906B-$906C is $B620, the setup JSR's first bytes.
Parameters 2,4,...,18 therefore retain the shipped ninth-shot trajectory instead
of clamping to an authored row. All selectors 0..19 (including ignored low-bit
aliases) match ROM words; unsupported later selectors fail explicitly. The
existing exhaustive angle-to-velocity tests remain, and the complete Crocomire
audit passes its volley setup, fight reactions, bridge collapse, both melting
passes, skeleton wall break, debris, drop and completion. Full Release Verification
and Windows build pass.

PuyoHopDefinitions replaces the seven $A2:9A07 records with named height,
horizontal-speed, vertical-index-delta and airborne-function fields. The live
serialized byte selector is unchanged; invalid selectors retain explicit errors.
Dropping retains its packed 8.8 constant speed rather than using hop gravity.
All 28 definition words and six real initial-hop integrations match independent
ROM references without a loaded production bus, including the half/three-quarter
animation thresholds. The complete Waterway Puyo audit passes all seven records,
six indirect functions, eight maps, 245 airborne frames, constant dropping,
terrain landing, contact and death/Grapple behavior. Full Release Verification
and Windows Release build pass.

Shot/bomb IsLiteralNoOpEnemyAi now uses EnemyShotCallbackDefinitions. The
pinned-source inventory covers 163 headers plus 221 count-prefixed hitbox lists
(309 hitboxes), yielding 80 distinct callbacks and twelve literal RTL identities.
Kraid's eight mouth rectangles and separate body geometry table are not callback
lists and are explicitly excluded. The initial parser wrongly treated the mouth
rectangles as lists; the runtime address-map check exposed that error before
verification passed. The corrected inventory asserts exact counts.

All bank/pointer pairs are checked through the production classifier against
the inventoried no-op set. Canonical multibox shortcuts remain independent:
Kraid's private RTL does not suppress scanning, unlike the two engine shortcuts.
No executable-byte classifier remains in OrdinaryCombat. This compiles supported
callback identities, not arbitrary ROM-patched entry points.

The broader retail projectile and normal-bomb audits exposed 28/44 failing room
states involving freeze deadlines, deleted-versus-cleared death expectations and
the unused area-7 room. These are tracked by #586 for baseline diagnosis and full
repair; they are not claimed passing or attributed to this migration without
comparison. The full Release suite and Windows build are the verification gates
for this scoped classification change; remaining fixed tables still keep #547 open.
The #586 failures were subsequently reproduced on pre-classifier ac700f53 and
repaired in diagnostics. Both complete audits now pass, with naturally unavailable
actors reported separately. See RETAIL-COMBAT-AUDITS-586.md for exact counts,
native lifecycle assertions and the explicitly excluded unused debug room.

Power Bomb literal-no-op classification now uses the bank-qualified
EnemyPowerBombCallbackDefinitions catalog rather than executable ROM-byte reads.
All 163 named pinned enemy headers are checked against their native first opcode;
ten unique callbacks are literal RTL. All 16,777,216 bank/pointer combinations
are checked against that inventoried identity set, including zero/common-damage
and bank-alias distinctions. Private reaction dispatch and collision preludes
are unchanged. Full Release Verification, Windows build and the room $01/$26
Power Bomb rendering audit pass. This does not claim complete combat coverage
for every actor. Shot/hitbox IsLiteralNoOpEnemyAi still reads opcode bytes and
requires its broader callback inventory before migration.

Bull's $A8:D885 maximum speeds and $A8:D895 interval pairs are compiled in
BullMovementDefinitions. All 104 authored selector combinations run through
the real initializer without a bus and match the pinned ROM words, live timer
copies, delay/function and instruction state. Out-of-range debug-edited
selectors now fail explicitly: arbitrary adjacent executable-byte reads are
not supported by this definition API. The retail Sponge Bath actor uses 3/3;
its complete seek/acceleration/deceleration, animation, contact, shot-reaction,
death, Power Bomb and Grapple audit passes. Full Release Verification and
Windows Release build pass. Serialized state and presentation remain unchanged.

The projectile copy at $A0:CBC7 is also verified identical across all 759
complete word windows. Polyp's real split-word integration now uses the
compiled definition; 570 combinations (95 records, both signs, three carry/
wrap origins) assert exact Y and final Variable1 scratch writes without a bus.
Invalid indices retain explicit failure. Rising/falling ordering and mutable
scratch remain unchanged. Full Release Verification and Windows Release build
passed. Volcano's population and natural Fune/Polyp motion checks pass before
its immediate-knockback contact assertion fails; the complete audit is not
claimed passing. That diagnostic follow-up is tracked by #585.
The #585 contact assertions were subsequently repaired without production edits;
the complete Volcano encounter now passes. See VOLCANO-AUDIT-585.md.

This is not the entire lookup-table migration. Remaining signed-table callers,
family-specific speed tables, other family tables, callback classification
reads and indirect/banked caller inventory remain. Mutable WRAM must remain
mutable, not become a compiled substitute. Wider ROM-free room/asset integration
is tracked by #530/#549. No player-visible fix or complete ROM-free gameplay is
claimed by this slice, and #547 remains open without a validation label.

## Botwoon health-phase speeds

The three NTSC movement/body-history pairs at $B3:94BB and three spit speeds
at $B3:9E77 now live in `BotwoonSpeedDefinitions`. Initialization, health-phase
updates and spit volleys consume named fields rather than reading these tables
from the cartridge. PAL values are deliberately not substituted for the pinned
NTSC revision. Path definitions, graphics and mutable body history are unchanged.

Verification compares all nine words with the pinned ROM, then calls the real
phase updater without a bus for all 3,001 health values from zero through full
health. It checks both threshold boundaries, the native zero-health phase and
phase retention while inside a hole. The complete `--botwoon-audit` passes,
including body traversal, exact spit movement, combat, death and room PLMs.
Full Release Verification and Windows Release build pass. This remains an implementation migration, not a
new player-visible behavior claim or completion of #547.

## Shared crawler and Yard speed/sign definitions

`CrawlerSpeedDefinitions` replaces the identical 32-word NTSC speed copies at
$A3:E5F0 and $A3:CCA2, preserving their gaps, repeated entries and trailing zero.
The shared crawler reset, orange Zoomer initializer and Yard velocity setup use
this catalog. `YardVelocityDefinitions` replaces the eight native sign records
at $A3:CD82 with equivalent wrapping sixteen-bit negation. Graphics, direction
instruction lists and live motion state are not replaced.

Tests compare both complete native speed copies, 128 real crawler resets and
256 real Yard resets without a bus. All 65,536 magnitudes across four property
orientations verify that the crawler preserve-velocity sentinel still applies
signs, and all 65,536 magnitudes across eight Yard directions match native
XOR/increment results. Invalid selectors fail explicitly. The complete shared
crawler and Aqueduct Yard audits pass, including movement, detachment, contact
and rendering. Full Release Verification and Windows Release build pass. Remaining family tables and the
broader #547 inventory are not claimed complete.

## Polyp launch selectors

`PolypLaunchDefinitions` replaces the eight cooldown words at $A2:B520,
sixteen initial quadratic indices at $A2:B530 and sixteen signed horizontal
velocities at $A2:B550. Each method accepts the original RNG word and applies
the native selector mask. The production caller retains three independent RNG
draws, including the cooldown draw after projectile allocation; no random state
or projectile behavior is synthesized or reordered.

All 65,536 RNG words are checked against all three native selectors, covering
every authored word and ignored selector bit. The complete Volcano audit passes
with the native three-draw launch, trajectory, cooldown underflow, contact and
rendering checks. Full Release Verification and Windows Release build pass. The remaining family-table
inventory is still open under #547.

## Shaktool angular velocities

`ShaktoolAngularVelocityDefinitions` replaces the seven angular words at
$AA:DEE9 and their all-zero initialization subtrahends at $AA:DEF7. Both the
initializer and group-target synchronization use the catalog. Live convergence
speeds, aliased segment state and instruction/graphics tables are unchanged.

All fourteen native words are verified. A constructed seven-segment group runs
the real synchronization routine without a bus across every 16-bit target,
rotating callers across all seven members and asserting every angle/velocity.
The complete Shaktool audit passes, including initialization, linked placement,
movement/reversal, contact, rendering, fatal teardown and unused attack circles.
Full Release Verification and Windows Release build pass. This does not complete the broader #547 inventory.

## Ceres Ridley getaway curves

`CeresRidleyGetawayDefinitions` compiles the 112 zoom/X/Y records and zoom
terminator at $A6:AE4D/$A6:AF2F/$A6:B00F. The irregular zoom entries and late
translation jumps are retained, not smoothed. Runtime palette and animation
artwork remain separate bus consumers. Serialized byte indexing is retained.

All 337 native words compare exactly. The full-suite Ceres fixture previously
overwrote the zoom table to terminate on the first call; it now runs all 112
real motion frames and asserts zoom and integrated offsets before checking
the terminator and warning handoff. Full Release Verification and Windows
Release build pass. The standalone `--ceres-ridley-audit` instead stops before
getaway due to its omitted required area-boss service; #587 tracks repair of
that diagnostic. It is not claimed passing. #547 remains incomplete.

The standalone audit was subsequently repaired under #587 without production
changes and now passes completely; see CERES-RIDLEY-AUDIT-587.md.

## Boyon bounce curve

`BoyonSpeedDefinitions` compiles the 23 triangular-number bytes at $A2:8701.
Native comparisons at $A2:875E/$880F/$885B explicitly saturate later indices to
$FF; the compiled sampler preserves this for every 16-bit index. Multiplication
still consumes only the multiplier's low byte. The real shared multiplication
helper and its initialization/rising/falling callers no longer require a bus.

Tests compare all stored bytes and all 65,536 index cases, then exercise the real
helper for 16,777,216 effective input pairs with nonzero discarded high bytes.
Full Release Verification and Windows Release build pass. The room audit passes
initialization, bounce and OBJ checks but stops at its unchanged immediate-hurt
assertion; #588 tracks that diagnostic repair. Complete encounter success is not
claimed, and this migration does not resolve the deferred player report #524.

The diagnostic was subsequently repaired under #588 without production changes;
the complete encounter now passes. See BOYON-AUDIT-588.md.

## Crawler slope scaling and Yard kick records

`CrawlerSlopeDefinitions` replaces the 32 adjusted multipliers at $A3:E931;
unused additive words are deliberately not consumed. Signed 8.8 tangent values
still produce full signed 16.16 products. `YardKickDefinitions` replaces all
sixteen fractional/whole pairs at $A3:D517. Only the vertical lookup selector is
capped at fifteen; horizontal displacement and native sign handling are unchanged.

All 32 slope values and 2,097,152 signed velocity/shape products match the ROM
reference. All 65,536 Yard selector inputs match both native words after the cap.
The complete shared-crawler and Aqueduct Yard audits, full Release Verification,
and Windows Release build pass. This is a mechanics-data migration, not a new player-visible fix.

## Alcoon and Fune/Namihe fireball launch records

`EnemyFireballLaunchDefinitions` replaces three Alcoon Y words at $86:9EF9
and eight left/right pairs at $86:DEB6. The Fune low-byte selector ignores its
high byte, and left speed remains stored in the projectile's Y-velocity field.
All 19 native words and 2,048 valid high-byte/selector combinations compare
exactly. Alcoon's existing selector restriction remains; Fune selectors beyond
the eight authored entries now fail explicitly instead of reading adjacent code.
Arbitrary modified populations using those values are not claimed supported.

Full Release Verification, complete Volcano audit and Windows Release build
pass. The Alcoon audit passes earlier motion/projectile checks but fails its
unchanged immediate-contact assertion; #589 tracks that diagnostic repair.
No full Alcoon encounter success or completion of #547 is claimed.

The Alcoon diagnostic was subsequently repaired under #589 without production
changes; its complete encounter now passes. See ALCOON-AUDIT-589.md.

## Rio family launch constants

`RioLaunchDefinitions` removes runtime launch reads from ordinary, Norfair and
lower Norfair Rio. Seven native words compare exactly, including ordinary Rio's
NTSC-specific Y magnitude. All 65,536 Norfair RNG values retain the native
single-bit vertical selector; horizontal facing/sign handling remains unchanged.

Full Release Verification, complete ordinary Rio audit and Windows Release
build pass. Both Norfair audits pass earlier launch checks but fail their direct
timer-injection freeze fixtures (parent/follower 0/0 rather than 9/9); this is
tracked separately for diagnostic repair. Neither complete Norfair encounter is
claimed passing. The wider #547 migration remains open.

Both complete Norfair encounter audits subsequently pass after diagnostic-only
repairs in #590; see NORFAIR-RIO-AUDITS-590.md.

## Boyon initialization and Boulder bounce indices

BoyonSpeedDefinitions now also owns the eight multipliers and nine jump heights
at $A2:86DF/$86EF. All 72 valid combinations run through the actual initializer
without a bus and compare both fields to the ROM. Existing invalid-selector
rejection remains tested. No bounce timing or byte-width arithmetic changes.

BoulderBounceDefinitions owns all three words at $A6:86EF. Native $88EA indexes
from table+2, so a zero remaining-bounce count still reads the leading zero word
before underflowing to $FFFF and entering rolling. Three constructed solid-floor
collisions exercise the actual falling routine without a bus, checking velocity,
counter, phase and final Y/subpixel handoff. Unsupported counter values now fail
explicitly rather than reading adjacent code; arbitrary modified states using
those values are not claimed supported.

Full Release Verification, both complete Boulder/Boyon retail audits, and the
Windows Release build pass. This removes mechanics-data dependencies, not a new
player-visible behavior fix; #547's remaining inventory and integration remain open.

## Zoa horizontal velocities

ZoaSpeedDefinitions replaces the live $A3:B415 whole/fraction reads with the five
NTSC records. All 17 complete four-byte windows compare to ROM, preserving odd
byte offsets instead of assuming aligned selectors. The actual shooting routine
runs 655,360 signed/subpixel integrations without a loaded bus (same animation
already installed), testing both directions and every fractional starting value.
Out-of-range/incomplete windows now fail explicitly; arbitrary modified states
reading adjacent executable data are not supported. Animation assets still use
their separate ROM pointer table and are outside this velocity migration.

The complete Butterfly Zoa audit reproduced an obsolete immediate-knockback
assertion after all its movement/rendering checks passed. It now uses the shared
native contact helper: exact 15 damage, pending timer/side, unchanged pose and
position, frozen-time rejection and single later movement admission. No contact
production code changed. The full encounter passes three retail actors, both
directions, wake/rise/launch/reset, four active speed stages, six maps and OBJ
drawing. Full Release Verification and Windows Release build also pass.

## Growing-shutter initialization

GrowingShutterDefinitions replaces the four initial function pointers at $A2:EA4E
and 24 split whole/fraction speed records at $A2:EA56. The speed selector still
ignores parameter two's high byte. All 52 native words and 24,576 actual
initializations compare without a bus, including four dispatch selectors,
every ignored high-byte value and both directions' wrapped section origins.
The initial direction word is still cleared after origin calculation. Speeds
outside the 24 authored records now fail explicitly rather than reading adjacent
code; arbitrary modified populations using those values are not supported.

Full Release Verification, complete shutter audit and Windows Release build pass.
The encounter audit covers all 33 named retail records, all four constructed
growing selectors, 40-pixel/four-map growth, vertical and horizontal variants,
rider carry, triggers, combat callbacks, sound gates and live OBJ. This remains
partial progress toward #547, not completion of the broader asset integration.

## Intro egg fragment and slime motion

IntroEggMotionDefinitions replaces five velocity tables in the real fragment/drop
steppers. The irregular negative prefixes are retained; slime gravity selects by
actor parity, not animation-frame parity. Initial fragment positions and visual
instruction/spritemap data remain separate ROM-backed presentation dependencies.

The first full cinematic run reproduced an important table-end assumption failure:
fragments zero/one survive past the 38 authored Y records and read three adjacent
instruction-byte pairs at $8B:AA9A-$AAA5. Native integration yields Y=$9E53, $B9D0,
then $52EB before the signed ground test deletes them. These exact pairs are now
explicit compiled data; neither clamping nor execution of arbitrary code is used.

All 366 native words compare, including those six overread words. Forty actual
actor lifetimes (six fragments/four drops at four subpixel starts) verify 2,482
frames of exact whole/fraction X/Y against independent ROM integration, through
ground impact and deletion. A bus guard rejects all old velocity reads, including
the overread range. Slime's post-impact frames retain the same frozen coordinates.
Unsupported frames beyond the authored/observed ranges fail explicitly; arbitrary
relocated debug actors surviving longer are not claimed supported.

Full Release Verification (including complete intro progression/render captures)
and Windows Release build pass. The wider #547 dependency audit remains open.

## Power Bomb renderer profile

PowerBombShapeDefinitions replaces the renderer's two fixed 32-byte unscaled
curves at $88:A266/A286. Both accelerating phases retain unsigned 8x8 truncation,
inclusive band overlap and the final center fill. Pre-scaled yellow/white shape
records remain separate presentation reads, not silently replaced here.

All 64 bytes compare to ROM. An independent native-style band-fill reference
checks 197,120 signed scanline/radius/phase cases against the real renderer helper.
Forty-five complete color-math frames check every pixel across five radii and nine
on/off-screen origins, including HUD preservation and clipping. A rejecting bus
proves the migrated path makes no ROM accesses. Full Release Verification and
Windows Release build pass.

Inspection also identified the shared absolute tangent table still read by X-ray,
eye windows and Mother Brain's rainbow beam; the remaining inventory now lists
those consumers explicitly. The inventory is still not exhaustive and #547 stays open.

## Shared absolute tangent

AbsoluteTangentDefinitions compiles all 129 native words at $91:C9D4 from the
exact quarter-table and its mirror, retaining the $3C00 horizontal substitute
and inclusive index-128 zero. X-ray rendering, scanner-eye window generation and
Mother Brain rainbow HDMA now use it. Their distinct angle folding remains at
the callers; unsupported indices past the native endpoint fail explicitly.

Every native word and each direct reader endpoint compares, including Mother
Brain's byte wrapping. Tests cover 768 wrapped/cardinal X-ray directions and 520
actual window builds with tangent-table reads forbidden. Existing eye-window
pixel tests and Mother Brain's exact apex/split/color/capture checks also pass.
The latter fixture now selects actual native unit gradients via a 64-angle-wide
beam instead of injecting artificial table words, retaining its precise indirect
split assertions. Narrow quadrant tests use their actual native gradients.

Full Release Verification passes; after the fixture refinement, the complete
focused Mother Brain suite and Windows Release build pass. More indirect
definition readers remain: the inventory explicitly lists Torizo, Chozo statue
and Ceres Ridley examples. This does not complete #547 or its shared integration.

## Golden Torizo walking and Chozo carry motion

Pinned-source inspection corrected misleading catalog names: $AA:D59A contains
twenty walking displacements (not four jump velocities), and $AA:E630/E670/E6B0
contains statue velocity and carried-Samus joint offsets (not projectiles).
GoldenTorizoWalkDefinitions and ChozoCarryMotionDefinitions now own these values;
the reference catalog names/size comments were corrected too.

Tests compare all 39 complete Torizo byte windows, all 96 Chozo words and 52 real
movement/carry calls in a constructed room without a bus. They assert signed
movement, fractional preservation, instruction handoff and actual Samus positions.
Chozo retains its even-offset validation. Torizo windows outside the authored
range now fail explicitly rather than reading adjacent code.

The full Golden Torizo audit reproduced two stale fixtures: immediate knockback
and null Samus during projectile-drop selection. Shared assertions now verify
exact 160-damage pending contact and later admission. The shot-response fixture
retains Samus's inventory while moving her out of collision range. No production
contact or drop behavior changed. Its complete encounter passes through attacks,
weapon reactions, damage phases, death, drops, music and boss-bit handoff.

Full Release Verification, complete Chozo statue audit, repaired complete Golden
Torizo audit and Windows Release build pass. The broader #547 work remains open.

## Ridley target-seeking inertia

Both 16-byte inertia tables ($A6:D61F and $A6:D712) now use the shared
RidleyInertiaDefinitions catalog. The real Ceres and Norfair two-axis helpers
no longer need a bus. Acceleration, reversal, minimum quotient and clamps are
unchanged. Verification compares all 32 authored bytes plus the adjacent byte
used by the existing Norfair death caller, and runs 2,162,688 production calls
covering every signed distance, wrapped target subtraction and rotating velocity
and reversal-boost boundary inputs without an address space.

The complete Norfair audit caught the death caller's index 16 before this change
was committed. Its prior read was the $B9 opcode at $A6:D62F, not an authored
inertia record. NorfairDivisor explicitly preserves that byte; Ceres still bounds
its own 16 entries. This migration does not claim that the existing death caller's
selector is itself cartridge-correct; that needs separate routine-level review.

Full Release Verification, complete Ceres/Norfair Ridley audits (including 896
Norfair death frames), and Windows Release build pass. Remaining indirect boss
mechanics groups are listed in LOOKUP-MIGRATION-REMAINING-547.md; #547 stays open.

### Follow-up: #591 corrects the death caller

The separate routine-level review proved the index-16 read was a port argument
mix-up: $C601 supplies Y=0 and A=$10, not Y=$10. A regression failed before the
fix. Both death phases now use index zero and reversal boost sixteen; the
temporary adjacent-byte compatibility entry was removed. See
RIDLEY-DEATH-ACCELERATION-591.md for reproduction and verification evidence.

## Phantoon figure-eight speed controller

The fourteen words at $A7:CD73..CD8D now have domain-named compiled definitions:
slow/fast fractional and whole acceleration, plus the three forward/reverse caps.
Both real adjustment routines are bus-free; their integer add/subtract, signed
wrapped comparisons, fractional resets and phase transitions remain unchanged.

Verification compares every native word and runs 5,898,240 real updates: both
directions, all 65,536 whole-speed words, nine fractional boundary values and
five phase words (including odd aliases and FFFF wrap). It independently computes
the expected tuple from the ROM records using wide arithmetic. Full Release
Verification and Windows Release build pass.

The complete Phantoon audit was also run and exposed a separate stale diagnostic:
rage-direction checks expect PAL +/-3 while the pinned ROM uses NTSC +/-2.
That audit is not claimed passing here; its repair is tracked separately. Attack
angle/timer tables and broader #547 integration remain unfinished.

## Phantoon flame spawn definitions

Compiled all 33 authored bytes: sixteen rage start angles ($86:98B4), nine rain
columns ($86:98F7) and eight spiral start angles ($86:9979). The real projectile
initializer uses the named catalog while retaining header/list loading and the
native packed rain delay. The catalog rejects out-of-range selectors; inspected
retail producers use only the authored ranges (rage 0..15, rain 0..8, spiral 0..7).

Verification compares every byte and runs 507 real initializers: every valid rage
and spiral selector, all sixteen rain delays across all nine columns, and casual
flames, each at zero, normal and wrapping origins. Assertions check exact angle,
X/Y placement, direction/delay, radius and fractional positions. A forwarding bus
rejects reads from all three migrated ranges while allowing real header reads.

Full Release Verification, the complete 5,906-frame Phantoon audit and Windows
Release build pass. The earlier audit region mismatch was separately repaired in
#592. Remaining timer/placement/schedule/direction consumers are still inventoried;
this slice does not close #547 or claim the entire projectile path is ROM-free.

## Phantoon timer definitions

Compiled the three eight-word tables at $A7:CD41, $CD53 and $CD63 into
PhantoonTimerDefinitions. All four live consumers now use them: eye opening,
second-round selection, first-round NMI selection and completed fade-to-rain.
The original RNG masks, NMI mask and number of RNG calls remain at their callers.
The old inline eye/rain timer addresses were removed from functional classes.

Verification compares all 24 native words, then executes each of the three RNG
consumers for all 65,536 input words (196,608 calls), checking duration, relevant
phase handoff and exactly one RNG call each. All 256 first-round NMI bytes also
exercise the actual phase routine and verify its first-four-entry selection and
direction RNG consumption. No bus is attached to these production-call fixtures.
The fade fixture starts at completed fade, so it verifies the timer handoff rather
than claiming independent palette-transition coverage.

Full Release Verification, complete 5,906-frame Phantoon encounter audit and
Windows Release build pass. Rain placement, random direction and mouth schedules
remain live readers; broader #547 remains open.

## Phantoon indirect mouth schedules and #593

Compiled all four reverse-read casual-flame patterns (30 words behind $A7:CCFD's
pointer list). Native cross-check caught pre-existing timer/count zero-branch
mistakes in the port. A failing regression proved the exhausted count mismatch;
both branches now follow $CFCA..D03E and select the inter-pattern delay correctly.
See PHANTOON-CASUAL-SCHEDULE-593.md for the corrected interpretation and evidence.

Verification covers every native word, 65,536 real RNG selections and 8,192
frame-exact timer/count/mouth-animation checks without a bus. Full Verification,
complete Phantoon encounter and Windows build pass. Placement and random-direction
readers remain; this does not close the broader migration.

## Phantoon rain placement and shot markers

Compiled the eight rain-placement records ($A7:CDAD: movement cursor, X, Y),
eight first-column bytes ($A7:CFC2), and eight shot eye-variable-B bytes
($A7:CDA5). The latter has no known native reader; the catalog deliberately calls
it a marker rather than assigning unproven direction semantics. Unused zero
record padding is verified but not carried as mutable state.

Verification checks 32 native record words and 16 bytes. It executes 2,048 actual
hidden-rain handoffs (all eight patterns across every RNG high byte with low-byte
mask noise), checking body cursor/X/Y, phase, direction reset, one RNG call, and
all eight resulting projectile X/Y/delays in order. The forwarding bus rejects
the migrated ranges while allowing real projectile headers. Another 65,536
actual shot reactions run with no bus, asserting the exact eye marker, selected
mouth pattern, window shortening and one RNG call.

Full Release Verification, complete 5,906-frame Phantoon encounter and Windows
Release build pass. A subsequent direct-reader inspection confirmed the 534-pair
figure-eight displacement path at $A7:E3D2 remains live; it is now listed in the
remaining inventory. This migration is not a claim that all Phantoon reads or
the broader #547 integration are complete.

## Phantoon discrete movement path

The 534 signed X/Y pairs at $A7:E3D2..E7FD are represented losslessly as numeric
keypad directions in PhantoonPathDefinitions. This is an authored discrete path,
not a sine/ellipse approximation; irregular diagonal steps are retained exactly.
The real figure-eight stepper now runs without a bus and uses the catalog length
for forward/reverse cursor wrap. Other acceleration and integration ordering is
unchanged.

Verification compares all 1,068 native bytes and executes 25,632 real movements:
every starting cursor, both directions, eight whole-speed inputs and three world
origins including wrap boundaries. Expected X/Y/cursor comes from the original
ROM bytes; exact whole speed and preserved fractional positions are asserted too.
Existing exhaustive acceleration tests independently cover speed/phase behavior.

Full Release Verification, the complete 5,906-frame Phantoon encounter and Windows
Release build pass. A direct-reader scan finds death-explosion position/type/delay
records still live; palette, eye-sprite and sound selectors also remain for their
respective presentation work. The remaining inventory reflects that distinction;
neither all Phantoon readers nor the broader #547 integration are claimed done.

## Phantoon death explosion schedule

Compiled the thirteen four-byte records at $A7:DA1D..DA50, including signed offsets,
explosion animation parameters and delays. The native loop bounds (thirteen
records, repeat from five, three passes) are named definitions. Runtime request
ordering, allocator behavior, sound markers and phase handoff remain unchanged.

Verification compares all 52 bytes and runs six complete frame-exact sequences:
three body origins including word wrap, each with either available or permanently
full projectile slots. It asserts every timer tick, index/repeat count, request
and successful-spawn count, exact final phase, selected sound marker, and actual
projectile X/Y and ROM animation list. All 29 requests still advance under full
allocation pressure. The forwarding bus forbids migrated schedule reads, while
real projectile headers and animation definitions remain in use.

Full Release Verification, complete 5,906-frame Phantoon encounter and Windows
Release build pass. The inspected Phantoon mechanics-table group is migrated;
palette, eye instruction-list and materialization-audio selectors remain for
presentation integration. Other enemy families and full #547 acceptance remain
unfinished, so this does not close the issue.

## Shared slope heights

Compiled all 512 bytes at $94:8B2B..8D2A into SlopeHeightDefinitions. Samus alignment,
enemy floor/ceiling alignment and collision, missile point collision and bomb-spread
collision now share the authored profiles without cartridge reads. BTS mirroring,
five-bit masking, strict versus inclusive collision comparisons and integer Y
adjustment remain caller-owned and unchanged. Enemy helper instance signatures
are preserved to avoid disrupting transitive reflection-based diagnostics.

Compared every byte with the pinned cartridge, then exercised 12,288 Samus samples
and 55,296 non-square point/orientation cases through the actual missile (both
axes), bomb-spread and enemy-alignment routines with ROM access forbidden. Tests
assert collision admission, exact whole Y and retained fractional Y. All shapes,
both mirror bits and every pixel are covered; Samus samples also vary world high
bits. Native enemy alignment arithmetic was cross-checked against A0:C8AD.

Existing synthetic terrain tests previously injected invented shape-$12 heights.
They now use the actual height-eight sample at X nibble eight, preserving their
exact correction, grounded displacement and fractional-movement assertions.
The redundant projectile test's copy of the native profile was removed.

Full Release Verification, the retail shared-crawler audit (Sciser, Viola, Zeela,
Sova across four rooms) and Windows Release build pass. A reader scan finds only
reference declarations for the migrated addresses. Square-slope definition tables,
Samus horizontal multipliers and broader ROM-free integration remain unfinished;
#547 remains open.

## Samus grounded slope speed multipliers

Compiled the 32 effective multiplier words at $94:8588 + 4*shape into
SlopeSpeedDefinitions and removed the production read from SamusSlopePhysics.
The interleaved speed-modifier words are not copied: inspection of $94:84D6..8585
shows that their add/subtract results are discarded on every path, with no state
writes. The native C translation likewise retains only the effective multiplier.

Compared every multiplier with the pinned ROM. Exercised the real production
scaler for all 65,536 middle-word operands in all 32 shapes, with four independent
high-byte/sign boundaries (8,388,608 cases). Expected results come from explicit
native byte packing, modular negation and wide unsigned multiplication, not the
production helper. Another 26,112 cases cover all BTS bytes, sign/truncation
boundaries and zero/nonzero whole and fractional vertical speeds. A throwing bus
proves these paths no longer read cartridge data. Existing real collision and
grounded-movement fixtures now use compiled native data rather than bus seeding.

Full Release Verification and Windows Release build pass. The reference-address
declaration remains for diagnostic comparison; no runtime multiplier reader
remains. Separate square-slope tables and the wider #547 inventory/integration
remain unfinished.

## Square-slope quadrant definitions

Moved the existing embedded Samus and enemy tables into SquareSlopeDefinitions,
then replaced the remaining missile ROM read with the same catalog. Samus body
collision and Grapple release retain their quadrant tests and clipping writes;
enemy collision preserves bit-seven tests and the native low-bit identity encoding.
No collision selector, movement state or animation ordering was changed.

Verified all 60 bytes across native $94:8E54, $A0:C435 and $86:8729 and confirmed
the distinct encodings have identical solidity. Exercised 10,240 actual missile
point cases through both horizontal and vertical paths: every pixel of every
square shape, all BTS orientations and the unused bit-five aliases. Expected
solidity comes from geometrically mirrored coordinates and original ROM bytes;
the production path receives a bus that throws on every access.

Full Release Verification, Windows build and the four-family retail shared-crawler
audit pass. Existing full-suite square-body fixtures retain their exact accepted
displacement, fractional clipping and downward support-latch assertions. The
remaining inventory no longer lists these square tables as ROM readers. This
does not complete the broader #547 caller inventory or integration contract.

## Bomb/Golden Torizo initialization records

Compiled the twelve native words in six parallel two-entry tables at
$AA:C95F..C976 into typed TorizoInitializationDefinition records. The real shared
initializer now selects the record by its existing validated enemy identity.
Positions, initial instruction pointer, property OR mask and both hitbox radii
are unchanged; palette setup and instruction execution are not part of this move.

All twelve words match the pinned ROM. The real initializer executes 131,072 times
with every possible incoming property word for both variants and a bus that
rejects all access. Assertions cover positions, radii, instruction/timers,
property/extra-property preservation, unchanged health and fractional position,
and initial falling velocity. Both defeated early returns preserve placement and
instruction while marking deletion. Native source cross-check: $AA:C87F..C95E.
This proves the migrated record behavior, not every branch of the original init
(for example the native Golden Torizo controller-code branch is outside this test).

Full Release Verification, Windows Release build and Golden Torizo encounter audit
pass. Bomb Torizo's audit first exposed its stale immediate-knockback expectation;
the separately tracked #594 diagnostic correction makes that complete encounter
audit pass as well. No gameplay contact behavior was altered to satisfy it.

Remaining inventory corrected: the supposedly Ceres-debris tables $86:A2D6/A2E2
actually belong to gunship liftoff dust. They remain live readers and are not
claimed migrated by this change. Broader #547 acceptance remains incomplete.

## Gunship liftoff dust spawn definitions

Compiled the six signed X offsets and six initial list pointers at $86:A2D6..A2ED,
plus the native Y-offset immediate from $86:A2C4, into GunshipDustDefinitions.
The real spawn routine retains validation before allocation, native reverse slot
allocation, header initialization, zero velocity/fractions and initial timer one.
Renamed/moved the reference constants from the misleading Ceres/FallingDebris
group to Gunship.Dust members; the reference-range diagnostic follows that rename.

All twelve record words match the pinned ROM. Exercised 393,216 real spawns:
six parameters times every 16-bit X/Y origin, including both signs of offset and
word wrap. Assertions cover actual allocated slot, position, list, timer, retained
parameter and clearing stale fractions/velocities. A forwarding bus rejects all
migrated table reads while still providing real projectile headers. Six requests
against a full pool preserve existing actors, and invalid parameters still fail
before that full-pool return. Native source: $86:A2A1..A2ED.

Full Release Verification and Windows Release build pass. Animation instruction
programs/artwork remain separate presentation work; this migration removes spawn
definition reads, not the entire gunship presentation dependency. #547 remains open.

## Ridley pogo launch records and RNG correction

Compiled $A6:B94D..B9D4: both six-word acceleration arrays and four six-word rows
each of horizontal and vertical launch speeds, resolving the intervening native
pointer tables. All six authored stages are represented; live health selection
retains the existing stage clamp plus two. NTSC integer 8.8 words, negative Y
velocities and previous-X-sign negation are unchanged.

The ROM-pointer-based oracle compares every authored record, then checks
1,572,864 real initializer calls across every RNG word, six health-stage inputs
and four prior horizontal speed/sign cases. The runtime bus rejects all access.
Source inspection and a failing assertion also exposed an erroneous RNG advance;
#595 switches to the native current-word read and verifies exactly one read and
zero advances per initialization. See RIDLEY-POGO-RNG-595.md for before/after evidence.

Full Release Verification, Windows build and the complete Norfair Ridley audit
(360 reveal / 4,096 combat / 738 death frames) pass. #595 awaits player validation;
#547 remains incomplete. Remaining side-target, carry/claw and health-divisor
readers are explicitly retained in the inventory.

## Ridley arena targets and health-stage movement selectors

Compiled five three-entry facing/side target tables and two distinct four-entry
health divisor tables (23 words total). Real pogo, ground attack, carry, release,
hover divisor selection and grab-approach consumers no longer read those tables.
Preserved the existing facing/health clamps, signed coordinate branches, timers,
phase handoffs and native inertia calculations. The carry initializer is now
static; the shared divisor reader retains its diagnostic instance signature.

Compared every word with the pinned ROM. Exercised all 65,536 facing/health words
through actual carry setup and hover divisor selection, checking targets, exact
X/Y acceleration, carry timer and phase. Another 192 real side-target movements
cover both pogo directions, ground approach and release across facing, health
and coordinate boundaries. All these run with a throwing bus. Another 65,536
actual grab approaches check their distinct health divisor's exact X/Y effect;
their forwarding bus rejects the migrated table while allowing still-required
Samus pose and claw data. Expected acceleration uses an independent specialization
of the already-verified native zero-velocity inertia, not the production helper.

Full Release Verification, final focused compiled-definition verification, Windows
build and complete Norfair Ridley audit (360 reveal / 4,096 combat / 738 death
frames) pass. Corrected the reference name for $A6:B439: these are hover movement
divisor indexes, not tail instructions. Claw Y has a separate bounds question
(three authored entries versus the existing host clamp of eight); no speculative
geometry change was made in this table migration. Attack-choice tables and full
#547 inventory/integration remain unfinished.

## Ridley attack-choice distributions

Compiled all six eight-entry action distributions at $A6:B38C..B3EA into
RidleyAttackChoices. The real selector uses typed spans instead of reading
callback words from the ROM. Preserved branch order, RNG advance, zero-health
lunge-count wrapping and same-frame tail dispatch. The native callback names
do not always match existing translated phase names; catalog summaries retain
the source identity rather than silently substituting a similarly named phase.

All 48 entries match the pinned ROM, including the normally unreachable reversed
distribution. 655,360 actual selections cover every RNG word across ten explicit
health/pose/zone cases, with migrated table reads forbidden. A reference dispatch
starts directly from the native table word and compares the resulting phase,
timer and exact X/Y velocity. Tests assert one selector RNG advance, identical
current-word reads during setup, and spin-jump priority over the wrapping
zero-health lunge counter. This verifies table substitution and dispatch timing,
not the independent correctness of every chosen attack implementation.

The selector's ordinary-health ordering matches the pinned native source and
was not reordered. Full-word signed comparisons outside ordinary boss health
remain a separate parity-audit consideration; this table migration does not
claim exhaustive selector equivalence for corrupted health values.

Focused and full Release Verification pass. Windows builds with zero warnings
and errors. The Norfair Ridley audit passes 360 reveal frames, 4,096 combat frames
across ten states and 738 death frames. #547 remains open for the remaining
definitions, caller inventory and integration contract.

## Ridley claw geometry and carried-Samus placement

Replaced both live claw-offset reads with RidleyClawOffsets. The three authored
X words and three authored Y words are compared directly with the pinned ROM.
The old host clamps X to index two and Y to index eight after shifting the foot
index. Native Y geometry has only three entries; the following six words are
instructions. These six exact words are explicitly segregated as an existing
out-of-range compatibility window, not advertised as additional geometry. No
new native bounds behavior is claimed or introduced. All 65,536 index words
preserve the pre-migration results. Remaining native out-of-range parity is
documented rather than hidden by a narrower table or a new exception.

Actual claw getters, collision, grab initialization and carried-Samus placement
are now static and bus-free. Tests exercise 1,179,648 carry placements spanning
all coordinate/offset words, three facing values and six foot words (including
odd-index truncation). Expected positions independently combine native offsets,
signed four-pixel offset decay and 16-bit coordinate wrapping. Another 228,150
actual collision probes verify strict hitbox edges across three world positions,
including wraparound. The tests do not substitute catalog-only assertions for
the reported domain behavior. #547's broader caller and integration audit remains.

Focused/full Release Verification pass. Windows and DebugRunner builds have
zero warnings/errors. The complete Norfair Ridley audit passes 360 reveal,
4,096 combat (ten states), and 738 death frames, including zero-health grabbing.

## Falling-spark launch definitions

Compiled $86:F3D4/F3D6 horizontal whole/fraction pairs into
FallingSparkLaunchDefinitions. Seven authored records plus the eighth RNG
outcome's actual $F3F0/F3F2 instruction words (DBBD/301A) are retained. Unlike
the earlier Ridley host-clamp compatibility window, this overread is directly
reachable through the native initializer's normal RNG mask. Do not replace it
with an invented symmetric positive speed. Pinned bank-86 initializer F391
copies the source positions/fractions, offsets Y by eight, clears the aliased
vertical velocity words, advances RNG once and chooses these horizontal words.

The real initializer no longer reads either table. All 65,536 RNG values compare
both compiled fields with ROM, then exercise the actual allocator/header/spawn
and horizontal-motion paths with a bus rejecting the entire table/overread
window. Assertions cover copied fractions, Y wrap, cleared stale vertical
velocity, launch aliases, exactly one RNG advance and resulting 16.16 movement.
Full-pool allocation preserves the occupied projectile and consumes no RNG.
Reference names were corrected from InitialY/InitialX to HorizontalWhole/
HorizontalFraction; those old names confused velocity aliases with positions.

Focused/full Release Verification and Windows build pass. The complete Spark
audit also passes after correcting its independently stale immediate-knockback
assertion in #597; see SPARK-AUDIT-597.md. This migration remains only one slice
of #547, whose broader inventory/integration requirements are still open.

## Dead-sidehopper post-landing launches

Compiled the four vertical and four horizontal 8.8 velocity words at
$A9:D951/D959. The actual post-landing dispatcher no longer reads either ROM
table. Pinned $A9:D91D decrements the timer, returns on nonnegative result,
branches to draining when the palette stage is nonzero, or restarts the idle
instruction list and publishes the selected launch pair. Preserved that ordering.

Compared all eight words with ROM and ran 524,288 actual dispatcher cases with
a throwing bus: every timer word, all four ordinary jump phases, and zero/nonzero
palette stage. Assertions cover signed timer expiry/wrap, exact velocities,
phase handoff, instruction pointer and both instruction timers, and unchanged
jump phase. Phase is initialized to zero and its only ordinary writer increments
modulo four upon landing. Non-authored corrupted phase values are not covered
by this migration's native-parity claim; compiled span indexing rejects them
rather than interpreting arbitrary adjacent ROM data.

Focused and full Release Verification pass. Windows Desktop builds with zero
warnings and errors. This does not close #547 or establish full corpse-system
parity; it verifies the migrated launch data and its real timer/phase consumer.

## KiHunter proximity, gravity and detached-wing radius

Compiled F180 proximity (96), F182/F184 fractional/whole gravity (E000/0000)
and the F186 low-byte detached-wing radius (48). Native F55A and F5E4 use the
same fractional ADC followed by whole-word ADC with carry. Existing reference
names incorrectly called those acceleration words attack radii; corrected them.
Removed live reads in patrol trigger, falling, hopping, detached-wing setup,
orbit and collision arc. Orbit and detachment setup are now static/bus-free.

Tests compare each constant with pinned ROM, exercise 65,536 actual orbit
placements with wrapped coordinates, and verify all fractional gravity carries.
The orbit expectation uses the already separately verified shared trigonometry
and quadratic-angle helper, with the radius read independently from ROM; this
is a constant-consumer check, not an independent revalidation of those helpers.
Gravity arithmetic is compared with independent 32-bit packed addition.

The orbit fixture uses native minimum speed index 0100 across all angle words;
an initial arbitrary-speed sweep exceeded authored quadratic records and was
corrected rather than relaxing production bounds. Full Release Verification,
the complete KiHunter audit (38 body/wing records in six populations), and
Windows build pass. Broader lookup/integration work remains open.

## Kraid spat-rock horizontal launch velocities

Compiled the eight signed 8.8 words at $A7:BC65. Native mouth-open attack code
selects with the current RNG word masked by 000E; bank-$86 initializer 9CA3
places the rock at body X+16/Y-96, clears fractions and uses that selected speed.
The translated spawn retains its existing allocation-before-current-RNG-read
ordering and never advances RNG. Only the immutable velocity read is removed.

Every RNG word is compared with the pinned ROM and exercised through the real
allocator/header/spawn path with BC65..BC74 reads forbidden and a throwing RNG
advance callback. 65,536 cases assert the exact signed velocity word, vertical
launch, wrapped mouth position, cleared stale fractions and graphics binding.
Full-pool allocation returns false, preserves the occupied rock and does not
read RNG. Broader rock collision/movement and other Kraid mechanics are not
claimed complete by this focused launch migration.

Full Release Verification, complete Kraid audit (788 rise frames, repeated
combat, second phase and 360-frame death/persistence) and Windows build pass.
The Kraid audit covers all four rock variants and damage-enabled projectile
contact lifecycles; its printed floor diagnostic is not a visual parity claim.

## Kraid second-phase indirect movement choices

Compiled BA7D's six position/pointer rows and all thirty pointed-to target/timer
pairs at BA95..BB0C. Native BA2F decrements its thinking word and selects only
when the result equals zero. Position search defaults to row one; RNG mask 001C
clamps offsets 10/14/18/1C to 10, giving the fifth choice half the probability.
Both rules were confirmed against pinned assembly and retained. Production now
uses a named compiled selector, without any of the former direct/indirect reads.

Tests read native position/pointer records and follow all six pointers to derive
the independent reference pairs. Every RNG word is checked for every authored
row. 524,288 actual thinking calls cover every coordinate word and all eight
masked outcomes, asserting global target, thinking timer, signed walking
direction, instruction selection and animation restart. All timer words verify
the exact decrement-to-zero gate, including zero wrapping to FFFF, with no RNG
read on inactive calls. The production bus throws on every access; the RNG
advance callback also throws. This verifies selector substitution and handoff,
not the full physical walking trajectory or all remaining Kraid definitions.

Full Release Verification, the complete Kraid audit and the Windows Release
build pass. The audit includes second-phase walking and 360-frame death and
persistence; its floor diagnostic remains distinct from visual parity evidence.

## Kraid body projectile contour

Compiled the seven overlapping left/top pairs at A7:B163/B165 into
`KraidBodyContour`. The actual outer-body collision consumer no longer reads
these words. Mouth hitboxes remain indirect and are not included in this change.
The native B161 definition confirms the final signed-minimum sentinel; the
existing host signed-relative-Y and strict X-boundary arithmetic are retained.
This is not a claim that arbitrary overflow coordinates match every native
16-bit CMP/ADC/SBC operation; that separate arithmetic audit remains necessary.

Tests derive all seven pairs independently from ROM, compare every signed Y,
and exercise 1,769,472 actual collision calls at the left boundary and either
side, with three body translations and three projectile radii. Body/shot Y
coordinates wrap across the full word domain. Any production bus access throws.

Focused and full Release Verification, the complete Kraid audit and Windows
Release build pass. No new player-visible collision correction is claimed.

## Kraid fingernail indirect launch words

BE3E/BE46 point to eight four-word records at BE4E..BE8D. All four
RNG-selected records within each sign group are identical. Compiled the two
distinct fixed-point tuples rather than retaining redundant pointer lookups.
Current RNG is still read for horizontal/diagonal spawn mode. The sibling-state
correction from #598 is preserved and not attributed to this migration.

Extended the actual initializer regression to forbid all bus access, compare
all four compiled words against the pointed-to native records, and explicitly
assert stale fractional velocities are cleared. Both nail slots, every RNG/sign
word and four sibling flags cover 524,288 actual initializations. This covers
launch definitions, not the remaining contour or bounce-arithmetic parity.

Full Release Verification, complete Kraid audit and Windows Release build pass.

## Kraid ceiling-break rock placement

Compiled A7:ACB3's nine X coordinates. The byte-addressed accessor preserves
the old reader's eighteen admitted offsets, including the final odd offset
overlapping ACC5's AD03 callback word. That compatibility is not a claim about
native execution of malformed odd callback selectors; authored progression is
even offsets zero through sixteen.

589,824 actual growth updates cover all Y words and nine authored selectors.
Tests assert movement-before-emission cadence, selector progression, actual
rock X/Y/vertical speed and paired ceiling PLM requests. The production bus
rejects migrated placement reads; projectile definition reads remain separate
runtime dependencies. The independent rising-rock cadence is disabled in
this focused fixture; the full Kraid battle audit covers the combined sequence.

Full Release Verification, complete Kraid audit and Windows Release build pass.

## Kraid sinking callback schedule

C5E7 contains 28 Y/tilemap-offset/callback records followed by FFFF. The
mechanical consumer now uses `KraidSinkSchedule` to select six named crumble
callbacks and the separate empty RTS for the other 22 rows. It preserves
exact-Y dispatch and event counting, rather than merging no-op and absent rows.

Tests independently read the native schedule and invoke the real dispatcher
at all 65,536 Y words. They assert event count, projectile count and X position,
and platform request arguments decoded directly from each native callback's
inline hardcoded-PLM invocation. The migrated schedule reads throw. Shared
projectile definition reads remain allowed and tracked separately. This is
not a claim about the presentation tilemap-offset field or the entire renderer.

Focused and full Release Verification, complete Kraid audit and Windows build pass.

## Kraid private-head entry timers

Compiled roar entry 96D2, glow 974A, death 9764, and four growth-resume timers
96EC/96F4/96FC/9704. All production fixed-entry readers now use the catalog;
the general mixed head stream is deliberately not replaced by extracted mechanics.

Tests compare native timer words and invoke both phase thinkers for every timer
word, then growth setup for every candidate tilemap word. They assert the
selected resume cursor and timer together. Glow's same-call decrement and
death's hurt-frame gate/no-decrement entry are exercised through real methods.
The bus rejects fixed-entry timer reads while retaining unrelated palette data.
The now instance-free second-phase body thinker is static; its separate
three-argument foot-thinking overload remains unchanged.

Focused and full Release Verification, complete Kraid audit and Windows build pass.

## Kraid mouth geometry

Compiled all eight four-word rectangles at 9788..97C7, including unused entry
four. The actual mouth collision helper bypasses the bus for these fixed
definitions. Collision still uses left, top and bottom only; retaining
the right definition word does not invent a new right-side collision limit.

Tests parse the entire native private-head region 96D2..9787 and resolve every
non-sentinel hitbox reference. All 32 words are independently compared to ROM.
1,572,864 real mouth collision calls cover every projectile Y and both sides
of each left boundary, including exact equality. Existing host arithmetic is
retained; full-word native overflow parity is not claimed by this substitution.

Non-catalog pointers retain existing address-space reads, including mutable
low-bank memory. The Kraid audit exercises pointer zero, exposing why rejecting
all non-catalog values would be an incorrect narrowing. Separate regression
probes cover zero, another low address, the low-bank boundary and an unaligned
ROM address. Thus this compiles the fixed geometry, not every possible indirect
read. The wider mixed head-program migration remains outstanding.

Full Release Verification, complete Kraid audit (including pointer-zero
diagnostic handoff) and Windows Release build pass.
