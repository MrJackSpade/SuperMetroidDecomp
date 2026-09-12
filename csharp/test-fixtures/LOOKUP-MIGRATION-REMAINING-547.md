# Remaining lookup migration inventory (#547)

Initially inspected at `73343942`; expanded at `a86728da`. This is a verified list of remaining callers, **not an
exhaustive completion certificate**. Multiline reads and intermediate-address
variables must be inspected; a search for `ReadWord(...Speed...)` misses them.

## Confirmed remaining runtime mechanics reads

The previously enumerated Ridley claw-offset consumers are now compiled as well.
The expanded scan below found additional live groups; exhausting the old rows
did not establish completion.

### Expanded named-catalog caller audit

At `a86728da`, `rg 'EnemyRomTablePointers\.' csharp/src/SuperMetroid.Core/Game`
finds 50 matching source lines. That count includes presentation reads and is
neither a count of unique tables nor all immutable-ROM dependencies. Inspected
the consumers rather than classifying them solely by the catalog names.

The formerly enumerated named-table groups are now compiled, including BF1D's
fingernail contour and the adjacent words consumed by its inherited bounded
walk. This does **not** establish completion: the scan excludes uncatalogued
indirection, mixed instruction streams and mutable-memory reads. The contour
limit question is now resolved: exhaustive native-word scanning proves all Y
inputs select one of six records. The unreachable 32-record cutoff and unused
trailing words have been removed.

### Mixed instruction selectors and presentation reads

Kraid's fixed roar, glow, death and growth-resume entry timers are compiled.
The private head stream still mixes subsequent timers, sound commands,
tilemap bindings and mouth-hitbox pointers and requires a wider separation.

Kraid's C5E7 sinking Y/callback schedule is compiled. All 28 rows, including
empty RTS callbacks, are covered through actual rock/PLM dispatch. The middle
tilemap-offset words are presentation data and are not a new mechanics asset.

The falling-spark launch group from the expanded scan is now compiled, including
the eighth outcome's native instruction-word overread. The reference catalog
names now identify horizontal whole/fraction velocity rather than initial X/Y.
Actual spawn and horizontal-motion tests reject reads of the migrated table.
Dead-sidehopper launch pairs are also compiled. The actual post-landing dispatcher
is verified for every timer word, all four ordinary phases, and both palette gates
with a throwing bus; the rest of its corpse/animation system remains separate.
KiHunter's proximity, gravity and detached-wing radius constants are compiled;
the misleading attack-radius reference names are corrected. Its instruction
programs, populations and presentation still belong to the broader integration.
Kraid second-phase movement choices are compiled, including all six indirect
rows, default-position handling and the half-probability final choice. Actual
walking target/direction/timer/animation transitions are covered without ROM reads.

The remaining named-catalog users also include Golden Torizo's reflected Super
Missile instruction selector, unpowered Work Robot instruction selection, and
Tourian statue instruction selection. These are **not automatically artwork**:
the selected programs can contain gameplay behavior. Coordinate their compiled
program/selector ownership with #538/#539 and presentation bindings with #549.
Work Robot explicitly accepts parameter three and observes an adjacent code
word beyond its three authored pointers; preserve that native case when migrating.

Confirmed presentation-oriented groups from this scan include Torizo/Chozo/
Tourian/Kraid/Ridley palette reads, Ceres door transfer pointers, Ridley wing and
tail-tip spritemap selectors, Crocomire death graphics transfers, and gunship
liftoff graphics transfers. They remain runtime dependencies for their associated
presentation/integration tickets; classifying them separately does not remove them.

These were selected indirect `EnemyRomTablePointers` consumer groups, not an
exhaustive list. Classify instruction selectors separately from artwork payloads.
Phantoon's inspected movement, attack selection and death-schedule data are now
compiled; its palette, eye instruction-list and materialization-sound selectors
remain presentation readers requiring their respective integration audit.
The shared Ceres/Norfair inertia bytes ($A6:D712/$A6:D61F) are now compiled.
The shared $94:8B2B height profiles are compiled for Samus, enemies, missiles and
bomb spread. Samus horizontal multipliers are also compiled and exhaustively
verified. Square-slope definitions now share a compiled domain catalog across
Samus collision, Grapple release, enemy collision and missile point reactions;
the remaining missile table read is removed.
Bomb/Golden Torizo's two initial position/radius/property/instruction records are
also compiled. Their instruction programs and presentation remain separate work.
Gunship liftoff dust's six X-offset/list-selection records are compiled as well;
their reference constants now reside under Gunship rather than the old Ceres label.
Ridley's four pogo launch-speed rows and both six-stage acceleration arrays are
compiled, including the native pointer indirection. Side targets, carry/release
anchors and both health-stage divisor tables are also compiled. All six attack
distributions are compiled and their live table read is removed. Claw geometry
is compiled, preserving the existing host bounds and explicitly separating the
three authored Y words from six adjacent instruction words formerly reachable
through its clamp. Native behavior outside authored indexes still needs a
separate parity review; a table migration does not establish those host bounds
as native rules. Inspect authored bounds and adjacent-data behavior before
substituting a catalog; do not assume every table is a smooth formula.

## Additional inventory still required

Samus ordinary/Hi-Jump and wall-jump launch pairs plus air/water/lava gravity
are now compiled in `SamusVerticalMotionDefinitions`. Their production setup
calls reject all ROM access in the regression. Bomb-jump and knockback launch
pairs are now compiled too; knockback still consumes pose/animation records.
The formerly named falling-transition pair is actually the shared ball-rebound
pair, and is now compiled and correctly named. Grapple's remaining
connection/pose selectors are separate readers. The source audit explicitly found
those readers outside the enemy-table catalog scan.

Grapple's ten launch velocity/angle triples and both ten-direction physical hand
origin pairs are now compiled, including the locked-connection and late-draw
consumers. Flare placement remains presentation data for #540. Out-of-domain
restored origin indexes preserve adjacent-ROM reads and remain an explicit
dependency. The 28 cancellation bytes, 30 connection function/handler pairs,
eight special-angle/pose/offset/function records and 20 dropped-pose selectors
are now compiled too. Tests exercise actual dispatch with their source ranges
forbidden. Non-catalog connection pointers still retain adjacent-ROM reads.
The separate bank-$90 HUD movement-handler table and twelve authored posture
flags are now compiled in `SamusHudDefinitions`, shared by Grapple admission
and projectile charge-preservation dispatch. All pose bytes retain native
posture threshold/adjacent-data behavior; out-of-table flags remain explicit
ROM dependencies. Grapple's swing body offsets and their physical angle selector
are now compiled separately from the displayed art frame. Flare animation and
drawing pointers remain presentation readers for #540/#541; editable assets and
their restart/update behavior are not yet implemented by this separation.

Ordinary-running and Speed Booster cadence is now compiled: six eleven-byte
streams, six pointer words and five boost-counter reset words at $91:B5D1..B628.
These timings advance gameplay stages and are not editable cosmetic timing.
The sound-call accumulator bug remains intact, including stage-five selection
of adjacent pose data and a low-bank mutable delay address. Non-catalog reads
remain explicit dependencies; per-pose animation commands and artwork are still
outside this completed cadence group.

Pose byte four is shared by artwork and projectile origins. Its physical copy
for all 253 authored poses is now compiled independently for beam/missile setup,
Grapple launch and the late physical-origin update. Visual body/cannon/flare
readers remain separate presentation dependencies. The three trailing pose bytes
$FD..$FF still read adjacent executable data. Facing, movement, fallback pose,
shot direction, collision radius and per-pose instruction programs remain live
mechanical readers; compiling this one shared field does not complete metadata.

The forty direction-specific beam/missile origin words at $90:C204..C253 are
now compiled too, with the standing/running and two special Moonwalk pose
branches preserved. Direction nibbles ten through fifteen still cross adjacent
rows; running Y eventually reads cooldown bytes outside the catalog. Charge-flare
origins remain presentation data. Beam initial speeds, missile/beam accelerations,
cooldowns and damage/animation definitions still need migration. The separate
previous-frame displacement omission discovered during this audit is addressed
under #600: the initializer now consumes native mutable movement/camera records,
with runtime producers/reset and saved-state continuation coverage. These WRAM
reads are legitimate live state, not remaining immutable lookup dependencies.

The four standalone horizontal records for diagonal bomb jumps and Grapple
release ($90:9F25/$9F31/$9F3D/$9F49) are now compiled. Exact-address recognition
preserves the existing fallback for non-catalog/unaligned/mutable records.
Movement-indexed `ReadEntry` now compiles all 82 authored records: normal air has
26 rows, water and lava have 28 each. Address-based resolution preserves reads
across those boundaries, including normal-air indexes 26/27 entering water rows
0/1. Every movement byte and restored base word has been checked for exact
address/alignment handling. Non-catalog reads remain live rather than being
clamped; higher byte indexes can still read adjacent executable/data bytes.
That residual ROM dependency needs explicit treatment under the wider contract,
not relabeling as mutable state. Low-bank aliases genuinely remain mutable.

- `EnemyRomTablePointers` consumers: boss jumps, projectile launch/angle records,
  death trajectories and other indirect family definitions.
- Samus movement, bomb-spread, combo and grapple definition readers. Coordinate
  ownership with the companion gameplay-definition tickets; shared scope does
  not mean the runtime dependency is already removed.
- Bank/indirect reads and definitions whose names do not contain speed, curve,
  angle or math. Trace intermediate addresses rather than treating search hits
  as complete coverage.
- Presentation-versus-mechanics classification. For example, Dachora's speed
  **palette** pointer selects artwork and is not a velocity table. Mutable WRAM
  aliases must remain live state, not compiled constants.
- The full #530/#549 integration contract: a compiled math subset does not prove
  ROM-free gameplay, presentation override persistence or missing-resource behavior.

## Completed categories are not remaining work

The evidence log `COMPILED-ENEMY-MATH-547.md` records compiled shared signed/
unsigned sine, linear/quadratic curves, multiple direct math callers, literal
shot/Power Bomb callback classification, and the Bull, Puyo, Crocomire, Botwoon,
crawler/Yard base-speed, Polyp, Shaktool, Ceres getaway and Boyon curve slices.
Their focused comparisons and diagnostic repairs do not close the rows above.

The earlier listed crawler, family, intro and renderer curves were subsequently
compiled and verified; see the main evidence log. The new rows are confirmed
live indirect-definition consumers, not a complete inventory. Next grouping:
EnemyRomTablePointers mechanics readers. Keep verified commits scoped and do not close #547 until its
complete caller and integration audit is satisfied.
