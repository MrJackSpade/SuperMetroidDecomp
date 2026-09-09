# Crouch-lock parity (#451)

Status: reproduced on the pinned cartridge CPU and matched by the existing C#
implementation. No production gameplay change is required; awaiting player
validation. Do not remove this native input quirk as if it were host input loss.

## Reproduction

The [technique reference](https://wiki.supermetroid.run/Crouch_Lock) describes
opposite-direction input being ignored during unmorphing or stand-to-crouch.
The fixture tests both paths, both facings, and the actual animation boundary.

Construct a 144-by-80-block room with empty space over a solid floor at block row
16. No enemies, liquid, or gameplay cheats. Morph Ball is equipped; health is
99. Initial X is 1024, all subpositions and speeds zero, animation frame zero /
timer one. Standing cases start pose $01/$02 at Y235; ball cases start $1D/$41
at Y249. Previous pose/direction/movement match the initial pose; last-different
history is zero. The movement-only experiment has no RNG-dependent actors.

At frame 16, press Down to crouch or Up to unmorph, for one frame. Opposite
direction begins at frame 16+offset, with offset swept from -8 through +16,
inclusive. Hold it thereafter. A second variant releases direction only on
frame 48 and re-presses it on frame 49. All inputs go through the ordinary
controller, pose, movement, animation, collision, and transition schedule.

The 2 facings x 2 initial forms x 25 offsets x 2 release modes x 96 frames
produce 200 cases / 19,200 samples. The capture compares X/Y with subpixels,
pose, movement type, animation frame/timer, base/extra speed, acceleration mode,
and facing on every frame, not merely the eventual position.

## Native result and explicit assertions

- Standing-to-crouch locks for offsets 0..3. Offset -1 and offset 4 avoid it.
- Unmorphing locks for offsets 0..6. Offset -1 and offset 7 avoid it.
- These windows include the animation completion frame: input is processed
  before the animation changes movement type.
- Locked cases remain at X1024.0000 / Y240.FFFF, original-facing crouch pose
  $27/$28, zero base speed, despite continuously held opposite direction.
- Releasing on frame 48 and re-pressing on 49 admits crouching turn $43/$44.
  Subsequent movement goes in the requested direction. Holding without a new
  edge remains locked for the entire capture.
- Turning before the transition or after its input-discard window allows
  movement. The audit checks the actual locked intervals, exact first recovery
  pose, and eventual direction, in addition to the full per-frame trace.

All 19,200 samples match without a production change. The first comparison was
already equal; the explicit assertions were then added to prove that the test
actually exercises lock, adjacent non-lock, and recovery cases.

## Cartridge explanation

Pinned bank $91 dispatches posture transitions (movement type $0F) to RTS at
$91:8146, so they do not buffer the opposite-direction edge. Once crouched,
$91:A66C/$A6BC require a **new** opposite-direction press to select turn
$43/$44. An already held direction therefore cannot start that turn. This is
the original state machine and transition table, not a separate lock flag.

## Capture and replay

Include `native-release-probe.h` then `native-crouch-lock-probe.h` after the
`StateRecorder` declaration in `sm_rtl.c`. Temporarily dispatch
`DiagnosticCrouchLock(romPath, newCsvPath)` before SDL initialization. The loader
restores the untouched ROM bytes after native harness initialization, and output
creation refuses to overwrite an existing file. Remove the temporary hooks
after capture. Player SRAM and debugger slots are not used or changed.

`crouch-lock-native-capture.zip` preserves the native CSV, independently repeated
with identical SHA256:
`CB7F3C834C9BA23B62D68769E3873A1F20D5F8667403E33F47D326B006681507`.

```powershell
dotnet run --project csharp/src/SuperMetroid.DebugRunner -c Release -- --crouch-lock-comparison-audit "Super Metroid.smc" path/to/crouch-lock-451.csv
```

ROM: Japan/USA NTSC revision zero, SHA256
`12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72`.
Native source: `578f90b3cc49557bb70060ad033bb90b8cf8ac50`.
Disassembly: `362be646929cf8e483f692b73a6561cfc2dc1d0d`.
This does not claim PAL timing or alter unrelated crouched combat/shinespark
behavior; it tests the native posture-input mechanism used by those situations.
