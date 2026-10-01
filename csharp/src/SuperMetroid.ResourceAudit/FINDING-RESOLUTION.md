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
