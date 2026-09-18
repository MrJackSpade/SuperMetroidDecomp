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

Those three fixed selectors are now compiled independently of their mixed
instruction programs. Golden Torizo retains both facing-specific reflected-Super
lists; all three Tourian statue byte offsets retain their lists; and Work Robot
retains all three authored lists plus parameter three's adjacent `$54AE` code
word. Nine native words and the real production initializers pass with all three
source ranges forbidden. The selected instruction programs remain separate
dependencies for #538/#539.

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
The Fireflea room effect's twelve flashing shades and all seven retail darkness
states are compiled as well. The maximum-death state preserves the cartridge's
intentional offset-twelve observation of the adjacent `$C208` opcode word; the
mutable phase timer, cycle index and darkness counter remain WRAM. The full
effect runs with `$88:B058-$B07D` forbidden after all nineteen definitions are
independently compared with the pinned cartridge.
Fireflea's eight parameter-selected circular/vertical movement radii are compiled
too. Every production initializer runs with `$A3:8D1D-$8D2C` forbidden; restored
selectors outside the authored zero-through-seven domain fail explicitly instead
of reading adjacent enemy code.
Cacatac's six parameter-selected patrol distances are compiled as physical
definitions as well. All eighteen selector/wrapped-origin initializer cases run
with `$A2:9F36-$9F41` forbidden; invalid restored selectors no longer consume
adjacent code as a distance.
Cacatac's ten spike-program selectors and cardinal/diagonal signed 8.8 launch
speed pairs are compiled too. All ten actual production spawns retain their
definition loading, allocator order and copied origin while `$86:D96A-$D97D` is
forbidden. The selected mixed animation programs remain separate dependencies.
Atomic's four population-selected initial instruction lists are compiled too.
All four production initializers retain the previously compiled shared linear
speed pairs while `$A8:E380-$E387` is forbidden. The selected mixed animation
programs remain separate dependencies.
Sbug's eight direction-selected instruction lists and seven activation callbacks
are compiled too. All eight production initializers preserve the cartridge's
odd-index normalization, and all seven proximity activations run with
`$A3:A111-$A12E` forbidden. Mixed animation programs remain separate dependencies.
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
$FD..$FF still read adjacent executable data. Facing, movement type and no-input
fallback pose are now compiled for all 253 authored poses in
`SamusPoseDispatchDefinitions`. Shot direction and per-pose instruction programs
remain live mechanical readers; these completed fields do not complete metadata.
The authored input graph is now compiled separately in `SamusPoseInputDefinitions`:
253 pose mappings, 86 list identities and 598 ordered conditions. The production
matcher consumes those typed conditions directly, preserving empty/self-match
fallback semantics and native diagnostic entry addresses. The three trailing pose
indexes still use the explicit pointer/record reader; animation programs are separate.
Collision radii are now separately compiled for all 253 authored poses, including
the prospective larger-pose and crouch-fallback consumers. The three trailing
pose indexes still retain explicit adjacent-data reads. This physical catalog
does not consume or replace the presentation offset.

The forty direction-specific beam/missile origin words at $90:C204..C253 are
now compiled too, with the standing/running and two special Moonwalk pose
branches preserved. Direction nibbles ten through fifteen still cross adjacent
rows; running Y eventually reaches the separately compiled cooldown bytes. Charge-flare
origins remain presentation data. Beam initial speeds, missile/Super Missile and
beam accelerations are now compiled as 85 native words, including the adjacent
ignition marker. Exact-address dispatch preserves invalid beam combinations
reading into missile data and leaves non-catalog/unaligned reads on the bus.
The 59 contiguous cooldown bytes (uncharged/charged/padding/non-beam/auto-fire)
are compiled too; combination indices beyond that range still read adjacent SFX
presentation bytes. Damage/animation definitions still need migration. The separate
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
- Samus movement, combo and grapple definition readers. Bomb Spread's twenty
  launch words are now compiled and checked through the actual five-slot producer;
  its shared damage/animation initializer still reads ROM and is not covered by
  that migration. Coordinate
  ownership with the companion gameplay-definition tickets; shared scope does
  not mean the runtime dependency is already removed.

Special-beam-attack Power Bomb costs and Ice/Plasma origin angles are now
compiled as typed mechanics definitions. All twelve beam-indexed cost words and
four slot-indexed angle words match the pinned cartridge, and the real Wave,
Ice, Spazer, and Plasma producers run with both source ranges forbidden. Combo
offsets now consume the shared compiled signed-sine catalog while preserving the
native low-byte hardware multiply and truncation. All 65,536 angle/amplitude
pairs match, and the real Ice, Spazer, and Plasma updates run with the 512-byte
sine source forbidden. Combo instruction programs and editable presentation
remain separate dependencies.

The Baby Metroid's eight ceiling-to-Samus movement records at `$A9:CA24` are
compiled as typed physical targets, divisor indexes, and callback identities.
The catalog preserves every overlapping +8 read, including the final record's
adjacent `$CA66` latch callback. The full entrance/drain/route/healing fixture
runs through the production state machine while all 66 source bytes are
forbidden, and all 33 overlapping words are independently compared with the
pinned cartridge. Baby sprite/instruction presentation and the wider Mother
Brain integration remain separate dependencies.

Mother Brain's three count-prefixed Samus-contact lists at `$A9:B427-$B454`
are now compiled physical definitions shared with #535's visual/mechanical
boundary. Body, brain, and neck retain their native asymmetric signed extents
and record ordering. Direct parity covers all twenty extent words; 49,155
production collision probes cover both sides of every origin, overlap depth,
and component selection through a helper that no longer accepts a ROM bus.
Editable spritemaps cannot change these application-owned hitboxes.

Mama Turtle's 48 signed sleeping-shell contour words at `$A2:8E80` are now a
compiled physical definition shared by parent/Samus carry collision and Baby
Turtle crawling. Both asymmetric 24-pixel halves match the pinned cartridge.
The real sleeping-parent path passes every in-range distance plus both outside
boundaries through a helper that no longer accepts a ROM bus; sprite presentation
remains independent.

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
