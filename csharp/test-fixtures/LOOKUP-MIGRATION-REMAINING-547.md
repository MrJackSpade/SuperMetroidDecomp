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
Wrecked Ship Spark's three authored initial instruction/function pairs are
compiled too, together with selector three's two adjacent-code observations.
All four production initializers retain the two-bit population mask while
`$A8:E682-$E68F` is forbidden. Mixed animation programs remain separate dependencies.
The elevator actor's Down/Up departure masks are compiled in its shared domain
catalog. Both real departure paths retain the doubled population byte offset,
pose/input/projectile/audio handoff, and Samus pinning while `$A3:94E2-$94E5`
is forbidden.
Owtch's eight patrol half-widths and six underground durations are compiled as
physical definitions. All 144 representative/wrapped production initializers
retain their shared linear speeds, state-specific burial setup and exact bounds
while `$A2:A3DD-$A3F8` is forbidden.
Nuclear Waffle's two sweep directions now combine twelve endpoint, link-spacing,
and joint-turn words into typed physical records. Both complete production
initializers retain their seven allocated articulated links while
`$A6:95F6-$A6:960D` is forbidden; invalid directions cannot consume main-AI code.
Hibashi's 22 eruption Y offsets and collision half-heights are compiled as paired
physical frames. Every production activity command retains exact placement,
radius and frame-zero width while `$A6:8DBB-$A6:8E12` is forbidden.
Magdollite's nine rise thresholds, body-list selectors and overlay offsets are
compiled as typed phase records. Real initialization, rising, falling and overlay
tracking retain their exact phase geometry while `$A8:AF55-$A8:AF8A` is forbidden.
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

Choot's five real fall-stream selectors and their five indirectly selected loop
distances are now compiled in `ChootPatternDefinitions`; all initializer consumers
run with the pointer tables and distance words forbidden. The variable-length X/Y
fall streams themselves remain live mechanical program data and still require an
explicit migration decision.

The five crawler-family initial orientation tables and all four shared six-species
surface tables are now compiled in `CrawlerAnimationDefinitions`. Shared crawlers,
Viola, Sciser, Zero, and HZoomer consume typed family/surface selectors with all 44
source words forbidden. Their selected instruction programs and spritemaps remain
separate program/presentation dependencies.

Maridia's large snail/Oum now compiles all eight mutually-exclusive facing/action
instruction selectors at `$A2:CB77-$A2:CB86`. Every production install consumes the
named definition catalog with the pointer table forbidden. The selected mixed
instruction programs and extended spritemaps remain separate dependencies.

Botwoon's four fixed hole rectangles and 32 fixed path descriptors are now compiled
in `BotwoonNavigationDefinitions`. The real target movement, hole collision, and path
handoff consumers no longer read `$B3:949B-$94BA` or `$B3:E150-$E24F`; native byte
offsets remain visible in saved/debugger state. Botwoon's variable-length signed path
streams are still live mechanical program data and require a separate migration
decision rather than being conflated with their fixed descriptors.

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
forbidden. The selected mixed animation programs and extended spritemaps remain
separate program/presentation dependencies.

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
Out-of-domain restored parameters deliberately retain the cartridge's unchecked
adjacent-data reads; the first such selector is independently compared with ROM.

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

Bomb Torizo's sixteen statue-hand fragment selections are compiled as complete
physical records, preserving the native eight-row wrap for Y position, velocity, and
acceleration. Every real room-graphics projectile allocation and initializer runs with
the five source tables forbidden; selected animation programs remain separate.

Bomb/Golden Torizo's eight randomized Chozo-orb, egg, and eye-beam launch tuples are
compiled as typed physical records. The shared real initializer retains independent
signed-byte velocity jitter, while the eye beam still replaces those base velocities
with its aimed launch. All family/facing spawn paths run with the eight tuples forbidden.

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
