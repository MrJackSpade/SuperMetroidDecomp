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

### Remaining work

This is not the entire lookup-table migration. Remaining signed-table callers,
linear/quadratic speed tables, family-specific tables, callback classification
reads and indirect/banked caller inventory remain. Mutable WRAM must remain
mutable, not become a compiled substitute. Wider ROM-free room/asset integration
is tracked by #530/#549. No player-visible fix or complete ROM-free gameplay is
claimed by this slice, and #547 remains open without a validation label.
