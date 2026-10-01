# Resource-audit finding reconciliation (#1156)

Starting audit: commit `87f924bf`, 266 distinct missing identities (573 reference
findings) and 353 unresolved analysis boundaries. This ledger accounts for fixes and
proven non-defects; it is **not** an audit suppression list or a passing baseline.

Investigations use source ownership, immutable declarations and actual provider routing.
Focused fixtures confirm identified changes; no gameplay probes discover new work.
Only documented, precise classification rules may remove irrelevant recurring findings.
New unsupported contracts must remain visible.

## Work Robot lasers — corrected missing dependency

- **Findings:** six direct OAM identities `8D:99FC`, `8D:9A03`, `8D:9A0A`,
  `8D:9A20`, `8D:9A36`, `8D:9A4C`, across fourteen reference findings.
- **Cause:** the seven timed operands in `WorkRobotLaserInstructionProgramDefinitions`
  were missing from `EnemyProjectilePresentationFrameDefinitions`. The actual projectile
  routing fell back to direct sprite lookup, but those six native compositions were not
  in the direct-art catalog either. The final operand reuses one earlier composition.
- **Source evidence:** the pinned disassembly's `bank_86.asm` declares the three prefix
  and four loop operands at `$86:D2EE..D306` for all five laser producer variants.
  `RoomEnemySystem.SetEnemyProjectileVisualOperand` chooses a program binding when
  installed; otherwise its selected pointer reaches `EnemyProjectileSpritemapCatalog.Get`.
  This is a real missing resource dependency, not a generated unused native record.
- **Correction:** add all seven operand bindings to the existing importer-owned catalog,
  bump projectile artwork to schema 12, and preserve schema-11 override edits while
  filling the new bindings from verified current stock. Incomplete old stock is rejected
  for installation repair rather than silently accepted.
- **Confirmation:** the constructed-data production catalog check failed before the fix
  on operand `D2EE`, then passed all seven composition lookups, preserved an existing
  legacy edit, inherited every new stock frame and rejected old stock without fallback.
  Static audit results removed precisely these six identities/fourteen references.
  No animation, AI, gameplay frame, ROM or player save was used for confirmation.
- **Player status:** implementation complete; actual appearance remains player validation.

After this correction: **260 missing identities (559 references), 353 unresolved**.
Those remaining findings are not yet classified or suppressed.

## Mama/Baby Turtle — corrected missing dependency

- **Findings:** twenty-nine bank-$A2 compositions selected by the tatori programs,
  `94D9..9535`, `9555`, `959D`, `95E5`, `96C9`, `96E9`, `9733..978F`,
  `97AF`, `97F7`, `983F` (only actual declared selections, not every address in these
  ranges), across seventy-five references.
- **Cause:** `MamaTurtleInstructionProgramDefinitions` and both enemy AIs existed,
  but their compositions were absent from `EnemySpritemapDefinitions`. The ordinary
  instruction interpreter sets `SpritemapPointer` from these compiled selections;
  `DrawLayers` then calls `DrawEnemySpritemap`, which rejects missing installed art.
- **Source evidence:** pinned `bank_A2.asm`, tatori lists `$8B80..8D50` and spritemaps
  `$94D9..983F`; the headers have ordinary OAM rendering, not a separate BG2 renderer.
  Unreferenced native compositions are not added merely because they are adjacent.
- **Correction:** derive the 29 required identities from the compiled program operands,
  append named editable OAM exports, and advance ordinary compositions to schema 62.
  Schema-61 overrides retain their edits and inherit the appended stock compositions;
  incomplete old stock requests installation repair. No runtime ROM access is added.
- **Confirmation:** the constructed-data catalog fixture first failed at `A2:94D9`.
  After correction it verifies all 29 display bindings and their actual authored parts,
  the exact append count, retained legacy edits, stock inheritance, and rejection of
  incomplete stock. The static report removes exactly 29 identities/75 references.
  No gameplay or animation search is involved.
- **Player status:** implementation complete; actual appearance remains player validation.

After this correction: **231 missing identities (484 references), 353 unresolved**.
The goal remains active; those findings have not been waived or suppressed.

## Common empty enemy frames — proven non-defect

- **Findings:** `A8:804D` and `B3:804D`, across seven generated selector references.
- **Evidence:** the pinned banks declare their `$804D` ordinary spritemap with zero
  parts. `CommonEnemyEmptyExtendedFrameDefinitions.HasEmptySpritemap` models this
  record for an explicit supported bank set. `DrawEnemySpritemap` already returns
  without emitting OAM when that predicate matches. No installed composition is needed.
- **Rule:** count the production-owned empty definitions in the display domain only.
  Use the actual supported bank set and predicate, not a hand-written finding allowlist.
  Statically guard the renderer's predicate arguments and no-op branch: a change requires
  adapter review. Direct `EnemySpritemapCatalog` lookups are not considered satisfied
  by a renderer fallback. JSON records the compiled definition owner, source and reason.
- **Confirmation:** the two identities are recognized, an unsupported bank and adjacent
  pointer remain rejected, and synthetic changed draw behavior/arguments invalidate the
  source guard. Static audit removes precisely two identities/seven references.
- **Player status:** no gameplay change or player validation needed for this classification.

After this classification: **229 missing identities (477 references), 353 unresolved**.
The goal remains active; no broad suppression or passing baseline has been introduced.

## Zero crawler — corrected missing dependency

- **Findings:** sixteen `Spritemap_Zero` compositions: `$A3:995B..9A16` and
  `$A3:9B37..9B6A` (declared frame starts only), across twenty-four references.
- **Cause:** Zero's shared crawler wrapper and all four orientation programs were
  compiled, but none of their selected compositions were exported. This uses the same
  ordinary instruction/display path as the missing tatori artwork.
- **Source evidence:** pinned `bank_A3.asm`, Zero loops `$984B`, `$988B`, `$98AB`,
  `$990B`, and sprite records at `$995B..9B6A`. `CrawlerAnimationDefinitions` selects
  these four loops for the Zero species; each loop uses four distinct compositions
  across six timed steps. No adjacent unselected sprite is added.
- **Correction:** append the sixteen program-derived, editable ordinary OAM compositions,
  advance the schema to 63, and preserve schema-62 overrides through stock inheritance.
  Older supported overrides, including schema 61, remain supported. No AI or timing
  behavior changes, and no runtime cartridge reads are introduced.
- **Confirmation:** the production-loader fixture first failed at `A3:9B37`, then
  confirmed all 16 compositions and authored parts, the precise family append, retained
  old edits, inherited new frames and rejection of incomplete old stock. The ordinary
  catalog fixture is now reusable by explicitly identified families; the Turtle fixture
  also passes after this extraction. Static audit removes exactly 16 identities/24 references.
- **Player status:** implementation complete; actual appearance remains player validation.

After this correction: **213 missing identities (453 references), 353 unresolved**.
The goal remains active. Original identities accounted so far: 51 corrected missing
dependencies (six Work Robot, twenty-nine tatori, sixteen Zero), two proven no-op
definitions, and 213 still under investigation.

## Friendly animals — corrected missing dependencies

- **Findings:** 82 ordinary compositions, across 199 selector references:
  31 Etecoon frames (`A7:EEED..F20A`), 29 Dachora body/echo frames (`A7:F9C4..FF53`),
  ten rescue Etecoon frames (`B3:E736..E91F`), twelve rescue Dachora frames
  (`B3:EB1B..ED3E`). Ranges denote selected frame starts, not all adjacent records.
- **Cause:** all four actors already initialize and run compiled animation programs,
  but their ordinary compositions were absent from the production export manifest.
  Their timed selectors therefore reach the generic OAM draw path without installed art.
- **Source evidence:** the pinned bank-$A7/$B3 declarations and the four corresponding
  `*InstructionProgramDefinitions` identify their selected ordinary OAM frames.
  `RoomEnemySystem.Etecoon`, `.Dachora`, and `.EscapeAnimals` enable instruction
  processing; the mechanics reader dispatches to those exact program declarations.
  No alternate renderer supplies these compositions.
- **Correction:** derive named exports from the four explicitly owned finite programs,
  append the 82 compositions, and advance the schema to 64. The shared declaration
  builder deduplicates repeated selections and recognizes only the production-owned
  empty sprite convention. Schema-63 and earlier supported overrides retain their
  edits and inherit newly required stock frames. AI, routes, jumps and timing are unchanged.
- **Confirmation:** separate focused catalog checks failed before registration at
  `A7:EFFF`, `A7:F9C4`, `B3:E736`, and `B3:EB1B`, respectively. All now confirm their
  exact selected composition sets and authored parts, legacy edits, new-frame inheritance,
  and rejection of incomplete old stock. The static audit removes exactly 82 identities
  and 199 reference findings. No ROM, rooms, gameplay frames or saves were opened.
- **Player status:** implementation complete; actual appearance remains player validation.

After this correction: **131 missing identities (254 references), 353 unresolved**.
Original identity accounting: 133 corrected dependencies, two compiled no-op definitions,
131 still under investigation. No remaining analysis boundary has been waived.

## Environmental enemies and Tourian baby Metroid — corrected missing dependencies

- **Findings:** 53 ordinary OAM identities across 99 references: Hibashi/fire pillar
  (23, `A6:9082..9469`), Zebetite barrier health tiers (10, `A6:FE08..FEB0`), Wrecked
  Ship ghost/Coven (three, `A8:9E46..9E72`), Powamp body/balloon (six, `A8:C675..C698`),
  Spark activation/active/emitter (eight, `A8:E71F..E79B`), and the Tourian baby Metroid
  drain/remorse frames (three, `A9:F9A8..FAD8`). Ranges denote selected frame starts.
- **Cause:** the six actors' compiled instruction catalogs and initializer/main dispatch
  were present, but their ordinary compositions were omitted from the export manifest.
  Each therefore reached the existing generic draw path without the selected artwork.
- **Source evidence:** the pinned bank-$A6/$A8/$A9 sprite/program declarations match
  the corresponding six `*InstructionProgramDefinitions`. `RoomEnemySystem` dispatches
  the initializers and mechanics reader to those declarations; Zebetite dynamically
  selects its declared health-tier program. These are ordinary sprites, not BG2 streams.
- **Correction:** add dedicated domain-named visual definition catalogs, derive exactly
  the selected frames using the shared finite declaration builder, append 53 exports,
  and advance the ordinary schema to 65. Schema-64 and older supported overrides retain
  edits and inherit all new stock compositions. No combat, timing, AI or movement change.
- **Confirmation:** six independent production-catalog checks failed before registration
  at `A6:9082`, `A6:FE08`, `A8:9E46`, `A8:C675`, `A8:E74F`, and `A9:FAD8`. All now
  confirm each family's exact selected compositions and authored parts, legacy edits,
  stock inheritance and old-stock rejection. Static audit removes precisely 53 identities
  and 99 references. No gameplay, ROM, save or parameter-search execution is involved.
- **Player status:** implementations complete; actual appearance remains player validation.

After this correction: **78 missing identities (155 references), 353 unresolved**.
Original identity accounting: 186 corrected dependencies, two compiled no-op definitions,
78 still under investigation. The original unresolved boundaries remain visible.

## Crocomire corpse/skeleton — corrected missing dependencies and capacity

- **Findings:** 33 extended OAM roots (`A4:E1FE..E716`) across 35 references.
- **Cause:** the existing extended export manifest included Crocomire's tongue and
  fight-body frames, but omitted every corpse/skeleton root. `CrocomireSkeletonArtwork`
  supplies tile uploads only, not those composite offsets or OAM parts. The ordinary
  eight-component limit also rejected the thirteen-component fragmentation poses.
- **Source evidence:** pinned `bank_A4.asm` declares `ExtendedSpritemap_CrocomireCorpse_0`
  through `_20`; `_E`, `_F` and `_10` each contain thirteen components. Compiled
  `CrocomireInstructionProgramDefinitions` selects all 33 roots. `RoomEnemySystem.CrocomireDeath`
  activates its falling, fragmentation, stable and river programs; the common extended
  OAM writer consumes their compositions. These are not BG2 streams or unused art.
- **Correction:** derive a dedicated skeleton visual catalog from the compiled selectors,
  append its 33 roots, and advance the extended composition schema from 27 to 28.
  Older overrides inherit the new stock frames without losing component edits or visual
  bindings. Import and load allow thirteen components only for declared skeleton roots;
  all other OAM families retain their existing eight-component limit.
- **Confirmation:** the focused check failed before registration at `A4:E1FE`; it now
  verifies all selected roots and offsets, exact schema append, legacy edits and bindings,
  inheritance and incomplete-stock rejection. Constructed bytes for the thirteen-component
  root pass through the real importer and loader with exact order/offset/OAM assertions.
  Fourteen components and nine components on an unrelated family are rejected. The fake
  source deliberately lacks hitbox bytes, confirming they are not imported as editable art.
  Static audit removes exactly 33 identities and 35 references; no gameplay was executed.
- **Player status:** implementation complete; actual battle appearance remains player validation.

After this correction: **45 missing identities (120 references), 353 unresolved**.
Original identity accounting: 219 corrected dependencies, two compiled no-op definitions,
45 still under investigation. No analysis boundary has been waived.

## Kraid foot and belly lint — corrected missing dependencies

- **Findings:** 37 identities across 108 references: 35 extended foot roots
  (`A7:8CE3..8F47` and initial `A7:A565`) and two ordinary belly-lint frames
  (`A7:A5DF`, `A7:8C6C`).
- **Cause:** the compiled foot and lint programs existed, but both families were omitted
  from their respective composition manifests. Kraid's BG2 body and installed arm
  compositions do not provide either family's independent artwork.
- **Source evidence:** pinned bank-$A7 declarations explicitly distinguish foot extended
  maps from lint ordinary spritemaps. The bank-$A1 population supplies the foot's extended
  flag (`extraProperties $0004`); the translated population preserves it. Initializers,
  growth and movement dispatch use `KraidFootInstructionProgramDefinitions` and
  `KraidLintInstructionProgramDefinitions`, then the existing common draw paths.
- **Correction:** derive dedicated foot/lint catalogs from those programs. Append 35 foot
  roots to extended schema 29 and two lint frames to ordinary schema 66. Preserve older
  overrides, component edits and display bindings; move the initial lint frame identity
  into its domain catalog. No combat, AI, BG2, layer, collision or animation-timing change.
- **Confirmation:** two checks failed before registration at foot `A7:8CE3` and lint
  `A7:A5DF`. Both now confirm every selected composition, exact manifest additions,
  retained authored offsets/OAM, legacy edits/bindings, new-stock inheritance and
  incomplete-old-stock rejection. The Crocomire skeleton capacity/migration check still
  passes after this schema change. Static audit removes exactly 37 identities and 108
  references. No gameplay, ROM, save or parameter-search execution was involved.
- **Player status:** implementations complete; appearance remains player validation.

After this correction: **eight missing identities (12 references), 353 unresolved**.
Original identity accounting: 256 corrected dependencies, two compiled no-op definitions,
eight still under investigation. The unresolved boundaries remain visible.

## Puromi/Nuclear Waffle head — corrected missing dependencies

- **Findings:** eight ordinary head compositions (`A6:9954..9985`), twelve references.
- **Cause:** the translated actor's head animation existed under the `NuclearWaffle`
  name, but its ordinary head compositions were omitted from the manifest. Its
  independently animated bank-$86 projectile links are a different resource family.
- **Source evidence:** pinned `InstList_Puromi` at `A6:9490` selects the eight maps;
  `NuclearWaffleInstructionProgramDefinitions` compiles that twelve-frame loop.
  Initialization/main dispatch maps native Puromi callbacks to the translated Nuclear
  Waffle actor, and its mechanics reader uses that program. These are active dependencies.
- **Correction:** add the program-derived head visual catalog and append its eight
  frames in ordinary schema 67. Schema-66/older overrides preserve edits and inherit
  the head frames from current stock. No arc physics, links, AI or timing change.
- **Confirmation:** the focused check failed before registration at `A6:9954`; it now
  confirms all eight selected compositions and authored parts, exact schema append,
  legacy edits, stock inheritance and incomplete-stock rejection. Static audit removes
  exactly eight identities and twelve references without executing gameplay.
- **Player status:** implementation complete; appearance remains player validation.

Original missing-identity reconciliation is now complete: **264 corrected production
dependencies plus two source-proven compiled no-ops = all 266 original identities**.
Current audit: **zero missing identities, 353 unresolved analysis boundaries**. The latter
remain explicitly failing findings, not silently waived or treated as proven resource bugs.

## Mother Brain cutscene bank and fake-death color rows — resolved metadata gaps

- **Original boundaries:** `MotherBrainBabyInstructionProgramDefinitions` had no declared
  bank; the fake-death room palette program was incorrectly offered to the generic sprite
  operand adapter. These were two of the original 353 unresolved boundaries.
- **Source evidence:** pinned bank-$A9 `InstList_BabyMetroid_Initial`,
  `InstList_BabyMetroid_DrainingMotherBrain` and `InstList_BabyMetroid_TakingFatalBlow`
  select `F9A8/FA40/FAD8`. Their nine operand addresses were also missing from the
  production selector registry because the generator required a byte-level mechanics
  probe instead of accepting the program's explicit bank. The three OAM compositions
  are shared with the already installed Tourian baby; no additional art is needed.
- **Correction:** declare the bank, restore all nine sparse selector bindings, and teach
  the generator to retain explicit-bank catalogs. Add a separate typed palette adapter
  that follows the real importer and validated loader's fourteen timed-entry identities.
  It uses constructed zero colors, never a ROM. Palette operand addresses are not OAM.
- **Confirmation:** the new bank metadata made the missing nine bindings concrete, and
  the focused contract check failed before their correction. It now confirms the exact
  native frame sequence, installed target frames and rejection of adjacent mechanics
  words. Palette checks confirm fourteen concrete omissions when exports are absent,
  fourteen imported/loaded rows, changed-identity rejection and truncated-document
  rejection. Windows compilation passes. No room, AI or gameplay execution was used.
- **Accounting:** all original 266 missing identities remain accounted for. Nine additional
  production bindings identified by resolving the bank gap are corrected. Current audit:
  **zero missing identities, 351 unresolved**. The remaining dynamic consumer boundaries
  still fail explicitly. Cutscene appearance remains player validation.

## Closed HUD/file-select/room-color providers — 52 source-proven coverage gaps

- **Findings:** 24 HUD operations, 23 bounded/constant-name file-select operations,
  and five Mother Brain room-color operations had no consumer-domain adapter.
- **Cause:** the audit treated every dynamic argument as an unknown resource identity,
  including indices into complete loader-validated arrays and pure layout operations.
  Source inspection found no missing resource in these particular valid domains.
- **Classification:** explicit method-specific closed-provider rules now account for
  these boundaries. Full loader/selector/definition source fingerprints guard every
  proof; new operations/types and changed contracts stay unresolved. Constant file-select
  patch/anchor names are still checked individually. A new use of the HUD's mutable
  key array outside the reviewed source files invalidates its closure proof.
- **Evidence:** [CLOSED-PROVIDER-CONTRACTS.md](CLOSED-PROVIDER-CONTRACTS.md) describes each
  accepted operation, validation path, bounds and exclusions. JSON schema 2 retains
  every consumer and adds `classifications` with proof reasons and source fingerprints.
  These are not baselines or claims that arbitrary caller inputs are correct.
- **Confirmation:** a nine-call constructed source fixture reproduces the missing-adapter
  boundaries before classification. Four reviewed calls then qualify; a missing anchor
  stays concrete, and invalid constants, dynamic names and unreviewed operations still
  fail. Changed selector source and external mutable-key usage revoke the proof at the
  production audit consumer pass. No assets, ROM, saves or gameplay are executed.
- **Accounting:** two metadata boundaries plus these 52 consumer classifications account
  for **54 of the original 353 unresolved boundaries**. Current audit: **zero missing
  identities, 299 unresolved**, all still visible as failing findings. The two dynamic
  file-select page/border names are deliberately among the remaining findings.

## Options/game-over/reserve UI — 25 further source-proven boundaries

- **Findings:** twelve options calls, seven game-over calls and six reserve-UI calls
  lacked definition coverage despite using complete loader-validated resource domains.
- **Cause/classification:** the source pass did not recognize fixed menu fields, exhaustive
  baby-frame/palette enum selectors, bounded controller/cursor/digit indices or masked
  arrow frames. New method-specific rules account for those domains using full reviewed
  provider/definition source guards. Named options pages/toggles and reserve labels are
  compared independently with their required key sets.
- **Finite names:** the named adapter now resolves constant conditional unions. The
  Auto/Manual branch requires both installed names, not just whichever is currently
  selected. Unknown parameters/function results remain unresolved; three options helper
  selections and the two file-select dynamic-name calls have not been waived.
- **Confirmation:** the scoped source fixture reproduces eighteen missing-adapter calls
  first. Nine reviewed calls classify, two missing named identities stay concrete, and
  seven invalid/unreviewed/dynamic selections fail. Invalid enum/digit constants and a
  missing conditional label are included. Existing changed-source/mutable-key safeguards
  still pass. This is source-only confirmation, not gameplay or bug hunting.
- **Accounting:** 77 guarded consumer classifications plus two metadata corrections now
  account for **79 of the original 353 boundaries**. Current audit: **zero missing
  identities, 274 unresolved**, with the entire 352-site consumer inventory retained.
  These changes affect development auditing only, not player-facing behavior.

## Beam/boss palette domains — 29 further source-proven boundaries

- **Findings:** five beam-palette, eight Ceres/shared Ridley, four Crocomire, three
  Spore Spawn, three Draygon, three Phantoon and three Tourian statue consumer sites.
- **Cause/classification:** these calls selected rows/colors in complete loader-validated
  arrays, but the audit lacked their finite domain contracts. Source-reviewed operation
  rules now record that coverage with provider/definition fingerprints. Unguarded or
  unreviewed raw methods remain excluded; this is not an exemption for every color catalog.
- **Bounds:** a dedicated index-domain catalog now holds the reviewed ranges/strides,
  replacing inline bounds in the audit consumer. Odd health-band/eye byte selectors and
  invalid constants still fail. Known Spore Spawn level/background death frames use the
  narrower seven-row domain. Draygon's white-hurt branch does not select health colors,
  so an ignored health argument is not falsely rejected.
- **Confirmation:** the fixture's thirty-three calls reproduce unresolved boundaries
  before classification; seventeen reviewed calls qualify, two missing names remain
  concrete, and fourteen invalid/unreviewed/dynamic selections stay failing gaps.
  Per-provider valid/invalid palette checks and the white-hurt exception pass, together
  with prior source-change/mutable-key safeguards. No art, ROM, save, boss AI, palette
  animation or gameplay frame was run.
- **Accounting:** 106 guarded consumer classifications plus two metadata corrections
  account for **108 of the original 353 boundaries**. Current audit: **zero missing
  identities, 245 unresolved**, with all 352 consumer sites retained. Production boss
  behavior is unchanged by this audit-only correction.

## Complete imported color sequences - 22 further source-proven boundaries

- **Findings:** eight Mother Brain rainbow/fade, four Mother Brain death, three
  Chozo/tube, three gameplay base and four Samus death-palette consumer sites.
- **Cause/classification:** these calls use complete validated arrays, not dynamically
  missing artwork. Method-specific source-guarded contracts now account for them.
  Samus's public death-art constructor validates and deep-clones its input, so it has
  the same complete-domain property without assuming a private loader. Unreviewed
  raw methods remain unresolved; arbitrary dynamic indices are not certified.
- **Bounds:** the beam's final aligned cursor is a compiled terminator, not an absent
  color; only 0..152 in steps of four qualify. Dedicated index domains retain precise
  frame, color, suit and explosion limits. CGRAM destinations are placement arguments,
  not additional resource names.
- **Confirmation:** forty-nine constructed source calls are unresolved before the
  adapter. Twenty-six then classify, two missing names stay concrete, and twenty-one
  invalid/unreviewed/dynamic calls remain unresolved. Endpoint and invalid-cursor checks,
  death/suit/explosion bounds, and existing source-change/mutable-key safeguards pass.
  Compilation succeeds. No ROM, saves, palette animation or gameplay were executed.
- **Accounting:** 128 guarded consumer classifications plus two metadata corrections
  account for **130 of the original 353 boundaries**. Current audit: **zero missing
  identities, 223 unresolved**, all 352 consumer sites retained. This changes auditing
  only; production palette behavior is untouched.

## Gameplay-message families - 15 further source-proven boundaries

- **Findings:** seven notice, four item-panel and four one-row-title consumer sites.
- **Cause/classification:** each loader already requires its complete owned message
  set, but the consumer pass had no adapter. New source-guarded method contracts
  distinguish complete templates/text from unknown family routing. Membership queries
  can validly return false; they are not missing-resource requests.
- **Ownership:** a dedicated sparse identity catalog rejects constant cross-family
  Build requests. YES/NO selection belongs only to the two save notices, not all five
  notices. Definitions and the shared glyph compiler are included in the proof guards.
- **Confirmation:** eight constructed calls reproduce missing adapters before the
  change. Five reviewed operations then classify; three incorrect family/save-only
  requests remain failing findings with precise ownership reasons. Prior source-only
  checks and compilation pass. No messages, rooms, ROM or gameplay were executed.
- **Accounting:** 143 guarded consumer classifications plus two metadata corrections
  account for **145 of the original 353 boundaries**. Current audit: **zero missing
  identities, 208 unresolved**, all 352 consumer sites retained. The production
  message system and player input/timing are unchanged.

## PLM multi-run visual domains - five further source-proven boundaries

- **Findings:** shot-block, station, Bomb Torizo hand, Mother Brain glass and n00b
  tube `GetWord` calls lacked resource-domain adapters.
- **Cause/classification:** all five constructors already reject unknown/duplicate
  frame identities, require complete coverage and exact payload shapes, and clone
  the selected art. Reviewed source-guarded contracts now account for that complete
  supported resource domain, without assuming every runtime caller value is valid.
- **Shape checks:** the adapter uses actual compiled draw declarations. A constant
  pointer narrows run/word bounds to that frame rather than an unrelated maximum.
  Flattened providers retain their native per-run limits. New families are excluded
  until separately reviewed; no broad PLM wildcard suppression was added.
- **Confirmation:** thirteen constructed source calls first reproduce the unknown
  adapter. Six then qualify; seven bad pointer/run/word requests stay failing gaps.
  All five real constructors accept complete constructed entries and reject omitted
  frames and duplicates. Build and existing audit checks pass. No ROM, saves, room,
  instruction stream, animation or gameplay execution was used.
- **Accounting:** 148 guarded consumer classifications plus two metadata corrections
  account for **150 of the original 353 boundaries**. Current audit: **zero missing
  identities, 203 unresolved**, all 352 consumer sites retained. Production PLM
  mechanics, timing, collision and artwork are unchanged.

## PLM actor layouts - five further source-proven boundaries

- **Findings:** downward-gate, elevator-platform, Draygon-cannon, Chozo-statue and
  linked-restoration visual word lookups lacked resource-domain adapters.
- **Cause/classification:** each separately reviewed constructor already requires
  its complete compiled frame set and exact payload shapes, rejects unknown/duplicate
  identities and clones the selected art. Source-guarded contracts account for these
  five supported domains only. Unsupported diagonal cannon orientations are excluded.
  Both bomb and contact-crumble definitions guard the linked-restoration proof.
- **Shape checks:** the existing exact tuple adapter now consumes each family's
  compiled declarations. Known narrow elevator/cannon/cleared-hand runs cannot borrow
  a different run's width; native geometry and collision words remain untouched.
- **Confirmation:** twenty-three constructed source calls reproduce unknown adapters
  first; eleven classify and twelve invalid pointer/run/word requests remain failures.
  All ten covered production constructors accept complete constructed entries and
  reject omitted frames and duplicates. Compilation and prior scoped checks pass.
  No ROM, saves, rooms, PLM instruction streams or gameplay were executed.
- **Accounting:** 153 guarded consumer classifications plus two metadata corrections
  account for **155 of the original 353 boundaries**. Current audit: **zero missing
  identities, 198 unresolved**, all 352 consumer sites retained. This commit changes
  the development audit only, not player-facing PLM behavior.

## PLM progression/boss-room layouts - nine further source-proven boundaries

- **Findings:** Tourian access-floor, Speed Booster reveal, elevatube, Spore Spawn
  ceiling, Samus Eater, Botwoon wall, Kraid room, Crocomire arena and Mother Brain
  fake-death visual word lookups lacked resource-domain adapters.
- **Cause/classification:** each constructor already requires its complete known
  frame set and exact payload shape, rejects duplicate/omitted frames, and stores
  independent selected art. Nine method-specific source-guarded rules now account
  for those supported domains. Shared pointer sources are guarded; no mechanics,
  boss phase, collision, timing or player-visible art changed.
- **Confirmation:** forty-one constructed source calls reproduce absent adapters
  first, then twenty classify and twenty-one invalid pointer/run/word requests
  remain failures. Cases distinguish narrow/wide Samus Eater and Mother Brain
  runs, Kraid crumble/clear frames, and single-word reveal/elevatube limits.
  All nineteen covered production constructors accept complete constructed data
  and reject omitted/duplicate frames. Build and prior scoped checks pass.
  No ROM, saves, rooms, instruction streams or gameplay were executed.
- **Accounting:** 162 guarded classifications plus two metadata corrections account
  for **164 of the original 353 boundaries**. The current report has **zero missing
  identities, 189 unresolved**, with all 352 consumer sites retained. Unresolved
  findings still fail the audit; these are resource-domain proofs, not certification
  of arbitrary runtime caller values or whole-game parity.

## Flat doors and single-word PLMs - seven further source-proven boundaries

- **Findings:** blue/colored/grey/eye door, escape-gate, collectible and Grapple-block
  artwork calls lacked domain adapters, including compiled reused-artwork pointers.
- **Cause/classification:** existing constructors require complete known frame sets
  and independent payloads. Method-specific source guards now cover those seven
  domains. Blue closed caps use required opening art; mirrored eye clears use
  required authored clear art with horizontal flip. No fallback artwork was invented.
- **Confirmation:** sixteen constructed source calls reproduce absent adapters first;
  eight then classify and eight invalid pointers/widths remain failing findings.
  All seven constructors accept complete constructed data and reject omissions and
  duplicates. Production lookups confirm four blue aliases and the mirrored eye
  alias using constructed data. Prior checks and compilation pass. No ROM, saves,
  rooms, door transitions, pickup effects or gameplay were executed.
- **Accounting:** 169 guarded classifications plus two metadata corrections account
  for **171 of the original 353 boundaries**. The current audit has **zero missing
  identities, 182 unresolved**, all 352 consumers retained. Production behavior
  and artwork are unchanged; dynamic caller correctness is not asserted.

## Player color domains - sixteen further source-proven boundaries

- **Findings:** nine player-color providers' sixteen call sites lacked resource
  coverage adapters: full-body cycles, normal suit, charge, hurt, Hyper Beam,
  visor, Crystal Flash, Power Bomb fixed colors and Hyper Beam projectile FX.
- **Cause/classification:** existing private-constructor loaders already validate
  every supported palette array and compile independent words. Guarded per-method
  rules now account for those exact sets, with sparse pointer/suit ownership and
  precise frame/color/variant bounds. A known short pre-explosion stream cannot
  borrow explosion capacity. Unsupported visor offset queries validly return false;
  they do not demand artwork outside the six installed colors.
- **Confirmation:** twenty-seven constructed source calls reproduce missing adapters
  first; fourteen then classify and thirteen bad pointers/indices/variants remain
  failing findings, including a known pre-explosion index in the longer stream's
  range. All prior checks and compilation pass. No ROM, saves, rooms, palette
  sequence, animation, HDMA, damage or gameplay were executed.
- **Accounting:** 185 guarded classifications plus two metadata corrections account
  for **187 of the original 353 boundaries**. Current report: **zero missing
  identities, 166 unresolved**, all 352 consumers retained. Production colors,
  clocks and player behavior are unchanged. Dynamic caller correctness and
  whole-game parity are not asserted.

## Remaining enemy color domains - sixteen further source-proven boundaries

- **Findings:** sixteen calls across Botwoon, cutscene Baby, Norfair Ridley,
  auxiliary enemy palettes, Kraid, Zebetite, Shitroid, Ceres Ridley Mode-7,
  Dachora and Mother Brain health colors lacked resource-domain adapters.
- **Cause/classification:** their private-constructor loaders already require
  complete independent arrays. Reviewed, source-guarded rules now account for
  those ten providers. Selected short palettes cannot borrow a larger family's
  frame/color range; Baby fade indices stay one-based. AI, damage, animation,
  palette cadence and visual data are unchanged.
- **Confirmation:** twenty-eight constructed source calls reproduce absent
  adapters first; fourteen classify and fourteen invalid constants still fail.
  Precise short-selection findings cover FaceBlock, DeadSidehopper, Kraid's
  RoomBackdrop and Dachora Default; Baby initial/fade widths and one-based
  fade indexing are checked separately. Compilation and prior checks pass.
  No ROM, saves, rooms, AI, battle, palette sequence or gameplay were executed.
- **Accounting:** 201 guarded classifications plus two metadata corrections
  account for **203 of the original 353 boundaries**. Current report: **zero
  missing identities, 150 unresolved**, all 352 consumers retained. Dynamic
  caller correctness and player validation remain outside these resource proofs.

## Text and map domains - twenty-four further source-proven boundaries

- **Findings:** text/credits and map sprite/arrow/page calls lacked adapters.
- **Cause/classification:** seven existing loaders already require their complete
  supported resource sets. Method-specific, fingerprint-guarded rules now account
  for them. Map strings remain constrained to finite constants or two reviewed
  bounded factories. A Core use of the variable-row credits verification factory
  revokes its production row proof. Mutable escape line arrays are explicitly
  excluded from the program-membership claim. Production behavior is unchanged.
- **Confirmation:** twenty-eight constructed source calls reproduce absent
  adapters first; fifteen classify, twelve invalid/unknown requests stay
  unresolved and one absent named page stays missing. Source mutations/alternate
  credits construction invalidate the relevant proofs. Build and all prior
  auditor contract checks pass. No ROM, assets, saves or gameplay were opened.
- **Accounting:** 225 guarded classifications plus two metadata corrections
  account for **227 of the original 353 boundaries**. Current report: **zero
  missing identities, 126 unresolved**, all 352 consumers retained. The audit
  still exits one for those remaining findings; no baseline was added.
