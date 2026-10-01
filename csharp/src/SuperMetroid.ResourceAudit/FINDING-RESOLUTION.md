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
