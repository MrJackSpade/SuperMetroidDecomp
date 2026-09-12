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

| Mechanics group | Confirmed live consumer and semantics | Migration requirements |
| --- | --- | --- |
| Kraid growth and combat | `RoomEnemySystem.KraidGrowth/Combat/Death`: initial timer, combat timer, death timer, ceiling-rock positions | Preserve byte selectors, countdown boundaries and current-RNG semantics. Keep palette reads in presentation scope. |
| Kraid hitboxes/projectiles | `KraidCollisions`, `KraidNails`: indirect mouth hitboxes and BF1D nail offsets | Inspect overlapping record strides, signed coordinates and actual collision/placement paths. BC65 spat-rock velocities, B163/B165 body contour and BE3E/BE46 indirect nail launch choices are now compiled. |
| Kraid death schedule | `KraidDeath`: C5E7 explosion Y/function records | Compile the schedule and typed callbacks; inspect dispatch, entry progression and timing. Do not substitute a cosmetic sprite-only migration. |

### Mixed instruction selectors and presentation reads

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
