# Static queued VRAM DMA audit

Issue #1163 extends the PLM ownership audit to every `VramWriteQueue.Enqueue`
and `EnqueueAsset` invocation in Core, including artwork helper classes. It uses
Roslyn symbols, not receiver names or a gameplay playthrough. It does not open a
ROM, asset installation, save, recording, renderer, or frame dispatcher.

Run from the repository root:

```powershell
dotnet build csharp/src/SuperMetroid.ResourceAudit -p:RunQueuedVramDmaAudit=true
dotnet run --project csharp/src/SuperMetroid.ResourceAudit -- --vram-dma-audit --root . --json out/queued-vram-dma-audit.json
dotnet run --project csharp/src/SuperMetroid.ResourceAudit -- --vram-dma-self-check
```

The dedicated CI workflow runs on Core, importer, and audit changes. Windows
release packaging runs the same gate. Missing ownership, wrong transfer lengths,
changed reviewed contracts, and unclassified producers produce a failing exit
code. Process exceptions remain stderr/nonzero, never a Windows dialog.

## What is checked

- Constant source/count pairs are compiler-resolved, including named arguments
  and constant casts.
- Escaping queue method groups (delegate producers) fail explicitly instead of
  bypassing the direct-invocation inventory.
- Nonconstant producers require a reviewed containing-method fingerprint and a
  correlated descriptor adapter. Adding a call or changing its arithmetic revokes
  that proof; unknown families are not silently skipped.
- Compiled families cover ordinary enemy uploads, Ceres lists, corpse WRAM,
  Kraid restoration, gunship takeoff, room FX, Tourian statues, treadmills,
  Samus death/arm cannon, beam/trail/Grapple tiles, and Landing Site/sky transfers.
- The shared PLM source runs its full compiled-program descriptor/operand/ownership
  audit. Its packed source/count pairs are not replaced with guesses.
- Native immutable transfers must match the **actual runtime resolver's** owned
  ranges and lengths. Typed transfers must match its owned asset IDs and lengths.
  Source contracts include required admission, importer manifests, PNG/JSON
  compilation, the queue drain, and runtime routing—not merely exported names.
- Mutable transfers classify every byte using the production DMA map and fixed
  bank/16-bit offset wrapping. A WRAM mirror that crosses into an unmapped window
  is not certified just because its first byte is RAM.

The initial run exposed ten native aliases missing from runtime dispatch: the
complete HUD upload, two escape-timer pages, and seven Grapple endpoint/segment
pages. Their typed transfers already worked. Native and restored queue records
now resolve the same current installed artwork, without ROM fallback, content
blobs in save states, or changes to timing/order/destinations.

## Scope and limits

This is finite compiled-definition coverage with guarded reviewed source
contracts, **not arbitrary interprocedural points-to or state reachability
analysis**. Sky arithmetic uses the compiled callback owners, physical room
dimensions and authored bottom-scroll alignment; inventing all values of the
camera mask would report ocean overreads beneath rooms that do not exist.
Out-of-room/corrupted camera states, edited scroll grids, invalid saved selectors,
and absent host bindings are not certified. The audit proves that admitted valid
descriptor domains have resolver coverage, not that a nullable runtime field is
always bound or that every edited/corrupted state selects a valid descriptor.

Source fingerprints are review guards, not auto-generated permission to skip
errors. Review changed production behavior and update the corresponding adapter
or ownership comparison before updating a fingerprint. Runtime failures remain
loud; this audit adds no catch-and-ignore path.

## Focused confirmation of the identified fixes

```powershell
dotnet run --project csharp/src/SuperMetroid.ResourceAudit -- --vram-dma-artwork-check "$env:LOCALAPPDATA/SuperMetroid/game/maps"
```

This optional focused check opens installed presentation assets, **not a ROM**,
and drains only the ten identified descriptors through the actual runtime and
RAM-only queue. It compares every byte with the corresponding typed upload,
rejects altered lengths, and confirms that an already-pending Grapple record uses
rebound PNG pixels. It is separate from the static audit and CI gate. No gameplay
search or whole-game validation is involved; player-facing confirmation remains
the user's responsibility.
