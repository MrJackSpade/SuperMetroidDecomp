# Kraid live floor investigation (#269)

Status: live-death crumble implemented; awaiting player confirmation. The player's
native death-state supplied the missing evidence. Earlier diagnostic sections below
are retained as history, not the current conclusion.

## Native-state finding and live-death fix (2026-09-11)

`issue-269-kraid-spike-destruction/SuperMetroid.001` has SHA256
`D8023E1C5D1EBEA0994204D4A612A5BCDA8A850F483997B8847276CF699700D4`.
Read-only inspection (`pwsh -File csharp/tools/inspect-kraid-spike-reference.ps1`)
finds Kraid room A59F and active PLM **B7BF**, instruction ABC2, with completed
0111/0110 floor pairs at x5..11, a crumble frame at x12, and spikes at x13..26.
This is snapshot evidence, not a replay or independent verification of ROM identity.

The earlier audit followed B7BB, the instantaneous defeated-room clear, and missed
the different **B7BF** spawn at **A7:C3F0** in death initialization. Both pinned C
and disassembly contain that call. The port omitted it and had no B7BF loader arm
or ABD6 move-right instruction. Restored that request and interpreter instruction;
the existing production PLM engine now reads the actual ABA9 ROM list. It executes
eleven two-block iterations, four three-frame draws per block, then deletes itself.
There is no hardcoded instant-clear workaround and no change to reload behavior.

Before the change, the real-room death capture failed the live floor assertion
at x5 (expected 0111, actual 2160); all 2816 visible spike-strip pixels persisted.
Afterward all 22 words match the exact list-derived animation cadence on every
frame (352 frames from first crumble through encounter completion), and zero old
pixels remain in the same 2816-pixel rectangle. Composed before/after frames were
visually inspected: the projecting spikes disappear, while the green floor below
remains. The complete death, door unlock, both exits, and defeated reload audit pass.
These tests use a retail runtime encounter with an input-locked observer and seeded
lethal damage, not a controller playthrough or full native-video comparison.

Ignored local captures: `csharp/test-temp/kraid-floor-269-native-reference-before`,
`kraid-floor-269-native-reference-after`, and `kraid-floor-269-cadence`.
Separate #268 upper-body validation remains excluded explicitly in floor mode.
No new state, ROM, screenshot, or generated capture is published by this fix.

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

## Observer correction and visible reproduction

The earlier bottom capture was invalid because moving Samus directly to (48,480)
could move the camera across multiple tilemap rows in one frame. Waiting afterward
did not refill those skipped rows. The observer now approaches its target one pixel
per frame before settling; no production camera or renderer code was changed.

`csharp/test-temp/kraid-floor-269-visible` is the corrected capture. At camera and
BG1 scroll (0,256), the isolated BG1 images show the spikes at screen x=80..255,
y=176..191 before and after death. All 2816 pixels in that region are unchanged.
The composed `death-0357-C843.png` also visibly retains the spike strip. The hazard
snapshot still contains all 22 blocks. This now reproduces the current port's
visible persistence, rather than merely its collision data.

The capture emits isolated BG1 images and logs scroll positions and the sampled
block's four character words. The floor block expands to $17A0/$17A1/$17B0/$17B1;
the Kraid room-background character upload alone does not establish its removal.
The native removal path and timing still require independent confirmation. No
production spike-clear call has been added speculatively.

The one-pixel observer approach also applies to the upper-body capture option;
older captures with different observer histories must not be treated as equivalent
frame-zero fixtures without checking their input/body trajectory.
