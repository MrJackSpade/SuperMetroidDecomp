# Temporary boost through ball bounces

Extends [retention](README.md) and [soft unmorph carry](CARRY.md); partial #429.
Uses the same synthetic floor, native CPU adapter, starting state and ROM revision.
Boost is earned through the preceding run/crouch sequence, never injected.

## Inputs and evidence

Both facings run 800 frames for eight modes. Through frame422 inputs match carry
mode4: store charge, let its timer expire while holding angle, jump and morph.
Modes0..3 then respectively hold Jump+forward, forward alone, Jump+forward until
480 then Jump alone, or forward until500 then Jump+forward. Modes4..7 repeat with
Spring Ball equipped from initialization. The CSV `aim` column identifies mode.

Native boost first disappears on518 for modes0/1/5,480 for2/6,508 for3, and never
within this sample for4/7. Modes4/7 finish airborne in the falling Spring Ball pose
with boost0401. Semantic assertions enforce these boundaries as well as exact
position, subpixels, pose, animation, velocity, boost/contact and charge fields.

16 cases / 12,800 frames match. An independent repeated capture has normalized
SHA-256 `1A930ABCDBD78434CC0CA6220F512E2DAC139AEB939C3A82BC0F38472A446296`.
Only numeric CPU traces are included, not ROM bytes or player states.

## Reproduced fixes

1. Held-Jump Spring Ball landing changed pose to jumping in the port on frame493;
   native retained the falling pose. Pinned `sm_91.c`'s
   `Samus_MorphBallBounceSpringballTrans` ($91:F25E) only clears bounce state and
   initializes jump velocity on this branch. Removed the extra pose/animation
   change; fresh grounded jumps still use their separate transition.
2. With that fixed, mode5 frame517 retained base speed in the port, in both
   facings. Native's common landing caller `Samus_HandleTransitionsA_5_1`
   ($91:F010) clears base speed/subspeed and acceleration when bounce carry ends.
   Ordinary Morph Ball already did this; settled Spring Ball landing now does too.

Focused tests assert retained pose/animation and cleared settled momentum for
both ball families. The exhaustive signed-speed/phase/facing test also checks
the common momentum and acceleration reset rather than preserving the omission.

## Reproduce

Build the native tool per README.md, then:

```powershell
cmd /c 'csharp\native\TemporaryBlueSuitAudit\audit.exe "Super Metroid.smc" bounce > csharp\test-temp\temporary-blue-bounce.csv'
dotnet run --project csharp/src/SuperMetroid.DebugRunner -c Release -- --temporary-blue-bounce-audit 'Super Metroid.smc' csharp/test-fixtures/issue-429-temporary-blue-suit/bounce.csv
```

This does not complete repeated full soft-unmorph chains, terrain damage,
sand/equipment/X-Ray cancellation, or persistent Blue Suit contrasts. #429 remains
open; the earlier carry document describes its scope at the time of that capture.
