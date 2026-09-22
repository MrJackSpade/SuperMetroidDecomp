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

Kraid's complete private head stream is now separated into compiled control metadata
and cartridge-backed presentation payloads. All 28 command records retain 21 timers,
three sound callbacks, four terminators, tilemap identities, and both mouth-hitbox
selectors; the real interpreter and growth-resume consumer run with all 91 stream
words forbidden. Mutable bank-$A7 low-half aliases remain live, while unrelated
upper-ROM cursors fail explicitly. The selected 704-byte tilemaps remain cartridge
presentation assets and the already-compiled hitbox pointers still resolve through
the collision catalog.

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
The two dead-sidehopper corpse-rotting configuration records are now compiled as
typed metadata too. Their sixteen pointer/count/callback words and the shared
derived wrap offset match the pinned cartridge, and both real initializers run
with those metadata sources forbidden. The mutable WRAM rotting table, variable
rotation-row lookup, VRAM transfer streams, graphics payloads, and animation
programs remain separate live dependencies.
The same migration now covers all eight dead Zoomer, Ripper, and Skree variants:
sixteen initial-instruction/configuration selectors, sixty-four configuration
words, and eight derived wrap offsets are compiled as typed definitions. All
eight real initializers run with the selector, configuration, and derived-offset
sources forbidden. Their mutable rotting state and variable streams remain live
for the same reason as the sidehopper streams.
Dead Torizo's single eight-word corpse-rotting configuration and derived wrap
offset are compiled too. Its real initializer runs with both metadata sources
forbidden. Mutable rotting state, variable rotation rows, VRAM transfer programs,
graphics payloads, and hitbox presentation remain separate live dependencies.
Crocomire's eleven bridge-fragment X positions are compiled as physical launch
definitions. Every authored word, all eleven real descending-pool allocations,
their RNG-derived vertical velocities, and the cursor-22 cutoff run with
`$A4:9156-$A4:916B` forbidden. The projectile definition and graphics
program remain separate dependencies.
KiHunter's proximity, gravity and detached-wing radius constants are compiled;
the misleading attack-radius reference names are corrected. Its thirteen body and wing
instruction programs now compile all 84 control words as well. Both directional acid-spit
paths retain their sound, wait-state, and projectile effects while 59 interleaved
spritemap pointers stay live presentation data. Populations and broader presentation
integration remain separate.
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
Phantoon's inspected movement, attack selection, death-schedule data, and all
three materialization-sound selectors are now compiled. Two complete real
callback cycles preserve the native sound order and 0/1/2 wrap with
`$A7:CDED-$A7:CDF2` forbidden. Its palette and eye instruction-list readers
remain presentation dependencies requiring their respective integration audit.
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
forbidden. All ten selected projectile programs now compile their twenty fixed duration
and terminal-sleep words too, while the ten interleaved spritemap operands remain live
cartridge presentation data.
Cacatac's four enemy-body programs now compile all 56 duration, callback, direction,
goto, and loop-target words as well. Both idle loops and both full attacks execute through
sound, all ten projectile spawns, and patrol restoration with mechanics bytes forbidden;
their 24 interleaved spritemap operands remain live presentation reads.
Atomic's four population-selected initial instruction lists are compiled too.
All four production initializers retain the previously compiled shared linear
speed pairs while `$A8:E380-$E387` is forbidden. The selected mixed animation
programs remain separate dependencies.
Sbug's eight direction-selected instruction lists and seven activation callbacks
are compiled too. All eight production initializers preserve the cartridge's
odd-index normalization, and all seven proximity activations run with
`$A3:A111-$A12E` forbidden. Their eight selected animation programs now compile all
48 duration, goto, and loop-target words as well; all 32 interleaved spritemap
pointers remain live presentation reads.
Wrecked Ship Spark's three authored initial instruction/function pairs are
compiled too, together with selector three's two adjacent-code observations.
All four production initializers retain the two-bit population mask while
`$A8:E682-$E68F` is forbidden. The four authored/installed animation programs now
compile 33 mechanics words while their 26 spritemap operands remain live; selector
three's adjacent code word is preserved as restored state but rejected as a program.
The elevator actor's Down/Up departure masks are compiled in its shared domain
catalog. Both real departure paths retain the doubled population byte offset,
pose/input/projectile/audio handoff, and Samus pinning while `$A3:94E2-$94E5`
is forbidden.
Owtch's eight patrol half-widths and six underground durations are compiled as
physical definitions. All 144 representative/wrapped production initializers
retain their shared linear speeds, state-specific burial setup and exact bounds
while `$A2:A3DD-$A3F8` is forbidden.
Its paired left/right animation loops now compile all twelve callback, duration, goto,
and loop-target words while retaining six live spritemap presentation operands. Both
production streams execute past their native goto with mechanics reads forbidden.
Stoke's two walking loops and two directional attacks now compile all twenty-six callback,
duration, projectile-direction, goto, and loop-target words while retaining twelve live
spritemap presentation operands. Both real attacks spawn the expected projectile and
return to the matching compiled walking stream with mechanics reads forbidden.
GRipper, Ripper II, and ordinary Ripper's six directional loops now compile all thirty-six
duration, goto, and loop-target words while retaining twenty-four live spritemap operands.
Every production initializer/reversal route selects and executes its complete 8/7/8/7
loop without mechanics reads, including Ripper II's intentionally inverted native labels.
Kzan's single spike-platform program now compiles its one-frame duration and terminal
sleep while retaining its live spritemap operand. The real top-platform initializer
reaches terminal sleep with mechanics reads forbidden.
Mellow, Mella, and Memu's shared flight loop now compiles all six duration, goto, and
loop-target words while retaining four live spritemap operands. All three production
initializers execute the complete loop with mechanics reads forbidden.
Bull's ordinary and immune-shot programs now compile all sixteen timer, duration, goto,
and loop-target words while retaining eight live spritemap operands. The real initializer
executes the complete normal loop, and the real immune-shot handoff repeats its flash loop
five times before returning to normal, with mechanics reads forbidden throughout.
Kago's slow and post-hit loops now compile all twelve duration, goto, and loop-target words
while retaining eight live spritemap operands. The real initializer executes the complete
slow loop, and the real shot tail spawns its Kago bug before executing the complete fast
loop, with mechanics reads forbidden throughout.
The horizontal shutter's stationary program now compiles its one-frame duration and
terminal sleep while retaining its live spritemap operand. The real shootable-shutter
initializer reaches terminal sleep with mechanics reads forbidden.
The growing shutter's four height programs now compile all eight duration and terminal-sleep
words while retaining four live spritemap operands. The real initializer and all four
production section transitions execute with mechanics reads forbidden.
Plain shootable/destroyable vertical shutters and Kamer platforms now compile all eight
duration, sleep, goto, and loop-target words while retaining five live spritemap operands.
All three real initializers and the complete Kamer loop execute with mechanics reads forbidden.
Choot's idle, jumping, and falling programs now compile all eleven property, duration, and
terminal-sleep words while retaining five live spritemap operands. Its real initializer,
jump preparation, and apex handoffs execute all three programs with mechanics reads forbidden.
The Norfair lava jumper's hidden, jumping, and follower programs now compile all 23 duration,
callback, timer, branch, and target words while retaining fourteen live spritemap operands.
Both real initializers, the rise handoff, handshake, and follower loop avoid mechanics reads.
Beetom's six left/right crawling, hopping, and draining programs now compile all 48 property,
duration, callback, sleep, goto, and target words while retaining 32 live spritemap operands.
The real initializer and action installers execute every program without mechanics reads.
Nuclear Waffle's two sweep directions now combine twelve endpoint, link-spacing,
and joint-turn words into typed physical records. Both complete production
initializers retain their seven allocated articulated links while
`$A6:95F6-$A6:960D` is forbidden; invalid directions cannot consume main-AI code.
Its single body-animation loop also compiles fourteen mechanics words while all
twelve spritemap operands remain live presentation reads.
Hibashi's 22 eruption Y offsets and collision half-heights are compiled as paired
physical frames. Every production activity command retains exact placement,
radius and frame-zero width while `$A6:8DBB-$A6:8E12` is forbidden. Its paired
graphics/hitbox programs compile all 50 mechanics words while retaining 24 live
spritemap operands; both production streams reach terminal sleep with control reads
forbidden.
Blue Brinstar face blocks now compile all ten duration/sleep words across their
neutral and directional programs. Both activation sides and all three terminal sleeps
execute with mechanics reads forbidden while seven spritemap operands remain live.
Boulder's mirrored rolling loops compile all twenty duration, goto, and loop-target
words. Both initializer-selected production programs loop with mechanics reads forbidden
while sixteen spritemap operands remain live.
Boyon's idle and bouncing loops compile all eighteen property, callback, duration,
goto, and target words. Both production programs loop with mechanics reads forbidden,
the bounce callback retains sound/state effects, and ten spritemap operands remain live.
Magdollite's nine rise thresholds, body-list selectors and overlay offsets are
compiled as typed phase records. Real initialization, rising, falling and overlay
tracking retain their exact phase geometry while `$A8:AF55-$A8:AF8A` is forbidden.
Its seventeen head, pillar and hand programs also compile all 134 callbacks, timings,
sound operands, timer controls and branches. Both complete directional attacks retain
their visibility choreography and six real lava spawns while 53 spritemap operands stay
live presentation data.
Fune/Namihe's eight active/idle and facing instruction selectors are compiled;
all eight real production installs retain their cartridge identities while
`$A8:96D3-$A8:96E2` is forbidden.
The shared $94:8B2B height profiles are compiled for Samus, enemies, missiles and
bomb spread. Samus horizontal multipliers are also compiled and exhaustively
verified. Square-slope definitions now share a compiled domain catalog across
Samus collision, Grapple release, enemy collision and missile point reactions;
the remaining missile table read is removed.
Bomb/Golden Torizo's two initial position/radius/property/instruction records are
also compiled. Their instruction programs and presentation remain separate work.
Gunship liftoff dust's six X-offset/list-selection records are compiled as well;
their reference constants now reside under Gunship rather than the old Ceres label.
The gunship's seventeen landing-brake Y deltas and four byte-packed idle-bob
timer/delta records are compiled too. The complete post-Ceres landing fixture now
uses cartridge-exact motion instead of patched synthetic tables, and both production
consumers reject reads from the retired source ranges.
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

Draygon's four signed opening-dance latencies and all 1,104 movement records
reachable during its native 1,232-frame owner are compiled. Malformed restored
stream indexes now fail at that bounded catalog instead of reading arbitrary
adjacent bank-$A5 cartridge bytes.

All 118 aligned six-byte enemy item-drop probability records are compiled too.
The production selector rejects misaligned and external restored pointers instead
of interpreting adjacent bank-$B4 code or presentation data as drop weights.

Ordinary-combat callback admission is now cartridge-independent. The complete
retail inventory proves fifteen literal no-op touch identities and twelve literal
no-op shot identities; both production gates classify the bank-qualified callback
without probing its executable opcode byte.

Draygon's eight reachable health-band thresholds are compiled independently of
the selected color records. The real palette updater no longer reads the fixed
decision table; malformed restored health above the authored 6,000 maximum fails
before the native `$FFFF` terminator could lead it into adjacent executable data.

Botwoon's eight health-band thresholds are compiled separately from its palette
colors too. All signed 16-bit comparisons preserve the native one-band-per-frame
progression, while odd or out-of-range restored phase offsets fail before they can
select adjacent bank-$B3 code as threshold data.

The intro egg's six shell-fragment spawn positions are compiled with its physical motion
definitions too. Every final biased X/Y pair matches the cartridge, and all six complete
actor lifetimes run without reading either the position or velocity tables.

The ending reward gesture/jump owners now use typed records for all ten actor-definition
triples. Their initialization callback, no-op pre-instruction identity, and initial list
selector are compiled, while the selected animation and spritemap programs remain ROM
presentation data.

The adjacent final-logo owner now compiles its four initialization/pre-instruction/list
triples as well. Its complete approach and palette-crossfade path no longer reads those
definition records; the selected animation, spritemap, and palette data remain in ROM.

The intro Mother Brain owner now compiles its own definition triple, both explosion actor
triples, and all eight explosion placement/timer rows in a dedicated catalog. The complete
explosion lifetime runs with those 33 native words forbidden; animation lists, spritemaps,
and palettes remain ROM-backed presentation data.

The four intro Rinkas and their spawner now compile both actor-definition triples, all four
biased origins, and the parameter-selected signed horizontal velocity components. Both spawn
waves and the Mother-Brain-explosion cleanup path run with those eighteen native words
forbidden; the animation/spritemap programs remain ROM-backed presentation data.

The SR388 egg, confused baby, delivered baby, and examined baby now compile their four
actor-definition triples plus the fixed position/palette initializer payloads. The egg's
hatching threshold and both scientist-scene page transitions run with all 24 native words
forbidden; animation lists and spritemaps remain cartridge presentation data.

The six egg-shell fragments and shared slime-drop owner now compile all seven actor-definition
triples. Every fragment and slime-drop lifetime runs with those 21 callback/list words
forbidden; their already-compiled physical curves remain independent, while animation lists
and spritemaps remain cartridge presentation data.

The save-map codec now compiles all six live area records too: six counts, six packed-SRAM
offsets, six source pointers, and all 327 ordered explored-map byte indexes. Production save
and load round-trip the complete seven-area in-memory map with the bank-$81 codec sources
forbidden; the native seventh Ceres list remains deliberately excluded exactly as in
<c>SaveMap</c>/<c>LoadMap</c>.

The complete bank-$91 X-ray revealed-block dispatcher is compiled too. All 4,096 collision-
type/BTS inputs are compared against an independent traversal of the native two-stage table,
including all seven command identities and every multi-block metatile operand. The production
lookup, extension traversal, and tilemap builder no longer accept an address space for this
fixed policy; room block definitions and item/special-room overlay art remain separate live
presentation data.

All 29 bank-$85 gameplay-message definition records are compiled too, including both
content-boundary terminators. Setup/draw callback identities and payload boundaries now come
from typed metadata; every supported production message builds with `$85:869B-$85:8748`
forbidden. Border, message, button, and confirmation tilemaps remain cartridge presentation
assets and continue to be read by the fallback renderer.

## Additional inventory still required

Samus's fixed atmospheric-effect policy is now compiled too: all 28 movement-type
water-splash selectors, ten running foot-contact flags, and both identical 16-byte
Crateria room classifications. The real splash, running-footstep, and landing
consumers run with all four source ranges forbidden. Animation timer lists,
spritemaps, and liquid damage rates remain separate authored data and are not folded
into this policy catalog.

The neighboring atmospheric animation cadence and liquid damage rates are now
compiled as mechanics as well: 37 frame timers, seven frame counts, and the four
lava/acid fixed-point words. The complete real update paths run with those ranges
forbidden. The direct OAM attribute pointer/list data at `$90:8BFF+` remains
presentation data and is deliberately still ROM-backed pending its asset owner.

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
restored physical-origin indexes now fail explicitly instead of reading adjacent
bank-$9B data. The 28 cancellation bytes, 30 connection function/handler pairs,
eight special-angle/pose/offset/function records and 20 dropped-pose selectors
are now compiled too. Tests exercise actual dispatch with their source ranges
forbidden. Cross-table connection indexes remain native within the 30 compiled
records; non-catalog and unaligned pointers now fail explicitly.
The shared charge/Grapple flare cadence is now bus-free as well: six compiled
list-pointer bytes and 46 delay/loop bytes drive both production owners, while
addresses outside that bounded program fail explicitly instead of consuming
arbitrary adjacent bank-$90 movement code. Flare placement and composition remain
presentation data for #540/#541.
The separate bank-$90 HUD movement-handler table and twelve authored posture
flags are now compiled in `SamusHudDefinitions`, shared by Grapple admission
and projectile charge-preservation dispatch. All pose bytes retain native
posture threshold/adjacent-data behavior in a bounded compiled observation window;
there is no remaining runtime ROM fallback. Grapple's swing body offsets and their
physical angle selector are now compiled separately from the displayed art frame. Flare animation and
drawing pointers remain presentation readers for #540/#541; editable assets and
their restart/update behavior are not yet implemented by this separation.

Ordinary-running and Speed Booster cadence is now compiled: six eleven-byte
streams, six pointer words and five boost-counter reset words at $91:B5D1..B628,
plus the two adjacent pose-zero bytes selected by the real Max6 sound result.
These timings advance gameplay stages and are not editable cosmetic timing.
The sound-call accumulator bug remains intact, including stage-five selection
of the adjacent zero word and mutable low-bank delay `$91:0303`. Selections outside
the native zero-through-five result domain and frame indexes into unrelated high-bank
ROM now fail explicitly. Per-pose animation commands and artwork remain outside this
completed cadence group.

Pose byte four is shared by artwork and projectile origins. Its physical copy
for all 253 authored poses plus the three exact adjacent-code observations is
compiled independently for beam/missile setup, Grapple launch and the late
physical-origin update. Visual body/cannon/flare readers remain separate
presentation dependencies. Facing, movement type, no-input fallback pose, shot
direction, and collision radius likewise compile all 256 byte indexes. Per-pose
animation programs remain live readers; these completed fields do not complete metadata.
The authored input graph is now compiled separately in `SamusPoseInputDefinitions`:
253 pose mappings, 86 list identities and 598 ordered conditions. The production
matcher consumes those typed conditions directly, preserving empty/self-match
fallback semantics and native diagnostic entry addresses. Non-authored indexes
`$FD..$FF` retain the native raw-zero-input early return, but otherwise fail loudly
instead of parsing adjacent bank-$91 code as a graph. Animation programs are separate.
Collision radii are compiled for all 256 byte indexes, including the prospective
larger-pose and crouch-fallback consumers and three exact adjacent-code observations.
This physical catalog does not consume or replace the presentation offset.

The forty direction-specific beam/missile origin words at $90:C204..C253 are
now compiled too, with the standing/running and two special Moonwalk pose
branches preserved. Direction nibbles ten through fifteen still cross adjacent
rows; running Y eventually reaches the separately compiled cooldown bytes. Charge-flare
origins remain presentation data. Beam initial speeds, missile/Super Missile and
beam accelerations are now compiled as 85 native words, including the adjacent
ignition marker. Invalid beam combinations preserve their native reads into the
compiled missile rows; non-catalog and unaligned addresses now fail explicitly.
The 59 contiguous cooldown bytes (uncharged/charged/padding/non-beam/auto-fire)
are compiled too; combination indices beyond that range now fail explicitly instead
of reading adjacent SFX presentation bytes. Physical muzzle origins consequently no
longer require a runtime bus, including their native cross-row cooldown reads. The separate
previous-frame displacement omission discovered during this audit is addressed
under #600: the initializer now consumes native mutable movement/camera records,
with runtime producers/reset and saved-state continuation coverage. These WRAM
reads are legitimate live state, not remaining immutable lookup dependencies.

The four standalone horizontal records for diagonal bomb jumps and Grapple
release ($90:9F25/$9F31/$9F3D/$9F49) are now compiled. Exact-address recognition
preserves live reads only for bank-$90's mutable low-bank aliases.
Movement-indexed `ReadEntry` now compiles all 82 authored records: normal air has
26 rows, water and lava have 28 each. Address-based resolution preserves reads
across those boundaries, including normal-air indexes 26/27 entering water rows
0/1. Every movement byte and restored base word has been checked for exact
address/alignment handling. Non-catalog and unaligned high-bank addresses now fail
explicitly rather than interpreting adjacent executable or presentation bytes as
physics. Low-bank aliases genuinely remain mutable and retain live reads.

- `EnemyRomTablePointers` consumers: boss jumps, projectile launch/angle records,
  death trajectories and other indirect family definitions.
- Samus movement, combo and grapple definition readers. Bomb Spread's twenty
  launch words are now compiled and checked through the actual five-slot producer;
  its shared damage and selector metadata are now cartridge-independent as well.
  All 357 selector words and 40 damage headers are compiled, every translated
  beam/missile/bomb/SBA initializer uses those definitions, and an unknown address
  fails explicitly instead of interpreting adjacent animation or executable bytes
  as mechanics. All 805 timed-frame collision-radius pairs now follow the same rule:
  every production projectile and bomb frame uses the compiled physical bytes, while
  an address outside those records fails rather than reading mixed program/art data.
  The 1,816 duration, trail-frame, delete/goto, and branch-target words now also
  reject non-catalog addresses instead of falling back into arbitrary bank-$93 data.
  Both real instruction owners and trail spawning consume those compiled mechanics;
  spritemap pointers remain a separate presentation dependency. Coordinate
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

The shared bank-$86 misc-dust actor's thirty instruction selectors at
`$86:E42C-$E467` and five randomized smoke placements at `$86:E47E-$E4A5` are now
compiled in `MiscDustProjectileDefinitions`. Mother Brain, Ridley, Eye Door, and
the general room-graphics producer all consume the catalog with the original ROM
ranges forbidden. The animation programs and spritemaps selected by those fixed
mechanics identities remain presentation/program dependencies for the wider
#530/#540/#541 contract; this completed slice does not relabel them as editable art.

The four Sidehopper/Dessgeega animation-selector tables at `$A3:AAC2-$AAE1`
are now compiled as four typed variant records in `HopperAnimationDefinitions`.
Every floor/ceiling initializer, jump, and landing consumer uses that catalog with
the source span forbidden. The selected animation programs and their spritemaps
remain separate program/presentation dependencies.

Phantoon's nine eye-direction instruction selectors at `$A7:D40D-$D41E` are now
compiled in `PhantoonPatternDefinitions`. All eight reachable tracking octants
consume the catalog with the source range forbidden; the selected eye animation
programs and sprite presentation remain separate dependencies.

Choot's five real fall-stream selectors, five indirectly selected loop distances,
and all 450 physical X/Y frames are compiled. The two slower streams retain their
exact cartridge structure as dwell-expanded normal paths rather than duplicated
opaque rows. Exhaustive frame comparison plus real fall-loop execution reject ROM
reads across all five source ranges.

The five crawler-family initial orientation tables and all four shared six-species
surface tables are now compiled in `CrawlerAnimationDefinitions`. Shared crawlers,
Viola, Sciser, Zero, and HZoomer consume typed family/surface selectors with all 44
source words forbidden. Their selected instruction programs and spritemaps remain
separate program/presentation dependencies.

Maridia's large snail/Oum now compiles all eight mutually-exclusive facing/action
instruction selectors at `$A2:CB77-$A2:CB86`. Every production install consumes the
named definition catalog with the pointer table forbidden. The selected mixed
instruction programs and extended spritemaps remain separate dependencies.

Botwoon's four fixed hole rectangles, 32 fixed path descriptors, and complete 8,316-pair
signed movement corpus are now compiled in `BotwoonNavigationDefinitions`. The real target
movement, hole collision, path handoff, and forward/reverse path consumers no longer read
`$B3:949B-$94BA`, `$B3:A058-$E14F`, or `$B3:E150-$E24F`; native pointers and byte offsets
remain visible in saved/debugger state. Exhaustive verification compares every component
pair and executes every sample in both directions with all three source ranges forbidden.

The Norfair lava-jumping enemy's four signed 8.8 launch velocities at
`$A2:BE86-$BE8D` are now compiled in `NorfairLavaJumpDefinitions`. All 65,536
native RNG selectors run through the production state transition with that ROM range
forbidden; animation programs remain separate presentation/program dependencies.

The Zebes escape room-main explosion selector is compiled too: all eight fixed
sprite-object identities and eight optional sound IDs now share typed records.
All 128 random/inherited-index combinations run through the real producer with
`$8F:C1D6-$C1E5` forbidden, including the cartridge's high-random-nibble inherited-X
quirk. Sprite-object bytecode and artwork remain separate dependencies.

Ordinary Metroid behavior now compiles the four Power Bomb escape displacement
pairs at `$A3:EA3F-$EA4E` and the eight random cry sound IDs at `$A3:EAD6-$EAE5`.
The typed catalog accepts the complete countdown or RNG word and preserves the
native low-two- and low-three-bit selectors. All 65,536 countdowns run through the
real escape state transition and all eight cries run through the real instruction
interpreter with both source ranges forbidden. Mixed instruction programs and sprite
presentation remain separate dependencies.

All 120 translated bank-$86 `EprojDef` records are now compiled as complete typed
definitions: initialization callback, pre-instruction, initial instruction list,
packed radii, collision/damage properties, touch list, and shot list. The common
projectile initializer, Samus-contact handler, and projectile-shot handler consume
that catalog without rereading any of the fourteen-byte native records. Verification
compares all 840 words with the pinned cartridge and executes all three production
consumers for every definition with every source byte forbidden. The callbacks and
mixed animation programs selected by those fixed identities remain separate
program/presentation dependencies.

The complete palette-FX construction domain is compiled too: all 63 aligned bank-$8D
setup/list definition records, the eight bank-$83 area-list identities, and all 64
area/bit selections now live in `RoomPaletteFxDefinitions`. Generic room loading,
direct cinematic/Samus spawns, and the specialized Hyper Beam owner no longer reread
that fixed metadata. Production verification executes every definition, every retail
room-area selection, and the Hyper Beam's first palette frame with all definition and
area-selection bytes forbidden. The mixed palette instruction programs and color data
remain cartridge-backed program/presentation dependencies.

Mother Brain's eight escape-door fragment records are compiled too: each record pairs
its signed world offset from `$86:C992-$C9B1` with its signed 8.8 launch velocity from
`$86:C9B2-$C9D1`. All 32 words match the pinned cartridge and all eight production
spawns run with both physical tables forbidden. Fragment animation bytecode remains a
separate program/presentation dependency.

Mother Brain's twelve room-turret definitions are compiled too: fixed placements,
initial directions, native rotation-policy pointers and all 96 permission bytes now
share typed records. The eight directional animation selectors are paired with their
signed bullet offsets and 8.8 launch velocities. All twelve real turret initializers,
all eight real bullet spawns and every signed rotation step run with
`$86:BE89-$86:BF58`, `$86:BF9F-$86:BFDE`, and `$86:C040-$86:C04F` forbidden.
The mixed projectile animation programs selected by those fixed identities remain a
separate program/presentation dependency.

Mother Brain's glass-shard producer now compiles all sixteen RNG-angle instruction
selectors at `$86:CE41-$86:CE60` together with the three physical PLM-relative origins
at `$86:CE61-$86:CE6C`. All 48 parameter/angle production spawns run with the source
range forbidden. The selected mixed animation programs remain a separate dependency.

The four Tourian statue eye/soul world positions at `$86:B90E-$B91D` are compiled
as paired physical records. The catalog retains the native doubled boss parameters
zero, two, four, and six. All eight source words match the pinned cartridge, and both
the real eye-glow and soul spawns run for every boss with position reads forbidden.
Eye colors and projectile animation programs remain presentation/program dependencies.

The sixteen-entry enemy spike-reaction table at `$A0:C2DA-$C2F9` is compiled as a
bounded rule: indexes zero through fourteen remain solid, index fifteen spawns the
enemy-breakable PLM, and BTS bit seven is ignored. All sixteen native words and all
32 authored/high-bit production collision paths pass with the old table forbidden.
The 224 low-seven-bit selectors beyond the authored table now fail explicitly instead
of interpreting adjacent executable bytes as PLM headers.

Mother Brain's three body-relative death-explosion instruction selectors at
`$86:C929-$C92E` are compiled independently of their mixed animation programs.
All three native words and all three real production spawns pass with the selector
table forbidden, including body-relative placement and retained signed offsets.

Waver's four facing/spin instruction selectors at `$A3:86DB-$86E2` are compiled
behind a proven flags enum: bit zero is facing-right and bit one is spinning. All
four real list handoffs and both production initial facing paths run with the pointer
table forbidden. The mixed animation programs remain separate dependencies.

Skree and Metaree's four animation-phase selectors at `$A3:C69C-$C6A3` and
`$A3:894E-$8955` are compiled behind their shared mutually-exclusive phase enum.
All eight list installs and both live attack transitions per family run with both
pointer tables forbidden. Their animation programs remain separate dependencies.

Zoa's four facing/rise instruction selectors at `$A3:B40D-$B414` are compiled
behind a proven flags enum: bit zero selects rising versus shooting and bit one
selects right versus left. All four installs and both complete live directional
rise-to-shoot handoffs run with the pointer table forbidden.

Dragon's six body/wing/attack selectors at `$A2:E5EF-$E5FA` are compiled behind
a mutually-exclusive selector enum with an explicit reinstall sentinel. All six
installs, phase/facing mappings, and both live directional body/wing/attack paths
run with the pointer table forbidden. The animation programs remain separate.

The six enemy-pickup selectors at `$86:EF04-$EF0F` are compiled in
`EnemyPickupDefinitions`, retaining the cartridge's non-visual kind order and byte
offsets. All five live pickup initialization paths now avoid the pointer table;
the no-drop identity remains outside that authored table and fails explicitly.

The five shared enemy-death explosion selectors at `$86:EFD5-$86:EFDE` are compiled
in `EnemyDeathExplosionDefinitions`. Generic deaths, special Rinkas, and Zebetites
now share the same named small-explosion identity; every generic variant and the
native out-of-range clamp run with the pointer table forbidden. Mixed death animation
programs remain a separate dependency.

Ceres steam's six parallel instruction/function selections at `$A6:EFF5-$F00C`
are compiled as typed initialization records. All twelve source words match the
pinned cartridge, and all six production initialization paths run with both tables
forbidden. The four shared directional programs now compile all 68 engine-control
words too: every activation branch, visibility callback, duration, goto, and loop
target executes with those source bytes forbidden while all 36 interleaved extended-
spritemap operands remain live cartridge presentation data.

Normal and strong Brinstar Pipe Bugs' four facing/action instruction selectors at
`$B3:882B-$883A` are compiled behind a proven flags enum: bit zero selects shooting
versus rising and bit one selects right versus left. All eight production handoffs
run with both pointer tables forbidden. The mixed animation programs remain separate.

Draygon's six death/burial Evir records are compiled too. Their initial world
positions, XY 16.16 subspeed magnitudes and movement angles are now one typed
physical definition per actor. The real descending sprite-object allocation and
movement paths run with `$A5:A1AF-$A5:A1F6` forbidden; the unused zero word in
each native angle record is independently checked rather than exposed as data.

Fake Kraid's four signed 8.8 spit launches and three body-spike Y offsets are
compiled as projectile physics definitions. Both spit facings and all six
facing/row spike combinations run through the real projectile allocator with
`$A6:9A48-$A6:9A57` and `$86:9E7D-$86:9E82` forbidden.

The escape Etecoons' three position/pre-instruction/list/speed records are
compiled too. All six low-bit-masked retail selectors use typed definitions and
run through the production initializer with `$B3:E718-$B3:E735` forbidden.
Out-of-domain restored parameters now fail explicitly rather than retaining a
runtime bus solely to interpret adjacent tables and executable code.

Zebetites' four six-word generation records, ten health-tier instruction selectors,
and two embedded respawn populations are compiled as one barrier definition set.
All primary/linked initializers, every tier boundary, and both real respawn paths
run with `$A6:FC03-$A6:FC32`, the two spawn records, and `$A6:FD4A-$A6:FD5D`
forbidden. Palette animation and the selected instruction programs remain separate.

Mother Brain's embedded Baby Metroid population at `$A9:BE28-$A9:BE37` is compiled
as a complete physical spawn record. The real first-free-slot allocator,
initialization AI, palette load, immutable spawn snapshot and duplicate guard run
with the record forbidden. The Baby's route definitions remain separately compiled.

Yard's eight fixed direction records, opposite-direction selectors, movement-function
selectors, and the three identical airborne animation-list copies are compiled as
typed physical definitions. All eight real initializers and turns plus both-facing
detach, contact-kick, and shot-launch paths run with every migrated source forbidden.
Animation programs selected by these definitions remain separate dependencies.

Yard's twelve turn records at `$A3:CCE2-$A3:CD41` are compiled separately as
lookahead geometry plus outside/inside callback selectors. The catalog retains all
eight ordinary surface directions and the four zero-lookahead records used while
slope alignment suppresses turn transitions. Every record and all 24 real empty/solid
room crawl branches run with the complete source block forbidden. The selected mixed
animation programs remain separate dependencies.

Bomb Torizo's sixteen statue-hand fragment selections are compiled as complete
physical records, preserving the native eight-row wrap for Y position, velocity, and
acceleration. Every real room-graphics projectile allocation and initializer runs with
the five source tables forbidden; selected animation programs remain separate.

Bomb/Golden Torizo's eight randomized Chozo-orb, egg, and eye-beam launch tuples are
compiled as typed physical records. The shared real initializer retains independent
signed-byte velocity jitter, while the eye beam still replaces those base velocities
with its aimed launch. All family/facing spawn paths run with the eight tuples forbidden.

Bomb Torizo's recurring low-health drool now consumes the compiled shared signed-sine
catalog as well. Every eight-bit random angle plus both sixteen-angle facing cones run
through the real projectile producer while the complete `$A0:B443-$B642` word table is
forbidden; this closes a literal-address reader missed by the earlier named-table audit.
Mother Brain's five-joint neck solver now shares that compiled catalog too. All 65,536
angle/signed-distance products and complete geometry across every lower angle are compared
to an independent ROM-backed reference, and the solver no longer accepts a cartridge bus.
The named reference addresses remain available to cartridge-parity verification but have
no runtime consumer in either migrated path.

Bomb Torizo's eleven explosive-swipe and six low-health explosion placements are
compiled as paired physical records rather than anonymous runtime arrays. Every legal
swipe and every bounded facing/parameter explosion selection is checked against the ROM
and exercised through the real projectile allocators with both source tables forbidden.

Bomb Torizo's standing/sitting displacement copies and normal/faceless walking-velocity
copies are compiled as shared typed movement definitions. All authored offsets run
through the real posture and collision-aware walking consumers with the six native table
ranges forbidden; invalid odd or out-of-domain byte offsets fail explicitly.

Crocomire's hidden-wall rumble stream is compiled as typed target/timing records. The
complete retail schedule is compared word-for-word and replayed frame-for-frame against
an independent ROM-backed reference with `$A4:98CA-$A4:9909` forbidden to production.
The duplicate trailing terminator remains available to restored/debugger state.

All 36 room-shake types now pair their BG1/BG2 and enemy-projectile XY displacement
records in one compiled physical catalog. Both real consumers are exercised with positive
and alternating-negative phases while the complete bank-$A0 and bank-$86 source tables
are forbidden; frozen, expired, and non-rendered type boundaries remain unchanged.

Ridley's private Ceres-door draw hook now compiles all four effective signed X offsets
created by the cartridge's byte-indexing bug at `$A6:A321`. Every timer phase runs through
the real OAM producer with the malformed source table forbidden.

Ceres doors' seven population variants now compile their paired main-function and initial
instruction-list selectors. All fourteen native words match the pinned cartridge, and all
seven real initializers run with `$A6:F52C-$F539` and `$A6:F72B-$F738` forbidden. Variant
two's graphics transfer and the selected mixed instruction programs remain separate
presentation/integration dependencies.

Botwoon's complete fixed instruction-selector metadata is compiled too: eight visible head
orientations, the eight duplicated hidden-head entries, eight spit orientations, and all
32 visible/hidden body-and-tail orientations. All 56 native words match, while the real
movement, aim, and articulated-body consumers run with `$B3:946B-$949A` and
`$86:E9F1-$EA30` forbidden. The selected instruction programs remain separate dependencies.

The shared bank-$B4 room-sprite-object dispatcher now compiles all 62 initial instruction
selectors, including native object numbers not yet named by a translated caller. Every entry
runs through the real descending finite-pool allocator and first-frame loader with
`$B4:BDA8-$BE23` forbidden. All selected programs now compile their complete 552-word
control surface: frame durations, repeat/terminate/goto commands, and goto targets. The
real dispatcher executes every entry to termination or its authored loop with those bytes
forbidden, while all 471 interleaved spritemap pointers remain live cartridge presentation
reads. Exact ROM parity, strict rejection, and allocation-free warmed lookup are verified.

Shaktool's seven parallel initialization records at `$AA:DE95-$DEF6` are now
compiled in `ShaktoolSegmentDefinitions`, including property masks, chain ownership,
orbit angles, initial lists, layers, pre-instruction callbacks, and angular velocity.
All seven real initializers and the group callback reset run with the source range
forbidden. Its eight center-orientation selectors and both seven-segment collision/
dormant-attack tables are now compiled separately in `ShaktoolInstructionDefinitions`;
the real orientation, reversal, and dormant attack consumers run with those source
ranges forbidden.

Spore Spawn's four stalk offsets, four emitter positions, and complete 256-byte
wrapped signed spore movement stream are now compiled in
`SporeSpawnProjectileDefinitions`. All production spawn and movement/mirroring
consumers run with `$86:DCB9-$DCC0`, `$86:DCE6-$DCED`, and `$86:DD6C-$DE6B`
forbidden; animation programs and palettes remain separate dependencies.

Mama Turtle's 48 signed sleeping-shell contour words at `$A2:8E80` are now a
compiled physical definition shared by parent/Samus carry collision and Baby
Turtle crawling. Both asymmetric 24-pixel halves match the pinned cartridge.
The real sleeping-parent path passes every in-range distance plus both outside
boundaries through a helper that no longer accepts a ROM bus; sprite presentation
remains independent.

Both complete retail special-air dispatch tables are now compiled: seven areas by
sixteen inside-reaction entries and seven areas by sixteen collision-reaction
entries, resolving to 22 distinct typed bank-$84 header/setup identities. Real
Maridia surface movement runs with the dispatch rows, pointer words, setup words,
and all physical words forbidden. Restored BTS indexes beyond the authored
sixteen-entry domain fail explicitly instead of reading the following bank-$94
code as PLM pointers; the non-retail debug-area row is outside the typed domain.

All 68 aligned enemy-vulnerability records at `$B4:EC1C-$B4:F1F3` are compiled.
Common projectile, bomb, Power Bomb, contact-damage, boss, and Space Pirate paths
consume the catalog without an address-space parameter. External and unaligned
pointers fail explicitly instead of treating adjacent bank-$B4 code or presentation
data as damage multipliers. Enemy headers/populations remain ordinary cartridge
content; this completes only their fixed vulnerability-policy target domain.

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

## EnemyRomTablePointers live-reader audit

The remaining named readers were traced rather than classified from their field names.
Ceres Ridley's eye-fade byte stream at `$A6:E269-$E2A9` was the only remaining fixed
algorithm schedule: it selects sixteen palette rows, holds the final row, and publishes
the handoff to the body-fade phase. That complete 64-step schedule plus terminator is now
compiled in `CeresRidleyEyeFadeDefinitions`; every source byte matches the pinned ROM and
the complete production fade runs with schedule reads forbidden.

All other live `EnemyRomTablePointers` consumers are presentation dependencies: palette
colors, spritemap pointers, or graphics-transfer source/destination records. They remain
cartridge-backed for the presentation extraction/override work rather than being
misclassified as application-owned mechanics under #547.

Crystal Flash's ten interleaved body-palette records were also split by ownership. Their
uniform ten-call delays are compiled engine timing in
`CrystalFlashPaletteTimingDefinitions`; their palette pointers and colors remain live
presentation data. Every duration and the complete 100-call production cycle pass with
only the timer words forbidden.

The Work Robot's mixed palette records received the same ownership split. Its six timer
words and terminal wrap marker are compiled in `WorkRobotPaletteTimingDefinitions`, while
the four colors per record remain live presentation data. The complete production cycle
passes with timing and terminator reads forbidden.

Samus's mixed death-explosion records are split by ownership too. Their nine timer bytes
are compiled in `SamusDeathExplosionTimingDefinitions`; the interleaved palette selectors,
palette pointers, and palette colors remain live presentation data. The full production
sequence passes with all nine timer reads forbidden.

The Hyper Beam's specialized palette-FX owner now compiles the fixed control skeleton of
`$8D:D900`: destination setup, all ten two-call timers and done commands, terminal loop,
and the valid restored-pointer domain. Its eighty BGR555 color words remain live
presentation data. The complete production loop passes with control reads forbidden.

The Varia/Gravity pickup's 128-byte upper-half light-beam contour is compiled in
`SuitPickupBeamCurveDefinitions`. The production transformation still mirrors the contour
and applies the native asymmetric endpoint arithmetic, but no longer reads `$88:E3C9-$E448`.
Every offset and the complete Varia sequence pass with the source range forbidden.

## Samus special-sequence and Grapple ownership audit

The remaining `SamusSpecialSequenceRomData` consumers have been traced after compiling the
suit-pickup contour. Death tile-transfer sources and spritemap identities are presentation;
Power Bomb ellipse samples are already compiled through `PowerBombShapeTables`; the other
Power Bomb and shinespark members are named scalar mechanics rather than runtime lookups.
There is no further immutable mechanics-table read in that catalog.

The remaining `SamusGrappleRomData` read at `$9B:C1C2` selects only the displayed swing
animation frame. `GrappleSwingFrameCatalog` already exposes that 256-byte presentation
mapping as JSON, while `GrappleBodyPlacementDefinitions` independently owns physical body
placement. Compiling the visual fallback under #547 would prevent #535/#540/#549 from
replacing it and would not remove a mechanics dependency.

`SamusProjectileInheritance.ReadVelocity` was also inspected because its four unaligned
word reads resemble fixed operands. They intentionally read mutable WRAM `$0DA9/$0DAD/
$0DB1/$0DB5`, crossing the camera/movement fields recorded during the preceding frame.
Those reads must remain live state: compiling their observed values would erase movement
inheritance and the cartridge's documented adjacent-byte leakage.

## Mother Brain body instruction mechanics

Mother Brain's 18 ordinary body programs at `$A9:9730-$A9:9A41` are now split by
ownership. `MotherBrainBodyInstructionProgramDefinitions` compiles all 274 command and
frame-duration words that control world movement, BG2 compensation, pose, footsteps, and
AI-visible timing. The interleaved extended-spritemap pointers remain cartridge-backed
presentation data.

Both the encounter's generic enemy-instruction processor and the standalone rainbow-beam
sequence consume the compiled mechanics words, retaining ROM fallback only for presentation
and instruction streams outside this translated family. Verification compares every word
with the pinned cartridge and executes all 18 programs while every compiled source byte is
forbidden, while proving that live spritemap reads still occur.

## Mother Brain fake-death room-palette mechanics

Mother Brain's independent fake-death room-palette loop now compiles its fourteen
two-frame durations, goto opcode, and loop target in
`MotherBrainRoomPaletteProgramDefinitions`. The real production interpreter completes
and repeats the full flash sequence with all sixteen control words forbidden, while the
fourteen interleaved palette pointers and their BGR555 payloads remain live presentation
data. Exact ROM parity, every presentation read, strict rejection, and allocation-free
warmed lookup are verified.

## Mother Brain and misc-dust projectile instruction mechanics

Fixed control data for nine translated Mother Brain projectile programs is now compiled in
`EnemyProjectileInstructionMechanicsDefinitions`. The catalog contains 307
duration/opcode/operand words across those nine programs and all thirty misc-dust
programs. The generic dust selector and Mother Brain initializers share the same entry-point
definitions, so their instruction identities cannot drift.

The interpreter still reads each timed record's bank-$8D spritemap pointer from the ROM;
those words and sprite payloads are presentation. Mechanics lookups are bounded by exact
word address and reject adjacent/restored pointers rather than falling back to cartridge
code. Exhaustive verification compares every compiled word to the pinned ROM and executes
all 39 production programs with those source bytes forbidden, including the previously
unhandled looping misc-dust program `$86:E1FC`. The ordinary room-projectile producer now
also runs Mother Brain's exact six-stage, 30-frame rainbow-beam charging actor without
reading its fixed durations or terminal delete from the cartridge. Both normal and dying
drool variants likewise execute the five-stage head-attached interval, falling handoff,
twelve-pixel release correction, terminal sleep, and four-stage floor splash from compiled
control data. Mother Brain's rainbow-impact actor now also shares the already compiled
`$86:E152` misc-explosion owner instead of rereading that same 18-frame program through a
different projectile-kind path. All three body-relative death-explosion selectors likewise
name and execute the shared compiled small-explosion, smoke and big-explosion programs;
their real producers retain exact 31-, 32- and 30-frame lifetimes before deletion.

The ordinary room-projectile owner now also consumes the compiled onion-ring growth program
and its complete generic-contact impact list. The audit caught the packed X/Y-radius operand
still bypassing owner dispatch even after the surrounding opcode was compiled; that operand
now uses the same strict mechanics resolver. The real room producer reaches all six native
radii and terminal sleep, while the impact path applies its duplicate palette-zero opcode,
clears movement, displays six exact five-frame stages and deletes on frame 31.

Mother Brain's recursive red hand beam uses a dedicated
`MotherBrainHandBeamInstructionProgramDefinitions` catalog because its three callback
operands are byte-packed 24-bit executable identities rather than ordinary words. The
catalog compiles 25 duration/opcode/delete words and all three `$86:C7FB` callback
references while retaining 21 interleaved spritemap words as live presentation data. The
actual charging producer survives the exact three 13-frame stages, emits three fired child
actors through the compiled callbacks and deletes on frame 40 with every mechanics and
callback byte forbidden.

## Spore Spawn instruction mechanics

Spore Spawn's five private bank-$A5 programs now use
`SporeSpawnInstructionProgramDefinitions` for their fixed simulation data. The 116-word
catalog includes every duration, callback identity, callback operand, timer, wait, goto,
and branch target reached from defeated initialization, living initialization, combat
start, close-and-move, and death. The generic enemy interpreter and the boss's private
callbacks share that one strict lookup path.

The 41 words immediately following timed durations are spritemap pointers and deliberately
remain ROM-backed presentation. Verification compares every mechanics word with the pinned
cartridge, executes all five production entry points with mechanics reads forbidden, and
observes every one of those presentation words through the live bus. Invalid restored
pointers fail rather than escaping into adjacent bank data, and warmed lookups allocate no
per-frame storage.

## Ceres Baby instruction mechanics

The Ceres Baby draw interpreter's private `$A6:BF31-$BFC7` program is split by ownership
in `CeresBabyInstructionProgramDefinitions`. Forty-three fixed callback, branch-target,
and duration words are compiled mechanics. Thirty-three interleaved palette and spritemap
operands remain live presentation reads.

The production interpreter completes the entire two-list animation loop with all mechanics
bytes forbidden and every presentation operand observed. Focused cases also exercise the
stationary 50% random branch and the moving branch. Invalid restored pointers cannot enter
the adjacent native callback code, and warmed mechanics lookups allocate no storage.

## Rinka instruction mechanics

The ordinary and Mother Brain Rinka lists at `$A2:B9E0-$BA36` now resolve simulation
control through `RinkaInstructionProgramDefinitions`. The 26 compiled words cover both
visibility callbacks, the fire callback, every frame duration, and both terminal goto
targets. Their 18 interleaved spritemap operands remain live presentation reads.

The production interpreter runs both variants through a complete loop with the control
bytes forbidden and observes all 18 spritemap operands through the cartridge bus. A
restored Rinka cursor cannot escape the two authored lists, and the retail-unused
`$A2:B9A2` conditional callback is rejected because no retail list supplies its operand.
Warmed mechanics lookup remains allocation-free.

## Fune/Namihe instruction mechanics

The eight active/idle and left/right Fune/Namihe programs now resolve all fixed control
through `FuneNamiheInstructionProgramDefinitions`. Sixty-two duration, callback,
sleep/goto, and loop-target words are compiled; the 38 interleaved spritemap operands
remain live presentation reads.

Production verification runs all eight entry programs while every control byte is
forbidden and observes all presentation operands through the cartridge bus. The four
active programs also prove directional fireball allocation, sound publication, and the
finish callback's return to Fune cooldown or Namihe proximity ownership. Restored cursors
outside the authored lists fail loudly, and warmed lookups are allocation-free.

## Atomic instruction mechanics

Atomic's four selector-owned bank-$A8 animation loops now use
`AtomicInstructionProgramDefinitions` for all 32 durations, goto opcodes, and loop
targets. The 24 interleaved spritemap operands remain live presentation reads, and the
existing initializer-selector catalog points at the same named entries.

All four production loops execute beyond their terminal goto with the mechanics bytes
forbidden and every spritemap operand observed through the cartridge bus. Invalid
restored cursors fail instead of entering adjacent bank data, and warmed catalog lookup
allocates no per-frame storage.

## Hibashi instruction mechanics

Hibashi's visible-eruption and invisible-hitbox instruction streams now share a
strict `HibashiInstructionProgramDefinitions` boundary. Fifty immutable words cover
all durations, callbacks, and terminal sleep commands. The 24 interleaved spritemap
operands remain live presentation reads.

Production verification executes both paired programs with every compiled mechanics
byte forbidden, observes every spritemap, and asserts eruption sound publication,
all 22 hitbox placements, final collision removal, actor visibility, and both sleep
cursors. Presentation operands and callback code addresses are rejected as restored
mechanics cursors, and warmed lookup is allocation-free.

## Blue Brinstar face-block instruction mechanics

The neutral initializer program and both directional activation programs now share a
strict `BlueBrinstarFaceBlockInstructionProgramDefinitions` boundary. Ten immutable
duration/sleep words are compiled; seven interleaved spritemap operands remain live
presentation reads.

Production verification runs the actual initializer plus left- and right-side Samus
activation through each terminal sleep with every compiled mechanics byte forbidden.
It observes all seven spritemaps, rejects presentation and adjacent native-code pointers
as restored mechanics cursors, and proves warmed lookup is allocation-free.

## Boulder instruction mechanics

Boulder's left- and right-moving rolling loops now use a strict
`BoulderInstructionProgramDefinitions` boundary. Twenty immutable duration, goto, and
loop-target words are compiled; sixteen interleaved spritemap operands remain live
presentation reads.

Production verification exercises both real initializer selections, runs each loop past
its terminal goto with every compiled mechanics byte forbidden, and observes all sixteen
spritemaps. Presentation and adjacent-data pointers fail as restored mechanics cursors,
and warmed lookup is allocation-free.

## Boyon instruction mechanics

Boyon's idle and bouncing programs now use a strict
`BoyonInstructionProgramDefinitions` boundary. Eighteen immutable property commands,
callbacks, durations, gotos, and loop targets are compiled; ten interleaved spritemap
operands remain live presentation reads.

Production verification runs both loops past their terminal gotos with every compiled
mechanics byte forbidden, observes all ten spritemaps, and asserts the idle/bouncing
off-screen-processing transition plus the native bounce sound and arc-permission callback.
Presentation and adjacent-data pointers fail as restored mechanics cursors, and warmed
lookup is allocation-free.

## Skultera instruction mechanics

Skultera's two swimming loops and two turning programs now share a strict
`SkulteraInstructionProgramDefinitions` boundary. Thirty-two immutable layer callbacks,
durations, gotos, loop targets, completion callbacks, and sleep words are compiled;
twenty-two interleaved spritemap operands remain live presentation reads.

Production verification exercises both real initializer selections and both turn-entry
paths, runs every program to its goto or sleep boundary with all compiled mechanics bytes
forbidden, and observes every spritemap. It also proves the layer-two/layer-six callbacks
and both turn-completion flags execute. Presentation and adjacent native-code pointers fail
as restored mechanics cursors, and warmed lookup is allocation-free.

## Waver instruction mechanics

Waver's left/right steady and spinning programs now share a strict
`WaverInstructionProgramDefinitions` boundary. Sixteen immutable durations,
spin-completion callbacks, and sleep words are compiled; ten interleaved spritemap
operands remain live presentation reads. The separately verified selector table now
names these program entries rather than duplicating their numeric addresses.

Production verification runs every program through its terminal sleep with all compiled
mechanics bytes forbidden, observes all ten spritemaps, and proves both spinning callbacks
publish completion. Presentation and adjacent selector-table pointers fail as restored
mechanics cursors, and warmed lookup is allocation-free.

## Skree and Metaree instruction mechanics

Skree and Metaree's parallel idle, preparation, diving, and authored stop programs now
share a strict `SkreeMetareeInstructionProgramDefinitions` boundary. Forty immutable
durations, property commands, ready callbacks, gotos, targets, and sleeps are compiled;
twenty-two interleaved spritemap operands remain live presentation reads. The separately
verified phase selectors now name these eight catalog entries.

Production verification runs every program through its loop or sleep boundary with all
compiled mechanics bytes forbidden, observes every spritemap, and proves both ready flags
plus both off-screen-property transitions. Each species rejects the other's program domain,
presentation pointers fail as restored mechanics cursors, and warmed lookup is allocation-free.

## Zoa instruction mechanics

Zoa's left/right shooting and rising programs now share a strict
`ZoaInstructionProgramDefinitions` boundary. Twenty-six immutable speed callbacks,
durations, gotos, and loop targets are compiled; twelve interleaved spritemap operands
remain live presentation reads. The separately verified facing/phase selector uses these
same four named entries.

Production verification runs every program beyond its loop boundary with all compiled
mechanics bytes forbidden, observes every spritemap, and proves the complete three-stage
speed callback schedule in both directions. Presentation and adjacent selector-table
pointers fail as restored mechanics cursors, and warmed lookup is allocation-free.

## Norfair heat palette program selection

The Samus-in-heat pre-instruction now resolves all forty-eight fixed Power/Varia/Gravity
phase program selectors through `PaletteFxHeatInstructionListDefinitions`. The sixteen
published phase values and Gravity-before-Varia equipment priority remain cartridge-exact;
out-of-domain restored phases fail explicitly instead of reading adjacent bank-$8D code.
The three selected programs now compile their setup, sixteen durations, sixteen waits,
and terminal loop as well: 114 additional control words are owned by
`PaletteFxHeatProgramMechanicsDefinitions`, while all 720 BGR555 color words remain live
presentation data.

Verification compares every selector word to the pinned cartridge, exercises all four
equipment-priority cases, proves allocation-free warmed lookup, and runs the real paired
Norfair heat owners while all bytes of `$8D:E3E0-$E43F` are forbidden. Each suit also runs
through its complete real loop with every program mechanics byte forbidden and every live
color read observed. Damage accumulation, the one-frame shared-phase handoff, and
environmental sound cadence remain asserted.

## Wrecked Ship green-light palette mechanics

The powered Wrecked Ship green-light program shared by palette-FX definitions `$F76D`
and `$F771` now compiles its color-index setup, eight durations, eight waits, goto, and
loop target through `WreckedShipGreenLightPaletteFxProgramMechanicsDefinitions`. Its
sixteen BGR555 colors remain live presentation data. The room palette interpreter now
routes translated mechanics through the shared `RoomPaletteFxProgramMechanicsDefinitions`
owner rather than accumulating family checks in functional code.

Verification compares all twenty control words with the pinned cartridge, executes both
real definitions through a complete loop with mechanics bytes forbidden, observes all
live color reads plus the repeated first frame, rejects adjacent code, and retains the
exhaustive palette-FX and allocation checks.

## Room-FX animated-tile mechanics

The five simple room-FX animated-tile objects are now split by ownership too. Lava,
acid, rain, Maridia ceiling sand, and Maridia falling sand compile their five object
headers plus every frame-duration word, terminal loop opcode, and loop target in
`RoomFxAnimatedTileMechanicsDefinitions`. That is 48 fixed control words across 23
timed frames. Their interleaved frame-source pointers and character bytes remain live
cartridge presentation data.

Verification compares all 48 compiled words with the pinned cartridge and executes a
complete loop for every production object while every mechanics byte is forbidden. It
also proves that all 23 presentation-source operands are still read from the cartridge.
Unknown/non-retail definitions retain the strict generic interpreter; a restored stock
object cannot escape its compiled control domain into arbitrary adjacent bank-$87 data.

The two Wrecked Ship treadmill objects use a separate boss-gated interpreter and are now
compiled independently in `WreckedShipTreadmillMechanicsDefinitions`. Their two headers,
two boss-wait commands, eight frame durations, two loop opcodes, and two loop targets total
20 additional mechanics words. Both directions execute the wait, all four frames, and the
loop with every control byte forbidden, while their eight source operands remain live
presentation references. Constructed non-retail definitions still exercise strict generic
dispatch and unknown-command failure.

The four Tourian entrance-statue objects are now split at the same ownership
boundary. `TourianStatueAnimatedTileMechanicsDefinitions` compiles their four
headers and complete event/boss/palette/effect control graphs: 184 immutable words
covering transfer geometry, timed records, branches, animation-state serialization,
event publication, palette destinations, and unlock-effect parameters. The nine
interleaved character-source operands per statue remain live presentation references,
for 36 replaceable artwork pointers in total.

Verification compares every compiled word to the pinned cartridge, proves every
presentation operand is absent from the mechanics catalog, then enters the real
`$8F:A66A` room and completes all four defeated-boss release programs while a guarded
bus rejects any mechanics-byte read. All 36 live source operands are observed during
that production execution. This completes the current bank-$87 runtime mechanics-owner
inventory; the broader bank/indirect caller audit continues under #547.

## Projectile sound routing mechanics

The ordinary/charged beam sound tables at `$90:C28F-$C2C6` are compiled in
`SamusProjectileSoundRoutingDefinitions` as library-one routing policy shared by
#547 and #548. The domain deliberately contains all sixteen raw low-nibble selectors,
not just the twelve authored beam combinations: uncharged selectors C-F observe the
first four charged words, while charged selectors C-F observe the first four non-beam
words. This preserves Chainsaw/SpaceTime/Murder Beam adjacency rather than sanitizing it.

Verification compares 32 indexed observations (24 authored plus eight bounded overreads)
to the pinned cartridge and rejects indexes outside the four-bit domain. The actual beam
producers exercise all twelve ordinary and all twelve charged routes while their old
tables contain poisoned values; Hyper Beam selects the compiled charged-Plasma sound, and
the real Murder Beam path runs with the complete source range forbidden while retaining
its native zero-sound result.

## Music upload routing mechanics

The bank-$8F table that maps a queued music-data byte offset to an SPC upload stream is
now represented by the same `AudioAssetCatalogData` definitions used by extraction and
manifest validation. `CartridgeAudioState` no longer reads `$8F:E7E1` at dispatch time.
All 25 authored routes match the pinned cartridge, an overlapping non-identity byte
offset fails loudly, and a real queued title-bank command resolves correctly when the old
pointer bytes are unavailable. Sequence, instrument, envelope, and sample bytes remain
audio content for #548 rather than application mechanics for #547.

## Area animated-tile object selection

The complete bank-$83 area-to-animated-tile-object selector is compiled too: eight list
pointers and all 64 bit-selected bank-$87 object headers. The eighth native row is retained
for exhaustive parity even though typed production areas expose only the seven retail rows.
Both Maridia sand and Wrecked Ship treadmill population execute through their real loaders
while every byte of `$83:AC56-$83:AC65` and the eight pointed lists is forbidden. The
selected objects' instruction mechanics retain their independently compiled owners, while
character-source operands and graphics remain presentation data.

The audit also exposed that the compiled 253-entry Samus pose-to-input-list map was declared
as a collection-expression span property. The current compiler materialized that property
twice per lookup, allocating 144 bytes on every gameplay query despite the immutable data.
It is now one static array: the exhaustive 253-pose/598-condition parity remains unchanged,
and the existing 65,536-call production allocation gate is zero again.

## Dragon instruction mechanics

Dragon's six sleeping-body, cosmetic-wing, and attacking-body programs now resolve all
twenty-six fixed durations, gotos, loop targets, completion callbacks, and sleeps through
`DragonInstructionProgramDefinitions`. Their sixteen interleaved spritemap pointers remain
live cartridge-backed presentation data, and the six-way phase/facing selector names the
same catalog entries instead of duplicating raw addresses.

Verification compares every mechanics word with the pinned cartridge and executes all six
production programs through their loop or sleep boundary while every mechanics byte is
forbidden. Both attack programs publish their completion flag, all sixteen presentation
operands remain observable, presentation and adjacent selector-table pointers fail as
mechanics, and warmed lookup is allocation-free.

## Norfair Pipe Bug instruction mechanics

Norfair Pipe Bugs now resolve all thirty-six fixed durations, gotos, and loop targets
across the formation's four left/right rising and flight programs through
`NorfairPipeBugInstructionProgramDefinitions`. Their twenty-eight interleaved spritemap
pointers remain live cartridge presentation data, and the state-machine handoffs name the
same catalog entries instead of retaining raw addresses.

Verification compares every mechanics word with the pinned cartridge and executes all
four production loops beyond their terminal gotos while every mechanics byte is forbidden.
All presentation operands remain observable, invalid mechanics pointers fail loudly, and
warmed lookup is allocation-free.

## Yellow Pipe Bug instruction mechanics

Yellow Brinstar Pipe Bugs now resolve all twenty-four fixed durations, gotos, and loop
targets across the left/right straight and arcing programs through
`YellowPipeBugInstructionProgramDefinitions`. Their sixteen interleaved spritemap pointers
remain live cartridge presentation data, and every state-machine handoff names the same
catalog entries instead of retaining raw addresses.

Verification compares every mechanics word with the pinned cartridge and executes all
four production loops beyond their terminal gotos while every mechanics byte is forbidden.
All presentation operands remain observable, invalid mechanics pointers fail loudly, and
warmed lookup is allocation-free.

## Brinstar Pipe Bug instruction mechanics

Normal and strong Brinstar Pipe Bugs now resolve all sixty fixed durations, gotos, and
loop targets across their eight rising and shooting programs through
`BrinstarPipeBugInstructionProgramDefinitions`. Their forty-four interleaved spritemap
pointers remain live cartridge-backed presentation data, and the two four-way selector
tables name the same catalog entries instead of duplicating raw addresses.

Verification compares every mechanics word with the pinned cartridge and executes all
eight production loops beyond their terminal gotos while every mechanics byte is
forbidden. All forty-four presentation operands remain observable, presentation and
adjacent selector-table pointers fail as mechanics, and warmed lookup is allocation-free.

## Shaktool attack-circle projectile instruction mechanics

All three unused-but-authored Shaktool attack-circle programs now compile their eighteen
fixed duration, callback, goto, and target words. Eight spritemap operands remain live
presentation data. The real linked producer and complete stable loops execute with mechanics
reads forbidden, including both delayed movement-callback handoffs and the cartridge's
physical front-slot ownership link. Exact ROM parity, presentation reads, strict rejection,
and allocation-free warmed lookup are verified.

## Generic enemy-death instruction mechanics

The five generic death animations and shared blank respawn tail now compile all sixty-six
fixed mechanics words. This includes random sprite-object operands, timer/decrement loops,
sound callbacks, pickup conversion, respawn handling, and deletion. Thirty-one spritemap
operands remain live presentation data. All real death variants execute to their exact
conversion frames and through tail deletion with mechanics bytes forbidden; cartridge
parity, sounds, callback effects, every presentation read, strict rejection, and
allocation-free warmed lookup are verified.

## Spore Spawn projectile instruction mechanics

The stalk, ceiling-emitter, and airborne-spore projectile families now compile all
twenty-eight fixed control words while retaining seventeen spritemap operands as live
presentation data. All three real producers execute through the emitter release, spore
loop, stalk sleep, and complete shot/drop/deletion path with mechanics bytes forbidden.
Exact ROM parity, callback effects, every presentation read, strict rejection, and
allocation-free warmed lookup are verified.

## Botwoon projectile instruction mechanics

All seventeen selector-reachable Botwoon body/tail programs and the spit loop now compile
their seventy-three fixed duration, sleep, goto, and target words. Forty-six spritemap
operands remain live presentation data. The real body and spit producers execute every
program through its stable sleep or loop with mechanics bytes forbidden; exact ROM parity,
every presentation read, strict rejection of adjacent unused data, and allocation-free
warmed lookup are verified.

## Torizo landing-dust instruction mechanics

The paired right- and left-foot Torizo landing-dust programs now compile all sixteen fixed
duration, movement-callback, and deletion words while retaining eight spritemap operands
as live presentation data. The real dual-foot producer executes both complete sequences,
including symmetric spawn coordinates, three four-pixel rises, and deletion, with mechanics
bytes forbidden. Exact ROM parity, every presentation read, strict rejection, and
allocation-free warmed lookup are verified.

## Bomb Torizo explosive-swipe instruction mechanics

Bomb Torizo's explosive-swipe program now compiles its seven fixed opcode, duration, and
deletion words while retaining five spritemap operands as live presentation data. The real
producer executes with ordinary frame timers through its exact 25-frame visible lifetime
and next-tick deletion with mechanics bytes forbidden. Exact ROM parity, every presentation
read, strict rejection, and allocation-free warmed lookup are verified.

## Bomb Torizo drool instruction mechanics

Both Bomb Torizo drool identities now compile nineteen fixed instruction words while
retaining seven spritemap operands as live presentation data. The real recurring producer
restores the native first RNG draw and eight-entry 0/2/4-frame delay selection before its
trajectory draw; the gut-break producer preserves its distinct no-delay two-draw setup.
Both producers execute through the priority handoff, stable loop, immediate wall deletion,
and 24-frame floor impact with mechanics bytes forbidden. Exact ROM parity, RNG ordering,
every presentation read, strict rejection, and allocation-free warmed lookup are verified.

## Torizo explosion instruction mechanics

Bomb Torizo's low-health and death-explosion actors now compile fifty-three fixed control
words while retaining fifteen spritemap operands as live presentation data. The real
producers execute all three low-health cycles and both one-in-four death branches through
their exact random placement, timer, lifetime, and deletion behavior with mechanics bytes
forbidden. The probability branch target now uses the owner resolver too. Exact ROM parity,
RNG consumption, every presentation read, strict rejection, and allocation-free warmed
lookup are verified.

## Torizo Chozo-orb instruction mechanics

Bomb and Golden Torizo's shared Chozo-orb actors now compile forty fixed control words
while retaining eighteen spritemap operands as live presentation data. Both real producers
execute in both facings through their 85-frame loops, and the wall, floor, and shot/drop
paths reach their exact property changes, lifetimes, native drop headers, and deletion.
Drop operands now use the owner resolver too. Exact ROM parity, every presentation read,
strict rejection, and allocation-free warmed lookup are verified.

## Torizo sonic-boom instruction mechanics

Bomb and Golden Torizo's shared sonic-boom actors now compile thirty-one fixed control
words while retaining eleven spritemap operands and two packed sound IDs as live
presentation/audio data. Both real producers execute in both facings through their launch
poses and stable moving loops. A synthetic solid wall exercises the real movement callback
and all five twelve-frame random impact cycles through collision disabling, high-priority
drawing, center restoration, jitter, and deletion. Exact ROM parity, RNG consumption,
every presentation read, strict rejection, and allocation-free warmed lookup are verified.

## Bomb Torizo statue-fragment instruction mechanics

All sixteen breaking-statue fragment programs now compile their ninety-six fixed control
words while retaining thirty-two spritemap operands and sixteen packed sound IDs as live
presentation/audio data. The already-compiled physical fragment definitions now select
their programs through this catalog rather than duplicating raw pointers. Every real
fragment producer executes its authored staggered wait, installs the falling callback,
holds the second pose for 112 frames, and deletes with mechanics bytes forbidden. Exact
ROM parity, every presentation read, strict rejection, and allocation-free warmed lookup
are verified.

## Golden Torizo egg instruction mechanics

Golden Torizo's egg now compiles fifty-three private control words and resolves its
already-compiled shared shot-break program through the egg owner. Twenty-six spritemap
operands and two packed sound IDs remain live presentation/audio data. Both real facing
producers execute through the initial sleep, native hatch handoff, property transition,
24-frame charge loop, real synthetic-floor break, facing-specific break lifetime, and
shared shot break with mechanics bytes forbidden. Exact ROM parity, launch RNG count,
every private presentation read, strict rejection, and allocation-free warmed lookup are
verified.

## Golden Torizo reflected-Super-Missile instruction mechanics

Golden Torizo's reflected Super Missile now compiles forty-three fixed control words while
retaining twenty-four spritemap operands and one packed impact sound ID as live
presentation/audio data. Both real facing producers execute through their held pose,
opposite aim callbacks, thrown-callback install, and complete sixteen-frame flight loops.
A synthetic solid wall exercises the real collision handoff and the definition's shot path
shares that same compiled six-pose impact. Exact radii/property changes, lifetime, ROM
parity, every presentation read, strict rejection, and allocation-free warmed lookup are
verified with mechanics bytes forbidden.

## Golden Torizo eye-beam instruction mechanics

Golden Torizo's eye beam now compiles twenty-eight fixed control words while retaining
seventeen spritemap operands and one packed floor-impact sound ID as live
presentation/audio data. Both real facing producers execute through their complete
five-frame flight loop. Synthetic wall and floor collisions exercise the real movement
handoffs, exact floor alignment, twenty-frame wall burst, disabled blank floor loop, and
enabled thirty-nine-frame damaging explosion. ROM parity, every presentation read, strict
rejection, and allocation-free warmed lookup are verified with mechanics bytes forbidden.

## Enemy-projectile instruction-owner completion guard

The ordinary bank-$86 enemy-projectile instruction resolver no longer contains a generic
ROM fallback. Every translated projectile definition's nonzero initial, touch, and shot
entry resolves through its family catalog or the compiled shared delete program while a
read-forbidden address space is installed. Unknown identities and pointers now fail
explicitly without probing cartridge data. Family-specific traversal tests remain the
proof for internal branches and live presentation operands; this guard proves definition
entry coverage and prevents new projectile kinds from silently restoring the fallback.

## Ordinary-enemy instruction-owner inventory

The retail-wide owner audit now enumerates all 163 named enemy definitions and every
population record in all 323 compiled room states. It invokes the real initializer
dispatcher, preserves each record's initial instruction/property words, and probes the
production mechanics resolver with cartridge reads forbidden. This covers 154 translated
initializer identities and proves all 105 instruction-processing owners. The terminal
cartridge fallback has been removed; an unknown definition now fails with its identity
and pointer instead of reading fixed ROM mechanics. Family-specific traversal tests remain
the proof for internal branches and live presentation operands, while this audit prevents
a newly translated actor from silently restoring the fallback.

## Gunship instruction mechanics

The top hull, bottom hull, and entrance-pad definition now compile all 28 fixed duration,
sleep, goto, and target words across their five program paths. The real top, bottom, and
entrance-pad initializer routes install those compiled programs, the complete opening and
closing animations reach their native open/closed loops, and both hulls reach terminal
sleep with mechanics bytes forbidden. All 22 interleaved spritemap operands remain live
cartridge presentation data.

## Mother Brain initial body instruction mechanics

Mother Brain's previously omitted initial dummy frame and its dormant terminal sleep now
share the compiled body-program owner used by all eighteen active walk/posture programs.
The real body initializer selects that named program, the zero-duration frame retains its
native timer wrap, and its single spritemap operand remains live presentation data. The
body definition now rejects unknown mechanics addresses instead of falling back to bank
$A9 cartridge code.

## Ridley instruction mechanics

The shared Ceres and Lower Norfair Ridley owner now compiles all 221 command, timing,
branch-target, pose-distance, and movement-displacement words reachable from its nine
production entry programs. Both facing paths execute through the real ordinary-enemy
interpreter, including the Ceres and Norfair liftoff handoffs, while all compiled mechanics
bytes are forbidden. The 86 interleaved extended-spritemap operands remain live cartridge
presentation data. Exact ROM parity, every presentation read, strict rejection, and
allocation-free warmed lookup are verified.

## Draygon instruction mechanics

Draygon's body, eye, tail, and arms now share a compiled owner for all 524 command,
timing, branch-target, displacement, sound, and function words reachable from the 37
production entry programs. The 250 interleaved extended-spritemap operands remain live
presentation data. Verification matches every compiled word against the pinned ROM,
routes every word through all four physical definition identities with cartridge reads
forbidden, exercises the atomic four-list reset command, and covers both native HUD IRQ
opcodes (including the right-facing duplicate at `$A5:9C8A`). This completes the retail
ordinary-enemy owner inventory and removes its generic mechanics fallback.

## Walking Space Pirate instruction mechanics

All eight walking Space Pirate body programs now compile their 92 function, timing,
laser-offset, branch, and target words. Both complete three-shot attacks, the two patrol
loops, both flinches, and both look-around handoffs execute through the real ordinary-enemy
interpreter with mechanics bytes forbidden. Fifty extended-spritemap operands remain live
cartridge presentation data. Wall Pirate mechanics are covered below; ninja Pirate body
programs are covered below.

## Wall Space Pirate instruction mechanics

All eight wall Space Pirate body programs now compile their 150 function, timing,
movement, branch, and target words. Both laser/jump attacks, both landed fallthroughs,
and all four climb directions execute through the real ordinary-enemy interpreter. Both
walls also reverse through the real solid-collision path with mechanics bytes forbidden.
Forty-two extended-spritemap operands
remain live cartridge presentation data. Ninja Pirate mechanics are covered below.

## Ninja Space Pirate instruction mechanics

All twenty production ninja Space Pirate body programs now compile their 308 function,
palette, sound, claw geometry, timing, branch, and target words. Both claw attacks, both
spin jumps, active/flinch/kick programs, both divekick phases, both return walks, initial
loops, and landing handoffs execute through the real ordinary-enemy interpreter with
mechanics bytes forbidden. The claw and dive side effects are asserted directly, while
140 extended-spritemap operands remain live cartridge presentation data.

## Tourian entrance-statue projectile instruction mechanics

All eight Tourian entrance-statue projectile families now compile their fifty-eight fixed
control words while retaining twenty-eight spritemap operands as live presentation data.
The actual eye release, particles, tails, splash, soul, base decoration, Ridley, and
Phantoon actors execute through their native loops and deletion paths with mechanics bytes
forbidden. The particle-tail Y displacements now use the owner resolver too. Exact ROM
parity, callback effects, timer wrap, every presentation read, strict rejection, and
allocation-free warmed lookup are verified.

## Enemy-pickup instruction mechanics

All five live bank-$86 enemy-pickup animation programs now compile their thirty fixed
duration, goto, target, and sleep words. The sixteen spritemap operands remain live
presentation data. The owner accepts both direct `$F337` pickups and `$F345` death actors,
because the cartridge converts a completed death animation into a pickup without changing
the actor's definition identity. Exact ROM parity, all complete loops, dormant sleeps, both
owner identities, every presentation read, strict rejection, and allocation-free warmed
lookup are verified.

## Wrecked Ship Chozo and Tourian dust instruction mechanics

The two Wrecked Ship Chozo spike-clearing programs and the Tourian entrance-statue descent
dust now compile all thirty-one fixed instruction words, including their packed random
placement parameters and the complete 64-cycle Tourian loop. Fourteen spritemap operands
remain live presentation data. The real footstep and Tourian producers plus the authored
alternate execute through their exact deletion ticks with mechanics bytes forbidden;
cartridge parity, random placement, every presentation read, strict rejection, and
allocation-free warmed lookup are verified.

## Tourian statue grey palette mechanics

The four statue-specific entries and their shared eight-frame grey fade now compile all
31 color-index, branch, duration, wait, and deletion words through
`TourianStatueGreyPaletteFxProgramMechanicsDefinitions`. The 64 BGR555 colors remain live
presentation data. Constructed opcode tests now use a dedicated non-retail program seam
instead of overwriting a stock statue entry that has acquired compiled ownership.

Verification compares every mechanics word with the pinned cartridge and executes all
four real palette-FX definitions through deletion with mechanics bytes forbidden. Every
live color read remains observable and every presentation address is excluded from the
mechanics catalog.

## Torizo belly palette mechanics

Bomb and Golden Torizo's matching six-frame belly loops now compile all 36 color-index,
pre-instruction, duration, wait, branch, and target words through
`TorizoBellyPaletteFxProgramMechanicsDefinitions`. Their 36 distinct BGR555 colors remain
live presentation data. Both real definitions execute through a full cycle and repeat the
first frame with mechanics bytes forbidden, then delete through the cartridge's shared
enemy-zero-death pre-instruction. Exact ROM parity, every live color read, and adjacent-code
rejection are verified.

## Brinstar blue-spore palette mechanics

The standard Brinstar and Spore Spawn room variants now compile all 66 color-index,
pre-instruction, duration, wait, branch, and target words through
`BrinstarBlueSporePaletteFxProgramMechanicsDefinitions`. Their 84 BGR555 color words
remain live presentation data. Both real definitions execute a complete fourteen-frame
cycle and repeat the first frame with mechanics bytes forbidden. The Spore Spawn variant
then deletes through its area-mini-boss callback while the standard-room owner remains
active. Exact ROM parity and every live color read are verified.

## Red Brinstar background-glow palette mechanics

Red Brinstar's fourteen-frame background-glow loop now compiles all 32 color-index,
duration, wait, branch, and target words through
`RedBrinstarGlowPaletteFxProgramMechanicsDefinitions`. Its 112 BGR555 colors remain live
presentation data. The real definition executes a complete 140-frame cycle and repeats
the first frame with mechanics bytes forbidden. Exact ROM parity and every live color read
are verified.

## Crateria lightning palette mechanics

The live surface-lightning and unused dark-lightning programs now compile all 78 setup,
duration, wait, timer, branch, and target words plus their four byte-sized timer operands
through `CrateriaLightningPaletteFxProgramMechanicsDefinitions`. Their 202 BGR555 color
words remain live presentation data. The shared resolver now supports explicitly owned
byte mechanics without absorbing live audio command operands.

Both real definitions execute their complete nested timer cycles with all mechanics bytes
forbidden, preserving 503/743-frame cadence and every live color read. Focused coverage
also lowers Samus across the native Y=$0380 boundary and proves that each pre-instruction
restarts its neutral record immediately without restoring a cartridge read.

## Maridia environmental palette mechanics

Maridia's sand-pit, sand-fall, and background-waterfall programs now compile all 44
color-index, duration, wait, branch, and target words through
`MaridiaEnvironmentalPaletteFxProgramMechanicsDefinitions`. Their 112 BGR555 color words
remain live presentation data.

All three real definitions execute a complete 40/40/16-frame cycle and repeat their first
record with mechanics bytes forbidden. Exact ROM parity, every live color read, strict
owner boundaries, and continued slot activity are verified.

## Tourian glowing-block and red-orb palette mechanics

The live Tourian 2 object and unused Tourian 4 clone now compile both entries and their
shared program: 43 color-index, pre-instruction, inline CGRAM-skip, duration, wait, branch,
and target words. All 88 BGR555 colors remain live presentation data.

Both real definitions execute the complete 110-frame loop with mechanics bytes forbidden.
Verification also proves the native six-byte CGRAM gap and the slot-sensitive callback that
deletes the owner when two later palette-FX slots have been populated.

## Crateria/Brinstar beacon palette mechanics

The shared beacon-flashing program now compiles 35 color-index, duration, inline CGRAM
skip, wait, audio-opcode, branch, and target words. Its forty BGR555 colors and byte-sized
sound ID remain live presentation/audio data.

The real definition executes its complete 100-frame cycle with mechanics bytes forbidden,
queues exactly one library-two sound at the native midpoint, and repeats its first record.
Verification proves exact ROM parity, every live color read, cursor alignment across the
three-byte sound command, and the native eighteen-byte CGRAM gap.

## Norfair environmental palette mechanics

Norfair's four synchronized environmental programs now compile all 224 color-index,
heat-phase publication, duration, inline CGRAM-skip, wait, branch, and target words plus
the first program's sixteen byte-sized heat-phase operands. Their 320 BGR555 colors remain
live presentation data.

All four real definitions execute and repeat their complete 116-frame cycles with mechanics
bytes forbidden. Verification proves exact ROM parity, every live color read, all sixteen
published heat phases, and the mixed-width cursor alignment used by the separate
Samus-in-heat palette owner.

## Early Tourian escape red-flash palette mechanics

The shutter and background red-flash programs now compile all 64 color-index, duration,
wait, branch, and target words through
`TourianEscapeRedFlashPaletteFxProgramMechanicsDefinitions`. Their 140 BGR555 color words
remain live presentation data.

Both real definitions execute and repeat their complete fourteen-record cycles with
mechanics reads forbidden. Verification proves exact ROM parity, every live color read,
and the 28-frame shutter and 56-frame background loop boundaries independently.

## Shared Tourian escape red-flash palette mechanics

The general-level and arkanoid/red-orb entries now compile all 50 color-index, entry
branch, duration, inline CGRAM-skip, wait, loop, and target words through
`TourianEscapeSharedRedFlashPaletteFxProgramMechanicsDefinitions`. Their 98 BGR555 color
words remain live presentation data.

Both real definitions execute and repeat the shared fourteen-record, 28-frame cycle with
mechanics reads forbidden. Verification proves exact ROM parity, every live color read,
and both the explicit general-level branch and arkanoid fall-through entry paths.

## Old-Tourian escape-shaft red-flash palette mechanics

The old-Tourian shaft red-flash program now compiles all 60 color-index, duration,
dual inline CGRAM-skip, wait, loop, and target words through
`OldTourianEscapeRedFlashPaletteFxProgramMechanicsDefinitions`. Its 112 BGR555 color
words remain live presentation data.

The real definition executes and repeats its complete fourteen-record, 42-frame cycle
with mechanics reads forbidden. Verification proves exact ROM parity, every live color
read, and cursor alignment across both skips in every record.
