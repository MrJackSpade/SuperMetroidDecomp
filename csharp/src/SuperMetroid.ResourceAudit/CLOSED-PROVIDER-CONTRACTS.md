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
