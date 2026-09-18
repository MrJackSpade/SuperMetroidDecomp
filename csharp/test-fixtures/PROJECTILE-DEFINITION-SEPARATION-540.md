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

### Compiled trail selection (#540/#547)

`ProjectileTrailDefinitions` replaces the 78 authored selector words at $90:B5BB
and $90:B609 plus all 25 adjacent-code words reachable from the right base by the
projectile type's complete low six bits. Resolution uses the physical address, so
left-table overflow still selects right-table entries and the bounded malformed
right selections remain byte-exact. Reads outside those 103 aligned words now fail
explicitly instead of treating arbitrary bank-$90 bytes as selector metadata. Tests
compare the complete reachable window against the pinned ROM, reject preceding,
unaligned and following addresses, then exercise the real spawn method for all 64
low-six-bit selections with the complete window forbidden. Allocation timer and
fixed-origin assertions accompany the selected pointers. Coordinate tables and
trail PNG artwork are covered separately below.

### Installed gameplay trail appearance catalog

`ProjectileTrailExtractor` exports the 42 appearance-bearing records from the
four native trail lists. Strict JSON exposes tile position, palette, priority and
flips only; it cannot change durations or movement commands. The production trail
draw API can select immutable appearance without changing trail timing or positions.
A newly allocated frozen trail keeps its existing attributes until its first
record runs; artwork binding does not advance it. Tests independently walk native
streams to verify complete frame coverage, compare 630 full-OAM boundary/capacity
cases and 320 live/frozen animation frames, and assert timers, cursors and sibling
movement remain identical. An edited flip reaches OAM without a draw-time bus read.
Version-four manifests install/hash the catalog, with external overrides retained
across repair/upgrade. Desktop and Android sessions bind it at startup and after
state load; normal and attract-demo runtime creation carries the same catalog.
Tests verify the actual actor pass emits edited OAM, frontend/runtime serialization
excludes content, old-state rebind preserves trail timers, all 42 host selections
match disk after restart, and corrupt/missing files fail without fallback.
Trail PNGs are not completed by this slice. Android session tests run on Windows,
not an APK deployment.

The Mother Brain intro flashback now binds both projectile compositions and trail
appearance at creation and host rebind. Its live-projectile, explosion and trail
passes use the shared catalogs. Tests call the actual cinematic OAM preparation
after native page/flashback setup, compare stock and edited emissions, require
one trail timer decrement per preparation, and serialize/restore the frontend to
verify current content is rebound without embedding it in the saved graph.

`ProjectileTrailProgramDefinitions` now compiles 42 durations, 20 inline movement
commands and five terminators. It excludes appearance words and instruction-body
bytes. All 67 mechanics words match the pinned ROM; presentation gaps, odd addresses,
wrong-bank aliases, and unrelated high-bank words now fail explicitly instead of
being interpreted as program metadata. Only genuine mutable bank-$90 low-half aliases
retain live reads, and the command-dispatch fixture exercises that path. The existing
320 live/frozen animation comparisons run the catalog-backed owner with every bus
access forbidden, while checking native output, timing, cursors and sibling movement. On record advancement,
appearance attributes come from the selected catalog; the animation scheduler
is never supplied by editable JSON. Spawn coordinate lookups are covered by the
compiled coordinate catalog below.

`ProjectileTrailAtlasExtractor` now produces a 96x8 indexed PNG containing the
twelve trail-owned tiles. `ProjectileTrailAtlas` compiles it into separate ice/wave
and missile transfers, preserving the unrelated OBJ tiles between those regions.
Verification compares the complete VRAM image after real room setup and NMI,
then edits one pixel in each tile and checks every VRAM byte for exact isolation.
Malformed PNGs, incorrect dimensions and indices above 15 are rejected.
Version-five projectile manifests now install and hash this PNG, validate stock
before selecting an override, and include the selected PNG in content identity.
The shared host trail catalog carries immutable tiles across startup/state rebind.
Gameplay room loading queues two typed transfers after standard OBJ graphics;
rebound art publishes after legacy queued transfers on accepted NMIs only. Tests
cover edited pixels after room initialization, lag retention, repeated room load,
serialized pending queues and current-content rebind, as well as installation
repair and host restart with an old save state. Android session tests run on Windows.
The intro's $8B:A505 standard OBJ transfer uses the same trail regions; the later
compressed cinematic sprites begin at VRAM word $6E00. Flashback display preparation
now publishes rebound trail PNGs before either direct rendering or detached capture.
Tests compare every VRAM byte for stock parity and two edited pixels, including an
old frontend state rebound to current artwork. Binding alone retains the prior
display; no compressed cinematic sheet is replaced.

The trail-spawn owner's previous-record lookup now uses the compiled projectile
instruction catalog too. A guarded-bus regression first failed at its direct ROM
read, then passed with the shared resolver. It checks all 1,816 catalogued words
under four timer states with a deliberately unrelated cached animation frame,
and all 32,768 upper-bank byte addresses for native odd/gap/wrap behavior. This
does not change the cart's unconditional `(instructionPointer - 2)` selection
or change bank-$9B coordinate semantics.

`ProjectileTrailCoordinateDefinitions` now compiles the complete $9B:A4B3..B3A6
region as 174 pointer words and 870 signed left-X/Y, right-X/Y records, plus the
28 exact adjacent-code bytes reachable when a restored low-six-bit projectile type
is paired with any cataloged animation frame and the wrapped `$9B:FFFF` byte read
by an empty family on frame zero. Named native table identities are
retained, including the unused SBA selections.
Both pointer selection and coordinate reads use these definitions; normal reads
allocate nothing. Partial boundaries into genuine low-half WRAM/hardware retain the
native CPU operand/MDR/open-bus model, while uncompiled upper-ROM addresses fail
explicitly instead of becoming physical coordinates. Unknown hardware still fails
through the strict bus rather than being clamped. Verification checks all 3,828
authored bytes, the 29 bounded external observations, every possible bank-$9B
word start, and 196,608 absolute-indexed operands with the compiled region
forbidden on the bus.
Another 6,600 actual SpawnTrail calls compare all four positions against independent
cartridge lookups across beam/charged/SBA/missile types, directions, animation
frames and coordinate wrap boundaries. This compiles placement, not editable art.

`ChargeFlareAnimationDefinitions` compiles the supported NTSC revision's three
delay selectors and 46 stream bytes at $90:C481..C4B4. The main flare's rewind
and the sparks' restart retain their native commands; advancement still happens
only after the timer's signed decrement becomes negative. Verification compares
all upper-bank byte/word reads, 3,072 sequential loop ticks and 3,855 seeded boundary
advances with timing ROM reads forbidden. Cached state, frame wrap and neighboring
reads retain the native result. This does not extract flare artwork or change PAL
timing; the project remains pinned to its supported NTSC ROM revision.

Charge-flare muzzle offsets are visual presentation rather than physical launch
origins. `ChargeFlarePlacementExtractor` exports standing/running X/Y offsets for
all sixteen retained direction values to `charge-flare-placement.json`, including
the native adjacent-row results of unnamed selectors. The immutable strict catalog
is accepted by the shared charge/Hyper-flare draw path without changing physics,
timing, pose corrections, Mode7 ordering or OAM admission. Tests compare 1,536
actual draw cases with placement ROM reads forbidden, demonstrate a seven-pixel
edited OBJ displacement, and compare complete serialized Samus/projectile state
before/after drawing. Missing fields, out-of-range values, duplicate properties
and injected mechanics fields fail. Public normal-charge and Hyper-flare paths
also emit the selected displacement with identical complete post-tick state.
Version-six projectile manifests now install/hash the placement JSON and include
the selected override in content identity. Desktop and Android hosts bind it at
startup and state reload; normal and attract runtimes inherit the current catalog.
The gameplay actor pass forwards it into charge-flare drawing. Tests verify stock
actor OAM, edited actor OAM, nonserialized frontend/runtime content, restored-state
output, all 32 host selections, strict stock/override failures, and preservation
through the complete installer upgrade transaction. Android session verification
runs on Windows, not on a deployed APK.

To replace placement, copy `game/projectiles/charge-flare-placement.json` into
`overrides/projectiles/charge-flare-placement.json` within the installation root,
edit the signed `x`/`y` values, and restart. Keep every `standing-00`..`standing-15`
and `running-00`..`running-15` entry. These adjust visual flares only, after the
Samus-center transform; they do not move projectile launch points or change damage.

Charge-flare composition groundwork now extracts the 28 unique visual frames used
by the 54 native charge/Hyper/spark selectors into a separate
`charge-flare-compositions.json`. It shares the strict projectile-part compiler
without changing the existing 417-frame projectile schema. The selector identities
are compiled from the pinned bank-$93 table; cadence and placement are not exposed
in the composition document. Verification compares all selectors against native
OAM at coordinate boundaries and near sprite-buffer wrap (7,938 cases), and checks
visible edits plus rejection of missing frames, invalid palettes, mechanics fields,
duplicate properties and unknown selectors.

Version-seven projectile manifests install and hash this composition file. Both
hosts bind it at startup and after state reload; ordinary and attract runtimes
inherit current content. The normal-charge and Hyper draw paths use these parts
without changing timing, placement, visibility or simulation state. Non-catalog
animation selectors retain native adjacent-table behavior rather than being
clamped or newly rejected. Grapple's separate flare caller now consumes the same
composition catalog through its native actor-order entry point.
Tests compare 676 complete producer ticks with composition ROM reads forbidden,
stock and edited runtime actor output, byte-identical simulation graphs, current
content after graph restoration, all 54 host-bound selectors after restart/load,
strict stock/override failures and preservation through real installer upgrade.
The host integration test runs Android session code on Windows, not a deployed APK.

To replace flare composition, copy `game/projectiles/charge-flare-compositions.json`
to `overrides/projectiles/charge-flare-compositions.json` in the installation root
and restart after editing. Retain all frame identities and change visual part
offsets, tile row/column, size, palette, priority or flips. The file does not expose
cadence, charge thresholds, damage or physical muzzle origins. Placement has its
own file above; editing part offsets also leaves projectile launch geometry alone.

Grapple retains its independent flare timer and firing-only post-movement origin
refresh. Its main-flare cadence now uses the shared compiled delay definitions;
the equal first entries of both facing-selector rows are compiled as a named
identity. The counter-one frame/timer seed, signed decrement, rewind, visibility
test and physical/visual hand-origin update order are unchanged. Tests exercise
2,049 phase/pose/coordinate/counter/boundary cases and compare full native OAM,
independently read native timing and complete serialized Samus/Grapple state with
composition, cadence and selector ROM reads forbidden. Actual gameplay actor
drawing and saved-state rebind also emit the edited parts. Editable rope-segment
composition/animation and sound bindings remain incomplete.

Grapple endpoint and rope characters are now installed as `grapple-tiles.png`
under version-eight projectile manifests. The 128x8 indexed PNG contains sixteen
8x8 tiles: four endpoint frames, then four horizontal, four diagonal and four
vertical rope frames. The 64-sector angle mapping remains compiled and matches
every 16-bit native angle. Queue order, byte counts and VRAM destinations are
unchanged; queued identities resolve current host pixels at NMI. Known legacy
native transfers are rebound in place, and PNG data is excluded from saved graphs.
Desktop/Android hosts and normal/attract runtime creation carry the current atlas.

This inspection also found and reproduced an endpoint animation defect. The two
words at `$9B:C342/C344` are the animation's inclusive begin/exclusive end, not
two frames. The old producer selected `$9A:8A00` on its sixth call where native
selects `$9A:8400`. The corrected compiled cycle advances `$8200/$8400/$8600/$8800`
by `$200`, retaining DEC/BPL timing and pointer-wrap arithmetic. A two-cycle
upload regression and 1,536 frame/timer boundary cases cover the correction.
Tests additionally check all 65,536 angle selections and 1,024 real producer/NMI
transfers against stock bytes with tile/pointer ROM reads forbidden. Editing the
first pixel of all sixteen tiles changes only the five selected VRAM bits per
upload, with identical rope OAM and complete Grapple state. Actor, lag-frame,
legacy/typed pending-state, installer upgrade and host restart checks cover binding.

To replace this art, copy `game/projectiles/grapple-tiles.png` to
`overrides/projectiles/grapple-tiles.png` and restart. Keep its dimensions and
four-bit palette indices. Endpoint frames occupy pixel columns 0..31; horizontal,
diagonal and vertical frames occupy 32..63, 64..95 and 96..127 respectively.
Palette selection, rope geometry, attachment behavior and damage are not PNG data.

The rope-geometry audit reproduced a separate native-parity defect before changing
production code: diagonal displacements were truncated separately for every OBJ,
putting the second segment one pixel too low in the focused up/right example.
Drawing now retains the cartridge's signed 16.16 accumulator, stops after ticking
the first off-screen slot, and preserves signed-length rejection and the native
do-while behavior when the masked segment quotient is zero. The focused
`--grapple-rope-geometry` check compares exact OAM positions and visited animation
timers in 42 cases against cartridge sine samples. This change does not alter
attachment, collision, movement, or claim completion of editable segment compositions.

A subsequent endpoint audit reproduced ordinary off-screen endpoints incorrectly
emitting OAM. Drawing now selects the native endpoint routine from Samus's pose:
ordinary poses check the uncentered relative Y high byte, while swinging poses
bypass that check and retain the camera subtraction borrow in their centering
subtraction. A 360-case test checks clipping, low coordinates, high-X and attributes
across camera/world wrapping boundaries; actual gameplay actor tests verify the
pose reaches the renderer. This is presentation-only and introduces no saved state.

Version-nine projectile installs also include `grapple-sprites.json`. Copy it from
`game/projectiles` to `overrides/projectiles` and restart to edit the endpoint and
four `segments` appearances. Each style has `tileColumn` (0..15), `tileRow` (0..31),
`palette` (0..7), `priority` (0..3), `flipX` and `flipY`. Coordinates select the SNES
OBJ tile sheet, not a pixel column of the extracted PNG. Native angle-derived
segment flip bits are ORed with the authored flips just as with stock attributes.
Each remains one eight-pixel sprite. Timing, origins, spacing, clipping, collision,
damage and attachment are deliberately not fields in this document.

Stock attributes are extracted from the endpoint immediate and four timed-record
attribute words, without extracting adjacent delays or goto instructions. The
installer validates stock before overrides and includes both identities in its
manifest/hash. Invalid or missing stock and invalid overrides fail explicitly.
The existing nonserialized Grapple artwork binding carries the selected styles
through Desktop/Android startup and state restoration, so saved games do not
freeze an old appearance. A 1,600-frame producer test checks exact native stock
OAM and isolated tile/palette edits with identical positions, full Grapple state,
animation cadence and uploads. Strict-schema and install/restart/upgrade tests
cover presentation selection. Remaining weapon/Grapple audio and other runtime
ROM dependencies still prevent closing the overall issue.

The no-artwork Grapple producer now shares the installed path's compiled angle
mapping instead of reading `$9B:C346..C3C4`. A guard reproduced that leftover read
before removal. All 65,536 angles now execute the actual no-artwork producer with
the pointer region forbidden, asserting native source, byte count and destination.
Legacy pixel transfers themselves still read cartridge pixels when drained without
installed artwork; this is not a claim of a ROM-free legacy/debug renderer.

Remaining Grapple presentation reads identified by the follow-up audit include
the standing/running flare origins at `$9B:C14A/C15E/C19A/C1AE` (launch, late draw
origin and locked connection) and the 256-entry displayed swing-frame selector at
`$9B:C1C2`. Physical hand origins and swing-body offsets are already independently
compiled. These visual reads need extraction and binding without changing physical
anchors/body placement. Non-catalog origin/connection fallback reads and shared
pose metadata also remain; they must not be silently clamped away during removal.

Version-ten installs add `grapple-flare-placement.json`, using the same signed
standing/running offset schema as charge-flare placement but extracting Grapple's
own bank-$9B tables. Its 32 pairs retain the native adjacent-row results for nibble
directions; normal firing still admits only its ten original directions. Copy the
file to `overrides/projectiles` and restart to change visual muzzle placement.
Launch, late firing draw-origin refresh and ordinary locked-connection setup use
the selected offsets. Physical origins/body placement remain independent compiled
definitions. Out-of-domain restored directions retain their existing bus fallback.

The current catalog is excluded from saved Samus state and rebound by artwork
binding, the gameplay-frame prologue and independent actor drawing. This does not
rewrite already captured flare coordinate words: a restored locked beam retains
them until a native origin publisher runs again. Immediate restyling of such cached
coordinates would require a separate state/presentation policy; no synthetic
movement or phase-dependent coordinate repair was added here.

Verification compares all 32 extracted pairs, 20 stock/edited launch and late-origin
paths, 200 extension frames and 12 locked connections with visual-origin ROM reads
forbidden. The complete Samus graph is identical after excluding exactly the four
visual coordinate words. Actual frame/draw rebinding and nonserialized identity,
strict stock/override validation, installer repair/upgrade persistence and host
restart/old-state binding are covered. Displayed swing-frame extraction, broader
audio/ROM dependencies and the cached-coordinate override boundary remain open.

Version-eleven installs add `grapple-swing-frames.json`. Its `frames` array contains
256 displayed animation-frame indices (0..31), indexed by the mirrored angle's high
byte. Copy it to `overrides/projectiles` and restart to change displayed swing
orientation. It deliberately contains no body offsets, velocity or timing fields.
The existing style/placement JSON schemas are unchanged, preserving earlier overrides.

The pendulum updater now selects art from this bound catalog while continuing to
place the collision body from the separate compiled native mapping. Stock/edited
installed and legacy paths cover 524,288 angle/facing/wrapped-anchor updates; all
physical positions and the fifteen-tick animation timer retain their native values.
Another 256 actual pumped-swing frames compare the entire Samus graph with only the
displayed frame normalized. Installed runs forbid native selector/body-table reads.
Strict parsing, stock/override integrity, installer upgrade preservation, host
restart/old-state rebinding and actual frame/draw binding checks cover installation.
This removes the installed displayed-frame selector ROM read, not all Samus sprite
art or the remaining shared/audio ROM dependencies. The overall issues stay open.
