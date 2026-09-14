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

The frontend now binds the catalog to existing and newly created gameplay/demo
runtimes, and runtime actor drawing forwards it to all three timed-projectile passes.
Both frontend and runtime fields are nonserialized. The debugger regression proves
byte-identical graphs with/without content and draws the newly bound override after
restoration. Runtime OAM assertions cover stock parity and observable edits for live
shots, explosions and bombs.

The installer now stages `game/projectiles/projectile-compositions.json` plus a
versioned `projectile-manifest.json`, validates revision/content hashes and repairs
missing/outdated stock through its existing transaction. `GameInstallation.LoadProjectiles`
validates stock before selecting `overrides/projectiles/projectile-compositions.json`.
It reports separate original/selected hashes. Corrupt overrides fail instead of
falling back; reinstall and stock repair preserve the external override directory.
Focused tests cover missing/corrupt stock, bad overrides and manifest identities;
the full installer test covers replacement/cancellation/restart with an edited
projectile override alongside existing player-data preservation checks.

Installed Desktop and Android sessions now load and bind compositions at startup
and rebind the host's current catalog after debugger-state load. Restart reloads
disk selection; explicit diagnostic cartridge paths retain the legacy ROM path.
Startup logs original and selected content hashes. The installed Android session
test (run on Windows, not the device) verifies all 417 selected compositions at
startup and after restart/old-state rebind, plus invalid-override rejection.
Trails, flares and the wider weapon presentation scope remain open.

### Installed beam PNG artwork

`BeamTileExtractor` emits twelve 64x8 indexed sheets, one per legal beam selection
at $90:C3B1. Shared stock graphics may be edited independently per combination.
`BeamTileAtlas` compiles indices into the exact 256-byte 4-bpp upload at VRAM word
$6300. Palette colors in the PNG are diagnostic; the indices address the game's
separately selected beam palette.

Tests compare all twelve decoded PNGs with the complete VRAM result of production
`LoadBeamTilesAndPalette`. Changing the first pixel's low bit changes exactly bit
7 of the first destination byte, leaving every other VRAM byte unchanged. Invalid
dimensions, malformed PNGs and indices above fifteen are rejected.

`BeamTileCatalog` now resolves typed queued uploads for the twelve legal selections.
`QueueBeamTilesAndLoadPalette` can publish these IDs with the original destination,
byte count, queue order and palette behavior. Invalid combination indexes retain
the legacy adjacent-ROM path. Tests forbid graphics/selection reads during enqueue
and all bus access during drain, and restore a pending queue against edited content.
The runtime now routes typed HUD/beam assets through its NMI provider and supplies
beam content at room initialization, pause teardown and beam pickup. Frontend
binding carries content into normal/demo runtimes. Rebound beam VRAM is refreshed
only at accepted NMI after legacy queued writes, not in the retained display or a
lag NMI. Tests cover those boundaries and a restored typed equipment upload.
Version-two projectile manifests now hash all twelve sheets. Installed Desktop and
Android sessions bind the selected beam catalog at startup and after state load.
Stock validation precedes override selection; content identities include both
composition and every PNG in fixed order. Tests cover corruption, missing sheets,
edited-pixel upload, restart/state rebinding (Android session on Windows), and full
installer upgrade from composition-only manifests while preserving external edits.
No Android device deployment or full replacement of other weapon artwork is claimed.

### Installed ordinary beam palette catalog

`BeamPaletteExtractor` reads the twelve $90:C3C9 selections and exposes sixteen
RGB5 colors per selection in validated JSON. `BeamPaletteCatalog` compiles those
colors immutably and the production queued beam upload accepts it independently
of PNG artwork. Tests compare every CGRAM entry with the original $90:ACCD path,
including untouched neighbors, then edit one channel bit and require exact
isolation. The combined tile/palette catalog queue forbids every bus read.
Missing selections/colors, duplicate metadata, unknown fields and invalid RGB
values fail loudly. Version-three manifests include the palette JSON and its hash;
the installed beam catalog carries selected colors through both hosts. Upgrade
and restart tests preserve edits and verify all twelve host-bound color sets.
Normal colors refresh at accepted NMI after state rebind, while active Crystal
Flash/Hyper colors are retained. The Crystal Flash completion path restores the
selected catalog without ROM reads and still clears its palette-handler state.
Charge/Hyper palette animation and invalid beam-index adjacent reads are unchanged;
this catalog is not a claim that all weapon palette ROM dependencies are removed.
