# Reviewed closed-provider contracts

These rules account for particular #1156 coverage gaps whose providers already validate
their complete resource domain. They are **not findings baselines** and do not certify
arbitrary caller input, pixels, timing, host installation or gameplay parity. An invalid
dynamic index may still fail the provider's domain guard; that is not an omitted valid
resource. Static invalid constants remain audit failures.

Each rule requires the exact qualified provider type, an explicitly reviewed operation,
and SHA-256 fingerprints of its complete loader/selector source and definition set.
Fingerprints normalize CRLF to LF only. Changes, including comments, revoke the proof
until source review is repeated. Never refresh a fingerprint just to silence a finding.
New types and operations receive no automatic exemption. Every classified consumer is
retained in JSON `consumers` and `classifications` with its reason and guarded source
fingerprints. JSON schema 2 introduces that explicit evidence list.

## Gameplay HUD v2 — 24 original consumer boundaries

`GameplayHudPresentation.Load` is the only path to private construction. Construction
compiles the fixed top row/template, exactly ten health digits and ten ammo digits,
fourteen tank anchors, six full/empty AUTO cells, and exactly all five named icons.
No reviewed operation loads a further resource or accepts an arbitrary artwork name.

- `ApplyTemplate`, `ApplyAutoReserve`, `ClearAutoReserve`: fixed validated fields.
- `ApplyEnergy`: clamps tank iteration to the installed fourteen anchors; health digits
  use `% 100` and the helper's `% 10` lookup into complete glyph arrays.
- `ApplyAmmo`: only item indices 0..2, with digit extraction modulo ten.
- `TryApplyIcon`: `IconName` limits indices to 0..4; every corresponding icon is required.
- `ToggleItemHighlight`: only valid selected-item indices reach the installed icons;
  other indices return without a resource lookup.
- `MinimapCellIndex`: guards the 5x3 coordinate domain and computes a layout index.
  It does not request another resource identity.

The existing `IconNames` field is a mutable array behind a readonly reference. The
source pass additionally rejects any reference to that field outside the two reviewed
provider/definition files. It cannot silently accept a new caller that mutates its keys.
This scope is the Core source inventory, not arbitrary code in third-party assemblies.

## File select v1 — 23 original consumer boundaries

`FileSelectPresentation.Load` requires exact named page, patch, sprite, border-anchor
and dynamic-anchor sets, then compiles every member. It also requires ten digits,
three letters, three main/data slot layouts, 6/4/2 cursor anchors and three helmet
anchors. The sole constructor is private; no partial dictionary reaches consumers.

- `LoadBackground`: fixed `Background` page required by the loader.
- `Slot`, `WriteDigit`, `WriteSlotLetter`: complete arrays with bounds checks.
- `CursorPosition`: selects one of the three complete cursor arrays; `Point` guards
  the actual selected array's index. The audit rejects constants outside the union
  0..5; runtime guards still enforce the chosen 6/4/2 subset.
- `DrawCursor`, `DrawHelmet`: generated names are bounded to four/eight frames;
  all such sprite names and all three helmet anchors are required.
- `DynamicAnchor`, `ApplyPatch`: only compiler-resolved string arguments qualify,
  and each key is independently compared against the loader's named definition set.
  Unknown constants are concrete missing identities; unknown strings stay unresolved.

**Not covered:** `CopyPage` and `DrawBorder` currently take dynamically selected names.
Their two original findings remain visible until that name flow is bounded separately.
The rule does not assume that every string accepted by a method signature is authored.

## Mother Brain room colors v3 — five original consumer boundaries

`MotherBrainRoomColorPresentation.Load` requires fourteen complete flash rows, seven
complete recovery-light rows, and all fixed final/phase-two/room-entry arrays. Legacy
documents can omit newer fields only when inheriting them from validated current stock.
Private construction prevents partially validated providers.

- `ApplyFlash`: aligned timed-entry offsets select only the fourteen installed rows.
  Invalid alignment/range is rejected. The separate importer/loader adapter independently
  checks the compiled program's fourteen palette operand identities.
- `ApplyRecoveryLights`: guards the 0..6 frame domain and selects complete rows.
- `ApplyFinal`, `ApplyPhaseTwoInitial`, `ApplyRoomEntry`: operate fixed loaded arrays;
  no dynamic external identity is requested.

## Scoped confirmation

`--self-check` uses the actual reviewed source declarations and constructed consumer
calls, not a room or gameplay frame. Before applying the adapter, forty-nine fixture
calls are unresolved. With it, twenty-six valid reviewed calls are classified, missing
anchor/conditional-label keys remain concrete, and twenty-one invalid/unreviewed/dynamic
selections remain unresolved. Mutating digit-selection code or adding an external
`IconNames` mutation revokes the proof at the real consumer adapter. These checks
confirm only the identified adapter contracts; they do not discover new player bugs.

## Options v1 — twelve original consumer boundaries

`GameOptionsPresentation.Load` requires exactly all six pages, seven controller labels,
two special toggles, seven label anchors, four language regions, three heading/cursor
sets and seven heading/cursor compositions. Each cursor set has its specified 5/9/3
anchors. Private construction cannot bypass these validations.

- `LoadBackground`, `ApplyLanguage`: fixed loaded page/highlight fields.
- `ApplyControllerLabel`: action and button selectors are bounded to 0..6, and all
  seven controller-label names and anchors are required.
- `DrawCursor`: the four generated cursor names are exhaustive and guarded.
- `CreatePage`, `ApplySpecialToggle`, `CursorPosition`, `DrawHeading`: always require
  a separate finite named-key proof. The current five constant pages and two constant
  toggle calls qualify; the three helper-selected page/menu calls remain unresolved.

## Game over v1 — seven original consumer boundaries

`GameOverPresentation.Load` requires the complete tilemap, all eight sprite names
(three baby frames, egg, four cursor frames), and all four baby palettes, each with
sixteen colors. The only constructor is private.

- `LoadTilemapTo`, `DrawEgg`: fixed loaded resources.
- `DrawBaby`, `ApplyBabyPalette`: finite enum switches cover every valid frame/palette
  and explicitly reject values outside the supported enums.
- `DrawCursor`: all four cursor frames are installed and range-checked.

## Pause reserve UI v1 — six original consumer boundaries

`PauseReserveUiPresentation.Load` requires all four labels, ten complete digits,
ten unique arrow cells, 32 full animation frames and both fixed arrow colors.
Only its private constructor can publish a provider.

- `ApplyDigit`: positions 0..2 and digit values 0..9 are range-checked.
- `ApplyArrowTilePalettes`: uses the two loaded palette choices and validated offsets.
- `ApplyArrowColors`: masks every frame value into the complete 32-frame array,
  or selects the fixed solid colors; destination CGRAM indices are not resource IDs.
- `ApplyLabel`: the three current calls select Mode, ReserveTank or Auto/Manual.
  Each name must be independently checked; a conditional requires both branches.

## Finite named-key analysis

`NamedPresentationAudit` handles compiler constants, conversions and conditional
unions only. If the compiler knows the condition, only the selected branch is required;
otherwise both branches must resolve to constants. Missing keys in either branch remain
concrete omissions. Unknown parameters, mutable variables and function results remain
unresolved. This deliberately does not infer names from similar strings or assume an
arbitrary method argument matches its provider's catalog.

## Beam/boss palette providers — 29 original consumer boundaries

All rules below require private construction through a loader that compiles every
specified row and color. A runtime bounds error remains possible for an invalid dynamic
argument; these rules prove complete supported resource domains, not caller correctness.
`ClosedPresentationIndexDefinitions` owns the named domains and alignment constraints.

| Provider | Original sites | Required domain and reviewed operations |
| --- | ---: | --- |
| Beam palette | 5 | All twelve sixteen-color selections; `LoadTo` guards selection 0..11. |
| Ceres/shared Ridley | 8 | Complete start/retreat arrays; sixteen eye/body/alarm rows, three health rows, four baby rows. Seven `Apply` methods select fixed or range-guarded loaded arrays. Legacy fields inherit only validated stock. |
| Crocomire | 4 | Complete fight-body, wall, projectile, skeleton-arm and spike arrays. The four `Apply` methods transfer fixed loaded arrays. Raw resolver methods are not included. |
| Spore Spawn | 3 | Complete spore row, four health rows, eight death-sprite rows and seven level/background rows, sixteen colors each. `ResolveSpore/Health/Death` guard color/frame/layer domains. Known level/background constants use the narrower seven-row frame constraint. |
| Draygon | 3 | Complete intro/background/sprite/white-flash arrays and eight four-color health bands. Health indices must be even byte offsets 0..14. `ApplyHurt` does not select a health band on a white-flash frame, so a known ignored index is not falsely reported. |
| Phantoon | 3 | Eight sixteen-color health bands, sixteen fade-out targets and 112 power-on targets. Three reviewed resolvers guard band/color indices into these required arrays. |
| Tourian statues | 3 | Complete base/statue/grey arrays and four four-color eye rows. `ApplyEntrance/Eye/Grey` use fixed arrays or even doubled eye indices 0..6. Unguarded raw `Resolve` methods are excluded. |

The palette fixture checks one covered operation and one invalid/unreviewed operation
for each identified provider domain. It also checks Draygon's unused health selector on
a white frame. Invalid beam/health/color indices, odd Draygon/statue selectors, a
too-high level-death frame and an unreviewed Crocomire resolver all stay failing gaps.
No palette animation, boss frame, AI or combat callback is executed.

## Complete color sequences - 22 original consumer boundaries

| Provider | Original sites | Required domain and reviewed operations |
| --- | ---: | --- |
| Mother Brain rainbow | 8 | Private construction requires ten full rainbow frames, eight drain/revival/fake-death frames, normal colors, and 38 beam colors. The seven reviewed operations use these arrays with guarded indices. Cursor values 0..152 in steps of four include the final compiled terminator; 152 is not an omitted color. Legacy fake-death rows require validated stock. |
| Mother Brain death | 4 | All sixteen fourteen-color body/leg rows, eight fifteen-color corpse rows and fourteen door colors are required before private construction. Four reviewed resolvers guard the complete row/color domains. |
| Chozo/tube | 3 | Private construction requires all three fixed 32-color images. The three `Apply` methods transfer loaded arrays. Raw resolver methods are deliberately excluded. |
| Gameplay base | 3 | Private construction requires 256 initial CGRAM colors and sixteen common sprite colors. The projectile palette is the complete initial-image slice 208..223. The three `Load` methods select fixed fields/slices; destination indices are placement, not resource identities. CGRAM/layout definitions are guarded too. |
| Samus death | 4 | Its public constructor validates and deep-clones all three ten-row suited families, ten suitless rows, 22 whiteout shades and nine explosion selectors. All rows contain sixteen colors. Four direct array selectors have complete valid domains; CLR bounds checks reject invalid arguments, but the proof does not certify dynamic caller indices. |

The constructed-source confirmation checks valid endpoints and rejected out-of-range
rainbow, death, suit and explosion indices. It also distinguishes the valid aligned beam
terminator from an invalid intermediate cursor, and leaves the unreviewed Chozo resolver
unresolved. A dynamic CGRAM destination does not become an invented resource dependency.
No palette sequence, death animation or battle is executed.

## Gameplay messages - 15 original consumer boundaries

The three private-constructor loaders require their exact separate message sets:
fifteen one-row titles, seven large item panels, and five station/save notices.
Templates, borders and text glyphs are complete and validated before publication.
Notice/panel rules also guard the shared title glyph compiler and its definitions.

- `Contains` is a membership query, not a demand for artwork. Asking about an ID
  outside a provider's owned set correctly returns false.
- `Build` resolves only the corresponding provider's required message set; constant
  cross-family requests remain failures. Sparse ID sets are taken from the same
  source-guarded definitions used by the loader, not a broad numeric interval.
- Notice `ApplySelection` owns only SaveConfirmation and GunshipSaveConfirmation.
  Both YES and NO rows are required for each. Station-completion notices do not
  acquire fictional selection rows merely because they share the notice provider.

An eight-call constructed-source fixture first reproduces all missing adapters.
Five calls then qualify, while three cross-family/non-save selection calls remain
unresolved for their exact ownership violation. No message box or gameplay runs.
As with the other closed-domain proofs, unknown dynamic caller correctness is not
certified by a provider's complete supported resource set.

## PLM multi-run artwork - five original consumer boundaries

The five reviewed `GetWord` providers are shot blocks, stations, Bomb Torizo's
hand, Mother Brain's glass, and the n00b tube. Each public constructor accepts
only its compiled frame IDs, rejects duplicates, requires the complete frame
count and exact payload shapes, and clones the supplied artwork. Partial
providers cannot be published through those constructors.

Shot/station providers retain per-run arrays. Hand/glass/tube providers flatten
their payloads but check against the compiled run lengths before translating a
run/word tuple into a flat offset. Full provider and draw-definition fingerprints
guard each proof, including the shared run record definition.

`PlmVisualDomainDefinitions` supplies those exact shapes to the constant selector
check. A known pointer narrows the shape before run/word checking; a one-word
shot-block frame cannot borrow the width of another frame to accept word one.
Unknown pointer/run components use only the union of compatible declared shapes;
this is a resource-domain proof, not certification of arbitrary caller values.
New providers do not inherit a rule based on their class name or similar source.

The thirteen-call source fixture first reproduces missing adapters. Six calls
then qualify and seven bad pointer/run/word requests remain failures. Separate
constructed entries confirm all five production constructors accept full data
and reject both an omitted frame and duplicate identities. These checks execute
only immutable definition/catalog construction, not a PLM instruction or room.

## PLM actor layouts - five additional original consumer boundaries

The downward gate, elevator platform, Draygon cannon, Chozo statue and linked-block
restoration providers have the same complete constructor contract, independently
reviewed and source-guarded. Their exact required domains are fourteen gate/trigger
draws, three elevator frames, twelve left/right cannon frames, three Chozo layouts
and six linked-restoration layouts. Cannon diagonal orientations are not covered.
Linked-restoration proof guards both bomb and contact-crumble definition sources,
not just their combining wrapper.

The source fixture now has twenty-three calls: eleven valid-domain operations
classify, while twelve invalid pointer/run/word requests still fail. Added cases
check exact selected-run widths, including an elevator's one-word versus four-word
runs, the cannon's asymmetric left layout, and cleared versus slope-access Chozo
frames. All ten production constructors accept complete constructed definitions
and reject omitted frames and duplicates. Actor AI, animation, camera, collision,
trigger filters and room changes are not executed or altered.

## PLM progression/boss-room layouts - nine additional original boundaries

Separately reviewed constructors require complete Tourian access-floor (five),
Speed Booster reveal (one), Maridia elevatube (one), Spore Spawn ceiling (four),
Samus Eater (eight), Botwoon wall (one), Kraid room (ten), Crocomire arena (five),
and Mother Brain fake-death (twenty-two) frame sets. Duplicate or omitted frames
cannot publish a provider. Array payloads are cloned; the two one-word providers
store the selected value. All nine rules cover only `GetWord` and guard the full
provider/definition sources. Underlying shared pointer sources are guarded too.

The tuple adapter uses each compiled frame's separate run widths. A Samus Eater
two-word run cannot borrow the four-word run's width; Mother Brain's one-word
side-tube run cannot borrow the neighboring five-word run. Kraid's twenty-two-word
clear does not widen its one-word crumble frame. Owned unused Mother Brain rows
are included because construction requires them, without claiming reachability.

The source fixture now contains forty-one calls: twenty supported-domain calls
classify and twenty-one invalid tuple requests remain failing findings. All
nineteen covered production constructors accept complete constructed entries
and reject omitted/duplicate frames. No PLM instructions, rooms, collision,
boss phases, ROM, saves or gameplay are executed. Dynamic caller correctness,
event timing and the visible artwork itself remain outside this proof.

## Flat doors and single-word PLMs - seven additional original boundaries

Blue, colored, grey and eye doors plus Mother Brain's escape gate require exact
complete authored frame sets. Collectible and Grapple-block constructors require
all twenty-four and five single-word identities respectively. Every constructor
rejects omissions/duplicates and stores independent selected art.

Blue-door construction requires sixteen authored frames; four closed caps map
to required opening frames. Eye-door construction requires twenty-three authored
frames; the mirrored clear uses the required four-word clear with horizontal
flip. Guarded native definitions enumerate all supported alias pointers and exact
widths. Unknown pointers are not accepted just because a family has aliases.
Flat methods use run zero, while a present dynamic run parameter in the prior
multi-run methods remains unknown. The collectible `pointer` argument receives
the same membership check as the other families' `drawPointer` arguments.

Sixteen constructed source calls first reproduce absent adapters. Eight classify
and eight invalid pointer/word requests remain failures, including a narrow eye
frame versus its four-word clear. Seven real constructors accept full constructed
entries and reject omissions/duplicates. Production `GetWord` confirms all four
blue closed-cap mappings and the eye clear's flip using constructed data.
No door transition, attack, pickup, room or gameplay runs; mechanics are unchanged.

## Player colors - sixteen additional original consumer boundaries

Nine private-constructor loaders validate complete independent arrays before
publishing a provider: Samus full-body cycles, normal suits, charge/pseudo-Screw,
hurt/intro, Hyper Beam body colors, visor, Crystal Flash, Power Bomb fixed colors,
and Hyper Beam projectile FX. Each rule guards the full provider source and
the underlying palette definition source where used. No palette clocks run.

The exact domains retain sparse native full-body palette pointers, normal suit
offsets zero/two/four, charge suit/phase bounds, hurt variants, and each independent
frame/color array. A known Power Bomb pre-explosion selector narrows the color
index to sixteen triplets instead of borrowing the explosion's thirty-two.
The visor's `TryResolveByteOffset` is intentionally different: an unsupported or
odd offset validly returns false; only `Resolve` demands a supported color index.
CGRAM destinations are placement, not fictional resource identities.

Twenty-seven constructed source calls reproduce absent adapters first. Fourteen
then classify, including the visor's false membership query; thirteen invalid
pointers, indices or variants stay failing findings. Prior contract checks and
compilation pass. No palette sequences, animation, damage, HDMA, rooms, ROM,
saves or gameplay were executed. Dynamic caller correctness is not asserted.

## Remaining enemy colors - sixteen additional original consumer boundaries

Ten separately reviewed private-constructor loaders require their full supported
arrays: Botwoon health; cutscene Baby initial/fades; Norfair Ridley initial/reveal;
four auxiliary enemy palettes; Kraid's five sources; Zebetite pulse; Shitroid
normal/targets; Ceres Ridley Mode-7 shades; Dachora phases; and Mother Brain health
pairs. Provider and dimension-definition source fingerprints guard every proof.

Selected dimensions remain distinct. Auxiliary palette constants narrow frame
and color ranges; Kraid's sixteen-word backdrop cannot borrow its 144-word health
capacity. Dachora Default owns only frame zero. The Baby's displayed fade index
is one through six, with fourteen colors rather than its fifteen initial colors.
Normal versus target Shitroid colors likewise retain separate widths. CGRAM
destinations are placement; direct indexed Norfair rows have CLR bounds checks.

Twenty-eight constructed calls reproduce absent adapters first. Fourteen then
classify and fourteen invalid constants remain failing findings. Known short
FaceBlock, DeadSidehopper, RoomBackdrop and Default selections receive their
precise failure reasons. All prior checks and compilation pass. No AI, battle,
fade, palette cadence, ROM, saves, rooms or gameplay runs; dynamic caller values
and player-visible correctness are not certified.

## Text and map presentation - twenty-four additional original boundaries

Seven reviewed providers already require complete resource sets before private
construction: the two escape programs, six narration pages, both ending text
sequences and fixed panels, 520 credits rows, 26 map sprite compositions, four
map arrow directions, and thirteen map-screen pages. Source fingerprints guard
the providers and their identity/layout definitions. Sparse sprite IDs, non-None
escape IDs, sequence/page enums and separate row/direction bounds are checked.

Credits has an alternate internal verification constructor with variable row
count. Any Core reference to that factory outside its provider revokes the
fixed production row proof. The production loader itself enforces 520 rows;
this rule does not infer retail dimensions solely from the provider's type.
Escape programs expose mutable line arrays: their rule proves complete program
membership, not line/glyph integrity after publication.

Map page names must be compiler constants/finite conditional sets or the exact
reviewed `WorldBackground`/`RoomFrame` factories. Those factories reject areas
outside the six Zebes maps, so unknown input has a finite six-name output set.
Known bad areas, arbitrary strings and lookalike functions remain unresolved;
a missing constant page remains a concrete missing identity. This does not
certify arbitrary caller input or VRAM placement.

Twenty-eight constructed source calls first reproduce absent adapters. Fifteen
then classify, twelve invalid or unknown selections remain unresolved, and one
absent named page remains missing. Additional source-only checks confirm that
alternate credits construction and a changed map factory revoke their proofs.
Compilation and prior checks pass. No assets, ROM, saves, menus, cinematics,
map navigation or gameplay were opened/executed; visual fidelity and timing are
not asserted.

## Pause presentation - seventeen additional original boundaries

Six private-constructor loaders require complete independent equipment base,
seven area backdrops/buttons, four wireframes, fourteen ordinary labels/Hyper,
six reserve origins/ten compositions, and sixteen selector anchors. Every
authored selector phase binds all three compositions before publication;
nonnegative phase indices normalize modulo its nonempty installed phase count.
These are resource-coverage rules, not inventory, input, sound or pixel checks.

Category/item selectors are correlated tuples, not independent maximum indices.
Boots cannot select the sixth equipment item; Reserve cannot borrow beam anchors.
Only Plasma may extend a five-word beam patch to nine using Varia artwork.
External Core access to the public mutable label `Keys` array revokes every
label proof, including when access appears outside the audited consumer files.
Negative phases, bad area/wireframe enums, absent reserve IDs and invalid origin
indices remain findings. Destination/live ownership behavior is not certified.

Twenty-six constructed source calls first reproduce absent adapters. Sixteen
then classify and ten invalid selections still fail. An external nested-array
mutation revokes all seven label calls in the fixture. Build and earlier contract
checks pass. No pause/menu sequence, sound, assets, ROM, saves or gameplay runs.

## Title/opening/Ceres presentation - eighteen additional original boundaries

Ten reviewed loaders require complete initial title colors, sixteen masked zoom
gradients, 31 title sprite identities derived from compiled selectors, four eye
rectangles, visible caret (including its legacy alias), three Mother Brain and
twelve explosion compositions, Ceres door art/colors/Mode-7 maps, decimal timer
art/anchors, and five Ceres warning overlay pages. Provider and identity sources
are fingerprint-guarded; sparse pointers and array indices retain exact bounds.
Title blank timed entries are deliberately not installed compositions.

The timer proves only Label/decimal digit/anchor coverage after BCD validation;
external Core access to mutable AnchorNames revokes its proof. Ceres overlays
are a complete owned-set membership query: unsupported sources or lengths may
validly return false. That rule does not prove the DMA caller chose a supported
source, nor certify warning placement/timing. Title gradients mask arbitrary
ushort zoom inputs into their sixteen loaded variants rather than imposing a
fictional zoom-input resource bound.

Twenty-four constructed source calls first reproduce absent adapters. Sixteen
classify and eight invalid pointer/index requests remain unresolved. A source
fixture accessing mutable timer AnchorNames revokes Draw's proof. Build and
prior checks pass. No cinematic, title, door, timer, palette sequence, ROM,
assets, saves or gameplay runs; mechanics and visual content are unchanged.

## Projectile/rope presentation - fifteen additional original boundaries

Eight guarded providers require all twelve beam sheets, both sixteen-direction
flare placement rows, 28 flare compositions/54 selectors, 42 trail appearances,
805 timed projectile bindings, 417 ordinary projectile compositions, four rope
segment appearances and 256 swing display angles. Sparse identity membership
and separate parameter bounds remain explicit. Timed trail resolution either
retains native attributes at an unconsumed list start or selects the installed
appearance four bytes before the next instruction; adjacent unaligned inputs
are not accepted merely because their numeric range is similar.

The shared projectile catalog has an alternate partial-set factory. Production
ordinary coverage is revoked by any Core reference outside its provider/private
flare wrapper. The flare wrapper itself does not expose that partial instance.
An external Core reference to the array-backed timed pointer property revokes
binding coverage. These source ownership guards are not runtime points-to proof,
host installation proof, or assertions about animation selection and physics.

Nineteen constructed source calls first expose missing adapters; eleven classify
and eight invalid requests remain unresolved. Two separate source-only fixtures
confirm partial-factory and mutable-pointer revocation. Build and prior checks
pass. No ROM, saves, art, firing, motion, damage, sound or gameplay is executed.

## Samus artwork tables - thirteen additional original boundaries

Four reviewed providers require the complete pose/landing/posture/drained offset
arrays, 253 upper/lower composition bases and 2096 indexed pointers, both four-word
direct atmospheric lists, and all cannon pose descriptors/drawing bytes/direction
attributes/tile selectors. Every nonzero indexed spritemap pointer must resolve
before public construction. Native zero pointers and unsupported Try queries
validly return false. Membership is not a claim about exposed part arrays after
publication, mutable-memory fallbacks, timing or pixel placement.

Cannon coverage is limited to the production Load path. External Core references
to its internal unvalidated FromPlacement factory or mutable TileSourcePointers
revoke that proof. Every provider's source declaration must also be among its
reviewed files: adding a partial implementation that can access private state
invalidates coverage even when the original files have not changed.

Twenty-three constructed source calls expose absent adapters first. Thirteen
classify; nine invalid indices and one body Frame call stay unresolved. Three
separate source fixtures confirm alternate factory, mutable IDs and partial
declaration revocation. Build and earlier checks pass. Body Frame/DefinitionAt
closure is not inferred from offset-array lengths. No ROM, saves, artwork,
rendering, movement or gameplay runs; production is unchanged.

## Background/effect/uploads - nine additional original boundaries

The four private boss BG2 wrappers require every declared Phantoon, Draygon,
Crocomire and Mother Brain body frame through the exact-set shared loader. Ordered
writes are independently compiled and bounds-checked. Unowned pointers, including
Mother Brain's OAM-only dummy, validly return false. This membership proof does not
certify the native operand selection, OAM/terrain layering, body position or timing.
The shared admission source and each family's definition sources are guarded.

Room FX requires all six 32x33 pages and eight sparse three-color blends. Apply(0)
is a compiled clear rather than an asset; Resolve(0) is invalid. Ending fragments
require four nonnull correctly sized atlases. Gunship takeoff requires five complete
1024-byte atlases; typed IDs have their own exact domain. Legacy source/length checks
are correlated: an owned source with a short transfer fails, while an unowned source
may validly return false. Destination/caller correctness is not inferred.

Nineteen constructed source calls reproduce absent adapters first. Thirteen qualify
and six invalid selections/lengths remain unresolved. Changing shared BG2 admission
revokes all four wrappers. Build and prior checks pass. No ROM, saves, artwork, room
FX, animation, rendering, scene or gameplay runs; production is unchanged.

## Closed menu name flow - five additional original boundaries

The named-key pass now follows compiler-bound local initializers, all their writes,
private-field initializers/writes, and closed private/local-helper call inputs.
Expression-bodied helpers with finite conditional/switch results are resolved from
source, including converted throw arms that cannot return a resource name. Every
possible returned name must be a loader-required key; finite absent names remain
concrete missing identities. No naming prefix or call-site allowlist is used.

Unknown writes, ref/out aliases, deconstruction, escaping helper method groups,
public fields/parameters, unavailable bodies and recursive flows remain unresolved.
This is static source flow, not arbitrary runtime/reflection/debugger-state validity
or interprocedural gameplay acceptance. Private strings are immutable values; the
entire compilation is checked for their assignments rather than just the nearby one.

Eighteen constructed source calls confirm six finite cases, eleven unresolved
boundaries and one missing-key case. Assertions retain the full assignment/input
unions and match the options helper's throwing fallback. Build and all prior checks
pass. No ROM, saves, art, menu navigation, rendering or gameplay runs.

## Room artwork sources - ten additional original boundaries

Three public catalogs check every character, metatile and static-palette source
selected by all 29 graphics sets for a nonnull resource, then copy their dictionaries.
Only those required sparse source domains qualify; arbitrary optional keys do not.
The seven-page sky store independently copies every byte before publication. Its
query owns aligned full pages and even-addressed complete 64-byte rows within the
store, including native mid-page overreads. Unowned sources may return false.

Known source/length constraints remain correlated: odd owned addresses, unaligned
full pages, short transfers and end-of-store overflow fail. The count-only library
background catalog cannot borrow graphics-set closure and remains unresolved.
These proofs do not establish optional key completeness, atlas geometry, graphics-set
selection, camera pointer arithmetic, upload destinations or visual correctness.

Seventeen constructed source calls first expose absent adapters. Nine qualify and
eight invalid/unsupported requests stay unresolved; changing required character
admission revokes its proof. Build and prior checks pass. No ROM, saves, art, room,
camera, upload, rendering or gameplay runs; production behavior is unchanged.

## Atomic area maps - eight additional original boundaries

The sole private-constructor path installs all seven area views before publishing
the catalog. Each map's full cell plane and independent exploration planes are
compiled by its loader. The same path requires HUD characters and both timer pages.
Resolve owns only the standard HUD, four Kraid restoration quarters and two escape
timer pages. Its constant identity domain is deliberately narrower than VramAssetId.

Ten constructed source calls expose absent adapters first; six qualify and four
remain unresolved (invalid area, foreign/None upload IDs, partial enemy catalog).
Changing the atomic install loop revokes both operations. This is successful-loader
resource completeness, not filesystem availability, selected host instance, map
centering, reveal correctness, upload destinations or rendered pixels. No ROM,
saves, artwork files, menu or gameplay was opened; production behavior is unchanged.

## X-ray and permanent-item stores - four additional original boundaries

X-ray Apply now has two inputs, not a separately supplied command. It resolves
the native command from the collision/BTS pair before selecting that pair's
installed operands. The constructor admits exactly every drawable compiled pair;
unowned pairs return null, and extension commands do not select artwork. This API
closes the command/operand identity gap rather than assuming callers correlate it.

The item overlay constructor independently copies all eight required metatiles.
Its optional room-overlay dictionary remains outside this proof. Permanent-item
construction requires all seventeen dynamic kinds, rejects duplicate/unknown/null
entries, and independently copies complete tile and palette payloads. Four fixed
tank/ammo kinds do not qualify as dynamic uploads.

A constructed complete reveal store and four chosen requests confirm command,
edited operand, extension and no-reveal behavior. Ten constructed source calls
expose missing adapters first; seven qualify and three remain unresolved. Changed
X-ray admission revokes both colocated providers. Audit and Verification builds
pass, as do auditor self-checks. No ROM, save, installed art or gameplay runs.
Traversal, host instance binding, destinations, clocks and pixels are not certified.

## Library-background exact keys - one additional original boundary

The count-only public constructor could accept 58 nonnull atlases with a substituted
key. A constructed fixture reproduced this admission hole before production changed.
The constructor now checks each of the 58 immutable compiled source identities for
a nonnull atlas, then independently copies the dictionary. The selected content-identity
fixture uses actual source identities instead of synthetic consecutive dictionary keys.

The substitution regression now passes, as does caller-dictionary independence.
Three source calls expose absent adapters first; two qualify and the known unowned
key remains unresolved. Removing exact-key admission revokes the rule. Audit and
Verification projects build and all auditor checks pass. No ROM, save, installed
artwork, room or gameplay was opened. Only required-key presence is certified, not
arbitrary source selection, page size selection, WRAM/VRAM destinations or pixels.

## Room-layout installation - two additional original boundaries

Production construction now requires a matching nonnull layout for every source
in the immutable compiled room-state domain, then independently copies the keys.
The former constructor admitted any subset; a constructed missing-source fixture
reproduced that gap before the production change. The importer already required
complete installation. Small content-identity fixtures now use an explicitly
internal partial factory, whose use outside the reviewed provider in Core revokes
the rule through semantic symbol ownership rather than a text-only name check.

Missing/null/mismatched source entries and dictionary independence are confirmed.
Three constructed source calls expose absent adapters first; two qualify and the
known unowned key stays unresolved. A constructed production partial-factory call
revokes all three classifications. Both affected projects build and all auditor
checks pass. No ROM, saves, installed layouts, rooms, rendering or gameplay was
opened. Membership does not certify caller source selection, geometry, collision
data, binding, destination placement or pixels.

## Mother Brain special sheets - one additional original boundary

The public constructor formerly checked only the four source keys and admitted
null or short atlases. A constructed null-sheet case reproduced the gap before the
change. Construction now copies the dictionary and requires the four immutable
source identities, each with its exact complete page length. Lookup takes only a
source ID; the existing transfer caller still verifies alignment, size and destination.

Null/short admission and caller-key independence are confirmed with authored zero
artwork. Three source calls expose absent adapters first; two qualify and a known
unowned key stays unresolved. Removing the size check revokes the source-guarded
proof. Audit/Verification builds and all auditor checks pass. No ROM, save, installed
artwork, battle, renderer or gameplay runs. Required sheet/page availability does
not establish arbitrary selectors, host binding, placement, clocks or pixels.

## Required X-ray room overlays - one additional original boundary

The importer already required room-overlay identities, but public catalog
construction admitted an empty room dictionary. A constructed eight-item catalog
with no overlays reproduced that admission gap before the fix. Public construction
now requires every nonzero key selected by immutable compiled room states and
independently copies all tiles. Partial synthetic overlays use a separate internal
factory; a semantic symbol guard revokes production closure if Core uses it.

Three constructed source calls expose absent adapters first; two qualify and zero
stays unresolved as a resource key. Caller-array independence and missing-overlay
rejection pass, and a constructed Core partial-factory caller revokes the proof.
The shared-source item/reveal contracts were re-reviewed and their prior invalid
item/kind checks still pass. Audit/Verification builds and all auditor checks pass.
No ROM, saves, installed reveals, room, renderer or gameplay ran. Only required
membership is certified, not arbitrary keys, coordinates, traversal, binding,
placement or pixels.

## Extended display-ID projection - one non-resource original boundary

GetDisplayPointer probes only the installed binding dictionary and returns the
selected ushort identity or the original native pointer. It does not index artwork
and cannot fail because the requested selector is unbound. This exact method now
has a source-guarded non-resource classification; TryGetDisplay stays unresolved.
Downstream BG2/OAM resource availability is not covered by the projection rule.

Four constructed source calls first expose two false missing-artwork requirements
and two unresolved boundaries. The three total projections now qualify while the
actual artwork query remains unresolved. Replacing the native-pointer passthrough
with a throw revokes the rule. Audit Release build and all auditor checks pass.
Production behavior is unchanged; no ROM, save, installed art or gameplay was opened.
This proves only absence of a resource request at this method, not correct visual
selection, frame coverage, BG2/OAM alignment, placement or pixels.

## Enemy tile/color/DMA installation - seven additional original boundaries

Public installed construction requires all 122 identities derived from compiled
retail graphics sets and independently pinned by their sorted-ID fingerprint.
Each needs a nonnull palette and a sheet with its exact masked native byte length.
DMA aliases are derived from compiled headers, or validated if supplied; the two
complete Ceres tile and overlay providers are mandatory. Dictionaries are copied.
The former public constructor admitted empty/partial dictionaries; a focused
fixture reproduced that gap before the production change. Partial fixtures now
use an internal factory whose Core use revokes the completeness rule.

LoadTo/LoadPaletteTo cover valid required definition identities. TryResolve covers
complete ordinary native sheets, bounded positive Ceres tile slices and exact
warning overlay pages; unowned sources remain false queries. Known bad definition
IDs and owned source/length mismatches stay findings. The rule also guards source
declarations of compiled graphics-set/header metadata, including new partials.
Other optional boss attachments are outside this rule's proof.

Constructed zero-art sheets confirm admission, dictionary independence and exact
VRAM/CGRAM transfers with unchanged neighbors. Fifteen source calls expose absent
adapters first; nine qualify and six invalid selectors/lengths remain findings.
Removing size admission, using the partial fixture factory in Core, or adding
metadata declarations revokes the rule. Audit self-checks and Audit, Verification
and Windows Desktop Release builds pass. No ROM, save, installed artwork, rendering
or gameplay ran. Caller selection, destinations, clocks and pixels are not certified.

## Enemy projectile direct/program frames - four additional original boundaries

The private constructor is reached only through Load. Current loading requires
each direct frame and each current compiled program-operand identity, then copies
compiled nonnull visual parts into private dictionaries. Older override formats
start with complete private stock dictionaries; every route to that stock type
has the same inductive completeness invariant. Get's native blank ID is an empty
composition, not missing external artwork. Known unowned IDs remain audit findings.

The source-guarded rule covers Get/GetProgramFrame valid resource domains. It
revokes on external Core references to the mutable direct-definition array or
additional metadata declarations. Program definition changes flow into both the
loader's required set and static identity set; the separate definition inventory
continues checking compiled program presentation dependencies. This does not prove
arbitrary slot identities, producer selection, projectile motion, timing or pixels.

Constructed empty visual parts confirm current admission, null/substitution
rejection, legacy inheritance and incomplete legacy stock rejection. Seven source
calls first expose five missing and two unresolved boundaries; five qualify and
two known unowned IDs remain findings. Changed admission or external definition
array access revokes the rule. Audit Release build and auditor checks pass; no
production behavior, ROM, save, installed artwork or gameplay changed.

## Ordinary/extended enemy display artwork - three additional original boundaries

Both private constructors are reached through complete loaders: every current
frame and display binding is required, and each target must be an installed
identity in the same bank (ordinary) or family (extended). Historical overrides
inherit complete private stock and replace only validated earlier members.
TryGetDisplay therefore has its own valid-domain resource rule. Extended fallback
includes the compiled empty extended frame. Known unowned bank/pointer pairs
remain findings. The total GetDisplayPointer ID projection retains its independent
source proof; routing by provider plus operation keeps these contracts separate.

Definitions expose read-only spans over private arrays. Added metadata declarations
revoke the affected rule, and derived identities flow into both installation and
audit selection sets. Their presentation dependencies remain checked by the
separate definition inventory. This is availability, not arbitrary slot inputs,
BG2 placement, collision, timing or pixels.

Constructed compositions confirm binding resolution, unowned-target rejection and
empty fallback. Seven source calls expose five missing and two unresolved boundaries
first; five qualify and two known unowned pairs remain findings. Changed shared
admission revokes both artwork rules; added ordinary metadata revokes only its
rule, leaving the independent extended rule intact. Existing projection checks
still pass. Audit Release build and auditor checks pass; no production gameplay,
ROM, save, installed art or enemy simulation was used.
