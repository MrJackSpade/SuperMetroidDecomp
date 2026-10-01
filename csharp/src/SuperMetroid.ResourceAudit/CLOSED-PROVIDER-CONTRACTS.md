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
calls, not a room or gameplay frame. Before applying the adapter, eighteen fixture
calls are unresolved. With it, nine valid reviewed calls are classified, missing
anchor/conditional-label keys remain concrete, and seven invalid/unreviewed/dynamic
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
