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
