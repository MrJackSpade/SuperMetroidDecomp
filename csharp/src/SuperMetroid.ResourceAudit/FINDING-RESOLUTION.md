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
