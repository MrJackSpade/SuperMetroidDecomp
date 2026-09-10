# Edge/corner boosts from hitbox expansion (#464)

## Independent reference

Original Japan/USA revision-zero ROM SHA-256:
`12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72`.
Native host pin `578f90b3cc49557bb70060ad033bb90b8cf8ac50`;
InsaneFirebat disassembly pin `362be646929cf8e483f692b73a6561cfc2dc1d0d`.

[Hitbox Manipulation](https://wiki.supermetroid.run/Hitbox_Manipulation), revision
10438, describes downward correction when expanding beneath an edge, and why
fast falling can miss it. Original CPU execution confirms this behavior.
Local `bank_91.asm`, `PoseChangeCollision_Block_FromAbove` at $91:FF20-$FF48,
checks available downward space before adding the correction to Samus's center.
This is the existing production `ResolveLargerPoseCollision` path, not a new
technique-specific permission to pass through blocks.

## Reproduction matrix

1,296 independent 96-frame cases, 124,416 frames total:

- Both facings; falling down-aim and falling spin poses.
- Initial vertical speeds zero, one and five pixels/frame, direction down.
- Ledge present/absent twins, identical otherwise.
- Initial gap below the compressed hitbox: zero through eight pixels.
- Expansion on frames zero through four, plus a no-expansion control.

Synthetic room is 144x80 blocks, solid floor row 48 (top Y768), everything else
air except the optional ledge at row31/column63 when facing left, column64 when
facing right. Its bottom boundary is Y512. Samus starts X1028/1020 respectively,
one pixel of her radius-five body horizontally beneath the edge. Initial Y is
512 + initial radius + gap: down-aim radius10, spin radius12. Subpositions and
horizontal momentum zero; matching previous pose/type/direction; animation
frame0/timer1. Morph Ball only, health99, no beams/liquids/enemies/PLMs/cheats
or RNG-dependent actors.

Down is already held for the down-aim seed; Jump is already held for spin.
Expanding down-aim presses the facing direction for four frames then releases
it. Expanding spin holds angle-up with Jump. All expansions use the original
controller/pose dispatcher, not direct calls to a collision-correction helper.
The no-expansion control keeps the original held input throughout.

Native order: alpha, interactive-enemy list, movement, animation, interruption,
block/pose dispatch, gamma, feet adjustment and enemy timers. Managed comparison
uses production runtime frames. Every frame compares fixed X/Y, pose/type,
animation/timer, horizontal speed/extra speed, acceleration/facing, vertical
speed/direction, charge counter and active movement radii. As in CROUCH-JUMP.md,
radii are sampled before movement rather than after native scratch restoration.

## Measured results and explicit regression assertions

All 124,416 frames agree. No production correction was needed.

- Immediate zero-gap, zero-speed expansion: down-aim center moves from Y522
  to Y531 (nine pixels), spin from Y524 to Y531 (seven pixels). Empty-space
  controls retain Y522/Y524 instead. Both facings agree.
- At one pixel/frame, corresponding corrections are eight and six pixels.
- At five pixels/frame, immediate expansion still gains four/two pixels, but
  waiting one extra frame loses the boost: ledge/empty twins both reach Y532
  for down-aim or Y534 for spin. This is an adjacent successful/failed timing,
  not a comparison between unrelated setups.
- Every frame asserts that the body bottom does not pass the floor. Every case
  ends settled on that floor with zero vertical direction. Downward correction
  beneath the upper edge is therefore distinguished from floor penetration.

The first draft accidentally treated the initial Down as a new press, causing
delayed cases to morph. Explicit timing assertions exposed that fixture error.
Version2 supplies the held-input history appropriate to its seeded posture;
the accepted capture tests delayed down-aim expansion itself. Production code
was not changed to accommodate the fixture.

## Re-run

Two independent captures have identical SHA-256:
`83C89AFB4D0A7CC4521E4904D9F2909642104C3E8C72414FA3977697FB8FBFA7`.
Accepted CSV is in `edge-boost-native-capture.zip`.

```powershell
dotnet run --project csharp/src/SuperMetroid.DebugRunner -c Release -- --edge-boost-audit 'Super Metroid.smc' PATH/edge-boost-464-v2.csv
```

For native recapture, apply `native-edge-boost-entrypoint.patch` in upstream-sm,
rebuild, and invoke `--diagnostic-edge-boost ROM NEW.csv`. The explicit headless
entrypoint suppresses SDL error/warning dialogs and uses instruction-bounded
original CPU execution. Temporary source hooks were removed; saved patch passes
apply --check. No player saves/SRAM were used. This establishes the requested
pinned-revision technique in room-local geometry, not every hitbox exploit or
ROM revision. Build passes with zero warnings/errors. #464 remains open awaiting
player validation.
