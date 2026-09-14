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

That instruction slice did not compile initial definition-selection pointer
tables. The selection slice below supersedes that status. Sprite-reference and
artwork extraction and arbitrary out-of-table bus reads remain unfinished.

### Implemented initial-selection slice

`SamusProjectileSelectionDefinitions` compiles the 357 definition/program-pointer
words at $93:83C1..86DA, excluding the forty damage headers already owned by the
damage catalog. It preserves null/unused entries and routes selectors that reach
adjacent damage headers through that catalog, then retains exact-address fallback.
These references select instruction programs, not editable spritemaps.

All seven initializer paths now use it, as do explosion-list selectors in beam,
missile and bomb impact handling. The initialization verifier blocks every byte
of the entire selection/damage region and exercises actual beam, Hyper, missile,
reflection, link, combo and bomb initialization. It checks all 397 region words
and every high-bank word address, preserving odd/adjacent/bank-wrapped reads.
List/radius/damage/timer and special-case assertions remain active.

The older synthetic beam fixture replaced definition selectors with invented
damage headers and a two-tick explosion. That no longer reaches the intended
artwork once selectors are compiled. It now seeds only sprite references on the
actual native-selected programs, keeps the constructed terrain and visible OBJ
assertions, compares native damage/radii, and derives exact finite explosion
lifetimes from ROM. Obsolete fake header/program writes were removed; the linked
Super Missile second-collision test retains its three-frame native sequence.
The focused Ceres Ridley 100-hit fixture also waited only two frames for a fake
one-tick explosion. It now derives the native lifetime plus the initial unconsumed
frame before reusing slot zero; no enemy hit count or production combat is changed.

Remaining scope is not just a missing table: editable sprite references and
artwork must be bound into runtime rendering/capture/restore with actual visible
override tests. Non-catalog/glitch reads, flare/trail presentation, combo costs
and origin-angle definitions also still need explicit audit and integration.

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

### Projectile-specific authored-part rendering prerequisite

The menu composition emitter cannot be reused unchanged for projectiles: its
vertical-wrap parking and capacity stop differ from $81:8A4B/$81:8A2B. The existing
packed projectile reader now delegates each decoded part to the public
`OamBuffer.AddProjectileSpritePart`, which retains source attributes, byte Y wrap,
X/size high-table packing and nine-bit OAM write-position wrap. This provides a
ROM-independent emission path for future authored projectile compositions without
changing the current draw caller's whole-projectile admission checks.

`VerifyProjectileVisualParts` tests 393,216 emissions: every encoded X/size word
and attribute word, all Y-offset bytes, six boundary origins and repeated OAM
wraparound. Independent arithmetic assertions cover each hardware field, and
complete low/high OAM tables agree with the packed-ROM reader after each part.
This is not a loaded asset catalog or rendered-PNG override claim; extraction,
validation, runtime binding and restore/capture integration are still required.
The pinned ROM's 805 timed records reference 417 distinct spritemaps, with at most
24 parts and maximum OBJ tile index 194. This inventory excludes separately
selected flare/trail artwork and is not a complete weapon asset inventory.

### Extracted timed-projectile compositions

`ProjectileSpriteExtractor.Extract` produces version-one
`projectile-compositions.json` for the 417 stable identities in
`ProjectileSpriteDefinitions`. `ProjectileSpriteCatalog.Load` compiles immutable
parts and draws through the projectile-specific OAM emitter with no address space.
Only offsets, tile column/row, size, palette, priority and flips are editable;
damage, collision and instruction fields are not part of this document. The
loader requires every known identity and rejects bad fields, unknown properties
and duplicate properties rather than silently selecting the last value.

`VerifyProjectileCompositions` compares all 417 maps at seven boundary origins
and empty/near-full OAM (5,838 full low/high-table comparisons). It also proves
an offset edit reaches OAM, compiled content is immutable, and invalid content
fails. This is composition extraction/loading, not PNG sheet extraction or a
complete runtime binding: installation/provenance, gameplay draw selection,
capture/restore rebind, actual rendered-frame overrides and flare/trail assets
remain open requirements. No ROM-derived JSON or screenshots are published.

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

## Production drawing composition seam

`DrawLiveProjectiles`, `DrawExplosions`, and the bomb owner's `Draw` now accept an
optional immutable `ProjectileSpriteCatalog`. Their existing admission, flicker,
slot ordering and power-bomb detonation suppression run before composition emission.
When supplied, the catalog is authoritative: a missing sprite throws instead of
falling back to the ROM. The legacy no-catalog path remains during migration.

The owner regression compares 120,096 stock OAM results across all 417 identities,
eight projectile families, four NMI phases and nine viewport-boundary positions.
The extracted path forbids every bus read/write and preserves damage and radii.
Zero-timer power bombs are separately required to emit nothing.

This is not host installation or automatic asset binding. Runtime/host content
selection, provenance, restore-time rebinding, PNG artwork, trails and flares remain
unfinished. The catalog is passed per draw and adds no serialized owner state.
