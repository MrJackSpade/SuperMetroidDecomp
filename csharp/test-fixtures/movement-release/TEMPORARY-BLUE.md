# Speedball to temporary Blue Suit (#470)

## Reproduction and fix

After controller-acquired Speedball, Up with angle held unmorphs to crouching.
At the first crouching movement frame (187), C# canceled the boost counter while
the native cartridge retained it. With aim bindings correctly initialized, the
128-case capture had 6,904 mismatching frames before the fix.

Native movement 90:A573 clears only the five horizontal velocity/acceleration
words at A57C..A588. C# used full collision cleanup, which additionally canceled
boost bookkeeping and echoes. Separating ClearHorizontalVelocity from complete
momentum cancellation restores that distinction. A second omission had been
hidden by that overbroad cleanup: crouching input lookup failure selects command
two (91:8304 / 91:ECD0), which stops acceleration and cancels boost after movement.
The runtime now applies that fallback only when no held-input table entry or
higher-priority transition won. Merely removing cancellation would incorrectly
keep boost after releasing angle. Both halves are required for native behavior.

The helper-level regression verifies movement preserves boost/momentum bookkeeping
while zeroing velocity; explicit fallback separately cancels it. Full-dispatcher
comparison verifies the actual input boundary, not just those helper endpoints.

## Controller fixture

Same tall room, equipment, standing initial conditions and sound publication as
[SPEEDBALL-SEQUENCE.md](SPEEDBALL-SEQUENCE.md). Run 64/80/96/112 frames, use a short
hop and second-Down timing 10/12/12/14 respectively, acquiring the earliest soft
landing. All fields are acquired through inputs, never injected. Both facings;
no enemies, liquid, gameplay cheats or RNG-dependent actors.

At frame 180 release forward/Jump and press Up for one frame. CSV speed selects:

| Mode | Angle input after frame 180 |
|---|---|
| 0 | None (negative control) |
| 1 | R / angle up |
| 2 | L / angle down |
| 3 | Both |
| 4..6 | Same as 1..3, released at frame 195 |
| 7 | R; additionally Dash+forward from frame 210 |

CSV inputMode 1 also holds Jump+forward from frame 210; zero stays stationary
except mode 7. Native bindings explicitly set R=0010 and L=0020 in addition to
Dash=8000, Jump=0080 and Shot=0040. An initial discarded capture omitted aim
bindings and therefore did not represent the intended native setup.

2 facings x 4 run-ups x 8 angle modes x 2 later-input choices x 300 frames =
128 cases / 38,400 samples. The main trace compares pose, position/subpixels,
base/extra velocity, Y speed/direction, acceleration, animation and boost word.

## Exact native boundaries

- Frame 179: moving ball with acquired extra speed 3.F000/4.F000/5.F000/6.F000
  and counter 0201/0301/0401/0401. Lower-stage controls are intentionally included.
- Frame 186: unmorph completes into crouch. Frame 187 clears numeric X velocity
  but keeps the acquired counter when angle is held; neutral mode cancels it.
- Frame 187 angle poses: right 71/73/85, mirrored 72/74/86; neutral 27/28.
  Grounded center is exactly Y496.FFFF.
- Modes 4..6 cancel on frame 195, the first released-angle frame, and stay canceled.
- Modes 1..3 preserve their counter through the stationary hold and optional jump
  for all sampled frames. The jump control is explicitly checked to leave the floor.
- Mode 7 without Jump cancels at frame 211 then begins ordinary running boost
  acquisition; it is not expected to remain zero forever. With Jump, it retains
  its counter rather than restarting the grounded run-up.

TemporaryBlueSequenceAssertions checks these boundaries independently of trace
equality. This completes #470's conversion dependency, not #429's broader
temporary-Blue-Suit carrying, repeated soft-unmorph and cancellation matrix.

## Capture and verification

Include native-release-probe.h then native-morph-bounce-probe.h after native
StateRecorder; dispatch DiagnosticTemporaryBlue before SDL. Hooks removed after
capture. No GUI, player SRAM or debugger slots are used.

temporary-blue-native-capture.zip contains accepted v2; independent repeat SHA256:
`EE43BE7FDD989E63F0E34F8E0AD65DCF46B83348B7791F28E7BF48527C6C0829`.

```powershell
dotnet run --project csharp/src/SuperMetroid.DebugRunner -c Release -- --temporary-blue-comparison-audit "Super Metroid.smc" path/to/temporary-blue-470-v2.csv
```

Pinned Japan/USA NTSC rev0 ROM SHA256:
12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72.
Native source 578f90b3cc49557bb70060ad033bb90b8cf8ac50; disassembly
362be646929cf8e483f692b73a6561cfc2dc1d0d. No PAL result is claimed.

The new 38,400-frame comparison passes, along with Speedball (120,000), Mockball
(67,200), crouch lock (19,200), and all three bounce matrices (84,480): 329,280
comparison frames. Both contact captures pass (162 native cases) with 96 vertical
checks. Full bank-$80 verification passes, including the new helper regression.
#470 remains open for player confirmation, with awaiting-player-validation applied.
