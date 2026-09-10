# Arm pumping (#452)

`arm-pump-native-capture.zip` contains `arm-pump-452-v2.csv`, SHA-256
`99556B48C8A16E32548FD7BDDBE3D3B42029ED8A2C0D8A8DF38D67B2608E1134`.
Independent recapture has the identical hash. Run:

```
SuperMetroid.DebugRunner --arm-pump-comparison-audit "Super Metroid.smc" arm-pump-452-v2.csv
```

## Reproduction and root cause

The [technique reference](https://wiki.supermetroid.run/Arm_Pumping) describes
one-pixel shifts on grounded arm transitions and loss of support on slopes.
Pinned disassembly `CheckIfProspectivePoseRunsIntoAWall` ($91:EADE) confirms
that the prospective-running-pose check directly calls $94:971E at $91:EB48.
Unlike ordinary bank-$90 movement, it does not call $94:87F4 to align Y afterward.

The port's shared horizontal collision method always performed that alignment.
That erased the transient vertical position produced by the pure horizontal
arm-pump step. The production API now distinguishes whether the caller owns
the post-movement alignment; the prospective-pose caller disables it. Ordinary
movement retains its existing alignment and collision/PLM behavior.

The initial native fixture omitted startup's slope-enable word and produced
2,295 mismatches; that was NOT valid production evidence. After seeding the
normal startup value three, the accepted v2 capture reproduced 199 differing
frames. The production correction reduces those to zero across 14,400 frames.

## Matrix

120 fresh cases: both facings, uncharged/held Charge Beam, five terrains,
and six input patterns. Each runs 120 frames through native alpha, movement,
animation, projectile and pose-transition stages. C# uses the complete runtime
StepFrame path on identical constructed room data, not a handwritten formula.

- Terrain: flat; flat with jump held from frame 55; repeated floor slopes with
  BTS $12 and $1B (mirrored for left travel); blocking wall.
- Patterns from frame 60: no pump, R/neutral, L/neutral, R/L, Shoot/neutral,
  and both shoulders/neutral. Each phase lasts four frames.
- Hold direction from frame 40. Charged cases hold Shoot from frame zero;
  no Speed Booster, cheats, liquids, enemies, SRAM or player recordings.
- Standing start X=1024, Y=235, health/max=99; room 144x80 blocks, floor row16.
  Slopes replace floor columns >66 (right) or <61 (left). Walls occupy column
  70 or 57. This is synthetic scalloped geometry, not a claimed retail route.
- Compare exact X/Y including fractions, pose/movement, animation frame/timer,
  base/extra velocity, acceleration mode, facing, Y velocity/direction and charge.
  Hash, dimensions, case order and every input are checked.

For the uncharged right-facing flat control, R at frame60 advances X by one
extra pixel; release at frame64 adds a second. Holding the same angle on the
intervening frames adds nothing. Fifteen transitions yield fifteen pixels by
frame119. Shoot animation does not necessarily return to neutral immediately
on release, so button edges alone are not counted as arm pumps. Airborne,
wall-blocked and slope cases are compared frame-for-frame as well.

## Capture

Use `native-arm-pump-entrypoint.patch` in pinned upstream-sm with
`git apply --unidiff-zero`, build Release x64, and run:

```
sm.exe --diagnostic-arm-pump "Super Metroid.smc" arm-pump-452-v2.csv
```

Reverse the patch afterward. This dispatches before SDL, suppresses explicit
GUI error dialogs, and runs the original restored ROM bytes under the bounded
CPU helper. The archive contains numeric diagnostics only.

Pins: Japan/USA rev0 ROM SHA-256
`12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72`;
upstream-sm `578f90b3cc49557bb70060ad033bb90b8cf8ac50`;
disassembly `362be646929cf8e483f692b73a6561cfc2dc1d0d`.
No PAL, renderer, complete retail slope route or every input cadence claim.
Release DebugRunner build and the full core verification suite pass.
Player confirmation remains required before closing #452.
