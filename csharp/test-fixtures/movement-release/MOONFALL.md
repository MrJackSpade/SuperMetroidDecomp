# Moonfall parity (#468)

## Accepted evidence

`moonfall-native-capture.zip` contains `moonfall-468-v6.csv`, SHA-256
`FAE378FC7BCE200B7815948D82EF94EB2C61BA484574C79F5FBBA2347BBD11ED`.
Two independent native CPU runs produced identical bytes. The comparator rejects
exploratory captures; earlier revisions had shorter geometry, insufficient morph
inputs, or did not actually buffer Jump.

ROM: Japan/USA revision 0, SHA-256
`12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72`.
Pinned upstream-sm: `578f90b3cc49557bb70060ad033bb90b8cf8ac50`.
Pinned disassembly: `362be646929cf8e483f692b73a6561cfc2dc1d0d`.
The bank-$90 comment at 90E2 explicitly explains the direction-none/negative-speed
mechanism; the capture executes the ROM CPU, not the translated-C implementation.
Technique reference: https://wiki.supermetroid.run/Moonwalk#Moonfall.

## Reproduction

Include native-release-probe.h and then native-moonfall-probe.h after StateRecorder
in upstream-sm/src/sm_rtl.c. Add a pre-SDL main dispatch to
`DiagnosticMoonfall(argv[2], argv[3])` for `--moonfall ROM CSV`. Build, capture twice
to distinct paths, and remove the temporary hooks. The helper restores original
ROM bytes, suppresses native fault dialogs, and never uses player saves or a GUI.

Extract the accepted archive and run:

```
dotnet run --project csharp/src/SuperMetroid.DebugRunner -c Release -- --moonfall-comparison-audit "Super Metroid.smc" PATH/moonfall-468-v6.csv
```

## Exact fixture and assertions

156 cases x 240 frames = 37,440 comparisons: both facings, Moonwalk on/off,
air / water without Gravity / water with Gravity, and thirteen input scenarios.
The C# fixture loads Climb only to obtain its 48x144 allocation, then replaces
all collision/BTS data with the same constructed shaft as the CPU probe.
There are no enemies, cheats, injected Moonfall poses or injected velocities.
Morph Ball is equipped. The initial position is (367,235) facing right or
(400,235) facing left; the edge control uses x=364/403. Subpositions start zero.
The shelf is row 16; the six bottom rows starting at 138 are solid. Thin-floor
controls add just row 125 or 126. The thick bottom bounds high-speed passage.

| Scenario | Inputs / purpose |
| --- | --- |
| 0 | Aim down + Shoot, backward at frame 10, Jump at 11 |
| 1 | Same, release angle at 12 for spinning Moonfall |
| 2 | Jump already held at fixture boundary; release Shoot at 11 to buffer entry |
| 3 | Nonspinning reversal at 60 and 70 |
| 4 | Spinning reversal at the same frames |
| 5 | Three Down pulses starting at 60, then remain morphed through landing |
| 6 | Same morph inputs, Up at 90 to cancel by unmorphing |
| 7 | Thin solid row 125: native skips it at high speed |
| 8 | Three pixels closer to the edge: adjacent entry/failure control |
| 9 | Thin solid row 126: native lands on it instead |
| 10–12 | Repeat ordinary, spinning and buffered entry with aim-up |

Buffered cases seed only the previous accepted controller word with held Jump,
representing a grounded boundary with no new Jump edge. All subsequent state
changes come from controller input. This proves the release-Shoot buffered entry,
not a full route from a previous room or elevator.

Both harnesses execute full alpha, projectile interaction, beta movement and
animation, pose resolution, and the gameplay timer tail. NMI counters start at
one and advance before each frame. Every frame compares X/Y including subpixels,
pose/type/facing, animation frame/timer, base/extra horizontal speed and mode,
vertical speed/direction, and bounce state. Additional assertions require the
uncapped negative-speed state, unmorph cancellation, no morphed landing bounce,
and different final heights for the neighboring tile-passage controls.

Air/Gravity ordinary Moonfall reaches a negative whole speed below -18 before
landing. The same unmodified collision code skips row 125 but hits row 126.
Turning preserves its negative velocity while the no-speed probe runs; spinning
reversal keeps gravity advancing. Water remains slower, as the ROM dictates.

## Reproduced defects and fixes

1. Aimed entry threw on `$78 -> $BF`: input tables request an un-aimed turn, then
   F8D3 substitutes the old muzzle's aimed turn. Admit the input request and apply
   the resolved target instead of requiring the caller to have resolved it already.
2. Animation completion incorrectly called InitializeJump. Super-special selection
   runs F433 but skips F404's later FBBB/FC99 jump initialization. Preserve the old
   direction/speed; ordinary input-table jumps still initialize normally.
3. Aerial turning rejected direction zero. A790/A7AD use the shared no-speed Y
   probe in this state; reuse that production routine instead of throwing.
4. The runtime's walk-off filter included Moonwalk turn art despite the native
   no-pose-change table entry. Remove that inclusion; turning finishes its animation.
5. Ball landing compared speed unsigned. Native tests the sign of speed minus
   three. Use word-subtraction semantics for Morph and Spring Ball, preventing
   Moonfall's negative speed from being mistaken for a bounce-producing fall.
6. Generic turns omitted F8F9's held-Jump check. Pass current input and share the
   existing Moonwalk turn/jump initializer for buffered Shoot-release entry.

The earlier synthetic animation test asserted the invented jump velocity. It now
asserts preserved zero speed/direction, based on the independently captured CPU
transition. No special Moonfall flag, speed override, collision bypass, or clamp
was added. Player validation remains required after automated verification.

## Verification at handoff

- Accepted Moonfall capture: 37,440 frames, zero mismatches; independent repeat identical.
- Existing Moonwalk capture: 20,160 frames, zero mismatches.
- Original bomb-chain/hurt matrices: 468,160 frames, zero mismatches.
- All 97 accepted damage-boost captures: zero mismatches.
- Morph-bounce, Mockball, Speedball sequence and Speedball family matrices pass.
- Full core Verification passes, including ten focused Morph/Spring Ball signed
  threshold checks at the ordinary threshold, negative velocity, and word-wrap boundary.
