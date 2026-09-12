# Remaining lookup migration inventory (#547)

Inspected at `73343942`. This is a verified list of remaining callers, **not an
exhaustive completion certificate**. Multiline reads and intermediate-address
variables must be inspected; a search for `ReadWord(...Speed...)` misses them.

## Confirmed remaining runtime mechanics reads

| Owner | Native definition | Remaining consumer |
| --- | --- | --- |
| Growing shutter | $A2:EA56 speed records | Four-byte speed-indexed initialization |
| Intro egg particles | $8B:A9EA/AA02 | Split horizontal/vertical velocity integration |
| Intro slime drops | $8B:AB35/AB49/AC41 | X and parity-dependent Y records |
| Gameplay renderer | $88:A266/A286 | Horizontal/vertical shape samples read from bus |

These are live references, not merely obsolete address declarations. Each still
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

The crawler slope multiplier and Yard kick records were subsequently compiled
and verified; see the main evidence log. Next implementation grouping: remaining family
launch records. Keep verified commits scoped and do not close #547 until its
complete caller and integration audit is satisfied.
