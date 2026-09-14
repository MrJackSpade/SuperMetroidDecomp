# Remaining projectile definition ownership (#540 / #547)

Audited against production at 1035f8c3 and pinned `upstream-disassembly/src/bank_93.asm`.
This is an implementation inventory, not a completed migration or gameplay fix.

## Implemented damage-header slice

`SamusProjectileDamageDefinitions` now compiles all forty damage headers, including
the unused negative marker and zero-damage record. All seven consumers below
use it. Exact-address selection preserves unaligned/adjacent reads on the bus.
At that slice, pointer-table reads, instruction streams and physical radii remained
ROM-backed; the later radius slice below supersedes the radius status only.
This is not a ROM-free projectile initializer or editable-art integration claim.

`VerifyProjectileDamage` compares every header with reads forbidden, then every
high-bank address (including odd addresses and bank-end wrapping) against ROM.
Actual firing/reflection covers twelve beam combinations, charged and uncharged;
both missiles, Hyper, the invisible Super Missile link, all four SBAs and three
bomb types also run through the guard. Assertions cover damage, directional list,
initial radii/timer, link allocation, Hyper override and negative-marker rejection.
Existing full-suite tests retain trajectory and combat coverage. Presentation
override tests and the rest of the integration gates below remain outstanding.

## Shared initializer consumers

### Implemented collision-radius slice

`SamusProjectileRadiusDefinitions` compiles the 805 authored byte pairs in
bank-$93 timed instruction records, including unused lists. Every radius reader
in ordinary/Hyper/missile firing, reflection, combo initialization and both
projectile instruction handlers now uses this catalog. Sparse exact byte keys
leave timers, opcodes, spritemap references and trail values on their existing
paths; unknown addresses retain the bus fallback.

`VerifyProjectileRadii` independently inventories the field addresses and compares
all 1,610 physical bytes with ROM reads forbidden, plus every high-bank byte for
adjacent-read preservation. It runs 1,610 actual projectile and 1,610 bomb timed
frames, half with a substituted spritemap reference, asserting unchanged physical
radii, durations and next pointers (and projectile trail values). The substituted
reference is intentionally a field-level stimulus, not a rendered asset: this
does not claim sprite rendering, editable resource loading or complete ROM-free
instruction execution. Full production initializers remain covered by the damage
slice's list/radius assertions and the existing verification suite.

At the radius slice, pointer tables, duration/control/trail words and artwork
extraction remained to be separated. The instruction slice below supersedes
duration/control/trail status; the wider integration checklist is not complete.

### Implemented instruction-mechanics slice

`SamusProjectileInstructionDefinitions` compiles 1,816 words: duration/trail words
from all 805 timed records and 206 words from 105 Delete/Goto commands and their
operands. Both instruction handlers use the catalog for these mechanics while
continuing to read sprite references separately. Radius bytes use their existing
compiled catalog. Sparse exact word lookup preserves non-catalog/unaligned reads.

`VerifyProjectileInstructions` compares every compiled word and every high-bank
word address against ROM, then runs both actual handlers from all 910 frame/control
entry points for up to 64 ticks each. An independent interpreter reads expected
values directly from ROM. All 56,800 frames per owner agree on deletion, timer,
next pointer, sprite, radii and projectile trail state while compiled mechanics
reads are forbidden. The prior art-reference substitution tests also still pass.

This does not compile initial definition-selection pointer tables, extract sprite
references or artwork, or remove arbitrary out-of-table bus reads. Those remain
unfinished rather than being hidden behind this instruction subset.

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
