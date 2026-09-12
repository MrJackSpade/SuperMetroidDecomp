# Remaining lookup migration inventory (#547)

Inspected at `73343942`. This is a verified list of remaining callers, **not an
exhaustive completion certificate**. Multiline reads and intermediate-address
variables must be inspected; a search for `ReadWord(...Speed...)` misses them.

## Confirmed remaining runtime mechanics reads

| Owner | Native definition | Remaining consumer |
| --- | --- | --- |
| Ridley | Carry/release anchors, pogo targets/path pointers, acceleration and health divisor selectors | RoomEnemySystem.Ridley and related partials |
| Phantoon | Rain-placement records, random direction, mouth schedule pointers | RoomEnemySystem.Phantoon and related partials |
| Bomb Torizo | Wake positions, radii, property masks and instruction selectors | RoomEnemySystem.BombTorizo initialization |
| Ceres debris | X offsets and instruction selectors | Ceres falling-debris spawning |
| Common movement | Slope height bytes | RoomEnemySystem.CommonMovement |

These are confirmed indirect `EnemyRomTablePointers` consumer groups, not an
exhaustive list. Classify instruction selectors separately from artwork payloads.
The shared Ceres/Norfair inertia bytes ($A6:D712/$A6:D61F) are now compiled.
These remaining groups are live references, not merely obsolete address declarations. Each still
needs reference-value parity, selector/bounds/sign/wrap evidence and removal of
the actual runtime read. Inspect authored bounds and adjacent-data behavior
before substituting a catalog; do not assume every table is a smooth formula.

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
