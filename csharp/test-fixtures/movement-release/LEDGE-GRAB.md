# Early landing, ledgegrabs and downgrabs (#459)

## Independent reference

Original Japan/USA revision-zero ROM SHA-256:
`12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72`.
Native host `578f90b3cc49557bb70060ad033bb90b8cf8ac50`;
InsaneFirebat disassembly `362be646929cf8e483f692b73a6561cfc2dc1d0d`.

[Hitbox Manipulation](https://wiki.supermetroid.run/Hitbox_Manipulation), revision
10438, describes early landing by expansion, spin ledgegrabs and down-aim grabs
at higher platforms. The following expectations come from original CPU execution,
not from assuming the wiki or current C# behavior is correct.

## Reproduction

1,728 independent 128-frame cases, 221,184 frames. Both facings, down-aim/spin,
falling/rising approaches, vertical speed zero/one/three, nine starting heights,
and expansion on frames zero through six plus a no-expansion control.

Synthetic 144x80 room, floor row48/topY768, one platform at row32/topY512,
column63 facing left or column64 facing right. Everything else is air. No enemies,
PLMs, liquids, beams, cheats or RNG-dependent actors. Health99, Morph Ball only.
Initial subpositions zero, matching previous pose/movement/direction, animation
frame0/timer1. Exact initialization is in the native probe and managed audit.

- Falling: X1028/1020 (left/right), one-pixel overlap beneath the platform edge;
  Y512 - compressed radius - gap. Zero horizontal momentum, downward direction.
- Rising: X1029/1019, immediately beside the platform; Y512 - radius + gap - 4.
  Upward direction, base horizontal speed2, acceleration mode2. This includes
  starts with feet below platform height, requiring the compact body to clear
  the side before expanding. Down-aim uses jump poses, not falling poses.

Down is already held in down-aim seeds; Jump is already held in spin and rising
seeds. Expanding down-aim presses the facing direction for four frames; rising
cases retain Jump. Spin expansion adds angle-up. All input is released at frame24.
All transitions run controller tables and full production runtime frames rather
than invoking pose-correction helpers directly. The no-expansion controls also
exercise the native short-held-Jump restart after landing.

Original CPU order: alpha, interactive-enemy list, movement, animation,
interruption, block/pose dispatch, gamma, feet adjustment, enemy timers. Compared
every frame: fixed X/Y, pose/type, animation/timer, horizontal momentum/mode,
facing, vertical speed/direction, charge counter and active movement radii.
Radius sampling uses the same movement boundary described in CROUCH-JUMP.md.

## Reproduced defects and production changes

Baseline: 18,786 mismatching frames.

1. Same-family normal-jump aim changes omitted $91:F543's acceleration-mode
   initialization; only falling targets received $91:F60D's equivalent behavior.
   The port retained mode2 after expanding from down-aim, altering subsequent
   horizontal travel. Both compact and equal-radius aim transitions now reuse
   `InitializeOrdinaryAerialAcceleration`. It preserves base speed and selects
   mode using extra dash speed. Removed the duplicate falling-only helper.
2. Compact down-aim landings copied the velocity cleanup but omitted $91:F1EC's
   one-shot held-Jump input handler. They now reuse the existing ordinary landing
   collision command, including that deferred restart. No new input shortcut was
   added. Native witness: compact landing on frame8, next-frame jump pose $4B/$4C
   on frame9; the old port remained in landing art instead.

Local disassembly/native source confirms $91:F543/$F60D, shared landing command
$91:F010/$F1EC and the upward position correction in $91:FF49-$FF75. Original
CPU trajectories now match in all 221,184 frames.

## Named mechanical assertions

- Falling, speed3, gap8: expanding immediately lands on frame1, while no
  expansion or expansion one frame later lands on frame2, for both pose families
  and both facings.
- Ascending down-aim, speed3, gap8: expansion on frame1 misses the upper platform;
  waiting one more frame successfully clears its edge and lands there on frame49.
  This is an adjacent too-early failure/success pair in identical geometry.
- First upper-platform landing must have center `01EB.FFFF` and radius21.
  Every frame must keep the body above the lower floor boundary.
- Normal-jump expansion must retain measured base speed `0001.4000` while
  resetting mode to ordinary acceleration.
- The compact landing/re-jump witness checks frame8 landing and frame9 upward
  jump transition, not merely eventual airborne motion.

## Verification and replay

Two independent original-CPU captures have identical SHA-256:
`27BF3ACB883C55E2D29A788BBB5AB95DA027CC4CF064EF90CDE43B8A28512148`.
CSV is archived in `ledge-grab-native-capture.zip`.

```powershell
dotnet run --project csharp/src/SuperMetroid.DebugRunner -c Release -- --ledge-grab-audit 'Super Metroid.smc' PATH/ledge-grab-459-v1.csv
```

Apply `native-ledge-grab-entrypoint.patch` inside upstream-sm and rebuild before
recapturing with `--diagnostic-ledge-grab ROM NEW.csv`. Explicit headless mode
suppresses both SDL dialog paths; CPU calls have instruction budgets. Temporary
source hooks were removed and the saved patch passes apply --check. No player
save or SRAM files were touched.

Build passes without warnings/errors; full core verification passes. Ledgegrab,
edge-boost, crouch-jump, Kago passage, Quick Drop timeline/bomb and arm-pump audits
total 581,440 zero-mismatch frames after the fixes. This is pinned-revision,
room-local evidence, not a whole-game route or a claim about other revisions.
Issue #459 remains open awaiting player validation.
