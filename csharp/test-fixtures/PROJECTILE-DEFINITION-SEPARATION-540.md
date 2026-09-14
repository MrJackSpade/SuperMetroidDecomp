# Remaining projectile definition ownership (#540 / #547)

Audited against production at 1035f8c3 and pinned `upstream-disassembly/src/bank_93.asm`.
This is an implementation inventory, not a completed migration or gameplay fix.

## Shared initializer consumers

| Production path | Definition selection | Coupled fields |
| --- | --- | --- |
| Firing: ordinary beam | Charged/uncharged table, low beam index | Damage, direction list, initial radii |
| Firing: Hyper Beam | Charged entry eight | Same fields; then damage overwritten with 1000 |
| Firing: missiles | Non-beam table, selected HUD item | Damage, direction list, initial radii |
| TrailsAndCollisions: reflection | Charged/uncharged or missile family | Damage, direction list, initial radii, timer |
| Motion: Super Missile link | Dedicated link pointer table, entry two | Damage, single list, timer |
| Combos: InitializeComboData | Charged, SBA or echo table | Damage; directional versus single list; ordinary initial radii |
| BombProjectileSystem: InitializeBombFromRom | Non-beam table, masked high type nibble | Damage, single list, timer |

The last consumer is shared by placed bombs, Power Bombs and Bomb Spread. Removing
only its damage read would leave ordinary firing and reflected projectiles with
different ownership rules. Compile the shared definition identity/damage domain,
then integrate every consumer above before claiming that damage is ROM-independent.
Direction-specific list selection is still presentation/program data, not damage.

## Important cartridge boundaries

- The data headers start at $93:8431. Twenty-four beam headers contain damage plus
  ten direction pointers. Charged ordering is not identical to uncharged ordering;
  compile by native identity, not by an assumed physical record index.
- The non-beam headers start at $93:8641. Missile and Super Missile damage are
  100 and 300. The separate Super Missile link at $93:866D also has damage 300:
  its empty spritemap does **not** imply zero damage. Production finds free slots
  by `Damage == 0`, so changing that value also changes allocation.
- Power Bomb ($93:8671) and bomb ($93:8675) damage are 200 and 30. Explosion
  headers include ignored damage and zero values; do not normalize them based
  on their visible appearance. SBA damage is 300.
- Echo/trail selection has null entries, an unused $F000 damage marker at
  $93:8695, a 300-damage Spazer trail at $93:86AB, a $1000 Shinespark echo at
  $93:86C1, and a zero-damage unused record at $93:86D7. The last record lacks
  a `Damage` annotation; searching only annotated lines misses it.
- Projectile instruction records are mixed: timer, spritemap, X/Y physical
  radius, trail frame; function opcodes occupy the same stream. Extracting the
  whole record as editable artwork would expose collision/timing mechanics.
- Hyper Beam deliberately replaces its initially loaded damage with 1000.
  Reflection reloads selected projectile definitions; verify that path separately
  rather than assuming firing-only tests prove damage ownership.

## Verification required for the migration

1. Compare compiled header identities and damage against the pinned ROM, including
   null/unused/negative-marker entries and adjacent indexing behavior used by glitches.
2. Exercise all seven production consumers with compiled damage reads forbidden.
   Assert damage, slot allocation, timers, direction-list choice and physical radii.
3. Change presentation resources and prove damage/radii remain unchanged, while
   asserting that the intended rendered artwork actually changes.
4. Cover reflection, Hyper override, combo echo selection, ordinary/PB/spread
   allocation and Super Missile link lifetime. A zero-damage/free-slot assertion
   is essential; no-crash or isolated catalog equality is insufficient.
5. Retain native trajectory/combat tests and strict unsupported-state handling.
   Address-based fallbacks, if retained during staged migration, must remain
   explicitly listed as unfinished ROM dependencies.

No raw ROM, player state, movie or screenshots are included in this audit.
