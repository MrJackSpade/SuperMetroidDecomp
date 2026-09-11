# Kraid live floor investigation (#269)

Status: unresolved. The player's 2026-09-11 live-ROM observation supersedes the
earlier unsupported claim that visible spikes should remain until room reload.
No production fix is included in this diagnostic update.

## Why the earlier audit was insufficient

`VerifyLiveDefeatRetainsSpikes` required one level word at (5,27) to remain unchanged
and the reload clear PLM not to be present. It inferred rendered visibility from
those facts and from the absence of a direct call to Kraid_ClearSomeSpikes in the
death coroutine. Neither establishes the complete cartridge visual lifecycle.
That assertion is now a diagnostic report, not a requirement that a future fix
must preserve the reported defect. The actual defeated-room reload assertions
remain intact.

## Current reproduction tooling

DebugRunner:

`--kraid-floor-death-capture ROM OUTPUT_DIRECTORY`

Uses the existing retail encounter, real projectile-hit seam and seeded lethal HP.
It positions an input-locked observer at (48,480), then lets ordinary camera
scrolling stream the crossed rows. Death updates and rendering are production
paths. This is an observation fixture, not a controller-driven encounter.
The normal upper-body capture and native arm trace path remain unchanged.

PNG checkpoints, every-frame arm/camera state, and before/after hazard CSVs stay in
the requested local directory. The hazard scan records every SpikeAir/SpikeBlock,
not just the block at (5,27). Current output contains 22 floor hazards at y=27,
x=5..26, alternating words $2160/$2161 with BTS $02, before and after death.

The floor-specific command excludes the separate #268 upper-body pixel assertion
so it can reach death. That assertion remains enabled in the normal audit and
currently fails with 5002/7675 differing pixels. Exclusion is printed explicitly;
passing this diagnostic does not establish #268 or #269 visual correctness.

Current local captures:

- `csharp/test-temp/kraid-floor-269-final`
- Earlier observer comparison: `csharp/test-temp/kraid-floor-269-current`
  and `csharp/test-temp/kraid-floor-269-bottom`

The bottom capture reaches camera (0,256), but its visible floor does not yet
provide a faithful before/after depiction of the spikes identified by the hazard
scan. The offset observer shows a small spike strip at the screen edge. Resolve
that level-to-viewport correspondence and compare the native rendered sequence
before modifying production clearing behavior. Do not claim exact visual repro
or mark the issue awaiting validation from these captures.

Verification: floor capture completes the death sequence, publishes both hazard
snapshots, and keeps the existing reload and encounter checks. No ROM, images,
state fixtures or generated traces are included in the public commit.
